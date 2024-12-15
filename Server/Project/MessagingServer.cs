using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Messanger_Server
{
    class MessagingServer
    {
        private TcpListener _listener;
        private RSA _serverRSA;
        private Dictionary<string, ClientData> _clients = new Dictionary<string, ClientData>();
        private Aes _aes;

        public MessagingServer()
        {
            _serverRSA = RSA.Create();
            _aes = Aes.Create();
        }

        public void StartServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            string localIP = GetLocalIPAddress();
            Console.WriteLine("Сервер запущен на: " + localIP + ":" + port.ToString() + ".");

            while (true)
            {
                TcpClient client = _listener.AcceptTcpClient();
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                Console.WriteLine("Новое подключение от: " + clientIP);
                NetworkStream stream = client.GetStream();


                // Получаем никнейм клиента
                byte[] nicknameBuffer = new byte[256];
                int nicknameBytesRead = stream.Read(nicknameBuffer, 0, nicknameBuffer.Length);
                string nickname = Encoding.UTF8.GetString(nicknameBuffer, 0, nicknameBytesRead);

                // Получаем открытый ключ клиента
                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string clientPublicKeyString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                RSAParameters clientPublicKey = ConvertFromPublicKeyString(clientPublicKeyString);


                _clients[clientIP] = new ClientData
                {
                    Stream = stream,
                    PublicKey = clientPublicKey,
                    Nickname = nickname
                };



                // Отправляем открытый ключ сервера клиенту
                string publicKeyString = ConvertToPublicKeyString(_serverRSA.ExportParameters(false));
                byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKeyString);
                stream.Write(publicKeyBytes, 0, publicKeyBytes.Length);

                Task.Run(() => HandleClient(client, clientIP));
            }
        }


        private async Task HandleClient(TcpClient client, string clientIP)
        {
            byte[] buffer = new byte[4096];
            ClientData clientData = _clients[clientIP];
            try
            {
                while (client.Connected)
                {
                    int bytesRead = await clientData.Stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"Клиент {clientIP} отключился.");
                        _clients.Remove(clientIP);
                        break;
                    }
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessReceivedData(receivedData, clientIP);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка обработки клиента {clientIP}: {e.Message}");
                _clients.Remove(clientIP);
            }
            finally
            {
                client.Close();
            }
        }


        void ProcessReceivedData(string data, string clientIP)
        {
            string[] parts = data.Split('|');
            if (parts.Length != 2)
            {
                Console.WriteLine("Invalid data format.");
                return;
            }

            string encryptedMessageData = parts[0];
            string signature = parts[1];
            string decryptedMessage = "";
            try
            {
                decryptedMessage = DecryptMessage(encryptedMessageData, clientIP);


                if (!VerifySignature(decryptedMessage, signature, _clients[clientIP].PublicKey))
                {
                    Console.WriteLine($"Signature verification failed from {clientIP}.");
                    return;
                }
                Console.WriteLine($"Received from {clientIP} ({_clients[clientIP].Nickname}): {decryptedMessage}");

            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка обработки сообщения от {clientIP}: {e.Message}");
                return;
            }


            BroadcastMessage(decryptedMessage, clientIP);

        }

        string DecryptMessage(string encryptedMessageData, string clientIP)
        {
            string[] parts = encryptedMessageData.Split(':');
            if (parts.Length != 3)
            {
                throw new Exception("Invalid encrypted message format.");
            }

            byte[] encryptedKey = Convert.FromBase64String(parts[0]);
            byte[] encryptedIV = Convert.FromBase64String(parts[1]);
            byte[] encryptedMessage = Convert.FromBase64String(parts[2]);

            // Дешифруем ключ и IV с помощью RSA
            byte[] aesKey = _serverRSA.Decrypt(encryptedKey, RSAEncryptionPadding.Pkcs1);
            byte[] aesIV = _serverRSA.Decrypt(encryptedIV, RSAEncryptionPadding.Pkcs1);

            // Создаем AES с полученными ключом и IV
            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.IV = aesIV;

                // Дешифруем сообщение с помощью AES
                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedMessage = decryptor.TransformFinalBlock(encryptedMessage, 0, encryptedMessage.Length);
                    return Encoding.UTF8.GetString(decryptedMessage);
                }
            }
        }

        bool VerifySignature(string message, string signature, RSAParameters publicKey)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(message);
            byte[] hashBytes = SHA256.Create().ComputeHash(dataBytes);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(publicKey);
                try
                {
                    return rsa.VerifyHash(hashBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка верификации подписи: " + e.Message);
                    return false;
                }

            }
        }

        void BroadcastMessage(string message, string senderIP)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            foreach (var client in _clients)
            {
                if (client.Key != senderIP)
                {
                    try
                    {
                        client.Value.Stream.Write(messageBytes, 0, messageBytes.Length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Ошибка при отправке сообщения {client.Key}: {e.Message}");
                    }
                }
            }
        }

        string ConvertToPublicKeyString(RSAParameters publicKey)
        {
            return Convert.ToBase64String(publicKey.Modulus) + "|" + Convert.ToBase64String(publicKey.Exponent);
        }

        RSAParameters ConvertFromPublicKeyString(string publicKeyString)
        {
            string[] parts = publicKeyString.Split('|');
            return new RSAParameters
            {
                Modulus = Convert.FromBase64String(parts[0]),
                Exponent = Convert.FromBase64String(parts[1])
            };
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        class ClientData
        {
            public NetworkStream Stream { get; set; }
            public RSAParameters PublicKey { get; set; }
            public string Nickname { get; set; }
        }

        static void Main(string[] args)
        {
            int port = 0;
            MessagingServer server = new MessagingServer();
            Console.WriteLine("Введите порт: ");
            port = Convert.ToInt32(Console.ReadLine());
            server.StartServer(port);
        }
    }
}
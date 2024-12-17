#EN
<br />
> [!WARNING]
> The server is running on .NET 5. If it is not present, then the server simply will not start. You can copy the server code to Visual Studio with any newer version.NET 5
> !! THE APPLICATION WORKS ONLY ON A LOCAL NETWORK !!
> !! THE `Project` FOLDER CONTAINS THE SOURCE PROJECT (`Messenger\App\Project` - asset for unity; `Messenger\Server\Project` - Visual Studio C# console application)!!

1. To launch the application, first start the server on one of the local computers: Messenger\Server\Server\Messanger Server.exe
- Enter any port into the server console. For example: 5000.
- - The server will output a message in the format: {ipv4}:{port}.

2. Now let's launch the application: `Messenger\App\App\Messenger (Lab_07).exe`
- Enter a nickname. If you leave the field empty, the application will fill it in with the nickname itself.
- - In the application, enter the ipv4 server and port.
- Click the "Connect" button.
- Upon successful connection, a message will appear: "The public key from the server has been received."

3. Send messages by pressing the `Enter` button on the keyboard or `↑` next to the message input field.

4. Now you can chat with everyone who is connected to the server!
<br />
#RU
<br />

> [!WARNING]
> Сервер работает на .NET 5. Если его нет, то сервер просто не запустится. Можете скопировать код сервера в Visual Studio с любой версией новее .NET 5
> !! ПРИЛОЖЕНИЕ РАБОТАЕТ ТОЛЬКО ПО ЛОКАЛЬНОЙ СЕТИ !!
> !! В ПАПКЕ `Project` НАХОДИТСЯ ИСХОДНЫЙ ПРОЕКТ (`Messanger\App\Project` - ассет для unity; `Messanger\Server\Projec`t - консольное приложение Visual Studio C#) !!

1. Для запуска приложения сначала запусти на одном из локальных компьютеров сервер: `Messanger\Server\Server\Messanger Server.exe`
- В консоль сервера впиши любой порт. К примеру: 5000.
- Сервер выдаст сообщение формата: {ip-v4}:{порт}.

2. Теперь запусти приложение: `Messanger\App\App\Messanger (Lab_07).exe`
- Введи никнейм. Если оставить поле пустым, то приложение заполнит его сам никнейном.
- В приложении впиши ip-v4 сервера и порт.
- Нажми кнопку "Подключиться".
- При успешном подключении будет сообщение: "Получен публичный ключ от сервера".

3. Отправляй сообщения по нажатию кнопки `Enter` на клавиатуре или `↑` рядом с полем ввода сообщения.

4. Теперь ты можешь переписываться со всеми, кто подключился к серверу!

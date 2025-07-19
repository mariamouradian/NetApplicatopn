using Seminar5.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Seminar5
{
    public class Client
    {
        private static bool _isRunning = true;
        private static string _userName = "";

        public static void Run(string serverPort = null)
        {
            try
            {
                if (string.IsNullOrEmpty(serverPort))
                {
                    Console.Write("Введите порт сервера: ");
                    serverPort = Console.ReadLine();
                }

                using var client = new UdpClient();
                var serverEp = new IPEndPoint(IPAddress.Loopback, int.Parse(serverPort));

                Console.Write("Введите ваше имя: ");
                _userName = Console.ReadLine();

                // Регистрация
                SendCommand(client, serverEp, Command.Register, _userName);
                Console.WriteLine($"Вы вошли как: {_userName}");

                // Поток для получения сообщений
                var receiveThread = new Thread(() => ReceiveMessages(client));
                receiveThread.Start();

                Console.WriteLine("\nДоступные команды:");
                Console.WriteLine("/to <имя> <сообщение> - отправить сообщение");
                Console.WriteLine("/list - получить непрочитанные сообщения");
                Console.WriteLine("/exit - выход\n");

                while (_isRunning)
                {
                    Console.Write("> ");
                    var input = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(input)) continue;

                    if (input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                    {
                        _isRunning = false;
                        Environment.Exit(0);
                    }
                    else if (input.StartsWith("/to "))
                    {
                        var parts = input.Split(' ', 3);
                        if (parts.Length == 3)
                        {
                            SendMessage(client, serverEp, parts[1], parts[2]);
                            Console.WriteLine($"Сообщение для {parts[1]} отправлено");
                        }
                        else
                        {
                            Console.WriteLine("Некорректный формат. Используйте: /to имя сообщение");
                        }
                    }
                    else if (input.Equals("/list", StringComparison.OrdinalIgnoreCase))
                    {
                        SendCommand(client, serverEp, Command.List, _userName);
                        Console.WriteLine("Запрос непрочитанных сообщений отправлен");
                    }
                    else
                    {
                        Console.WriteLine("Неизвестная команда");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА КЛИЕНТА: {ex.Message}");
            }
        }

        private static void SendCommand(UdpClient client, IPEndPoint ep, Command command, string fromName)
        {
            var message = new MessageUdp
            {
                Command = command,
                FromName = fromName
            };
            client.Send(Encoding.ASCII.GetBytes(message.ToJson()), ep);
        }

        private static void SendMessage(UdpClient client, IPEndPoint ep, string toName, string text)
        {
            var message = new MessageUdp
            {
                Command = Command.Message,
                FromName = _userName,
                ToName = toName,
                Text = text
            };
            client.Send(Encoding.ASCII.GetBytes(message.ToJson()), ep);
        }

        private static void ReceiveMessages(UdpClient client)
        {
            try
            {
                while (_isRunning)
                {
                    IPEndPoint serverEp = new(IPAddress.Any, 0);
                    byte[] receivedBytes = client.Receive(ref serverEp);
                    var message = MessageUdp.FromJson(Encoding.ASCII.GetString(receivedBytes));

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n[От {message.FromName}]: {message.Text}");
                    Console.ResetColor();
                    Console.Write("> ");

                    // Отправка подтверждения
                    if (message.Id.HasValue)
                    {
                        var confirmMsg = new MessageUdp
                        {
                            Command = Command.Confirmation,
                            Id = message.Id,
                            FromName = _userName
                        };
                        client.Send(Encoding.ASCII.GetBytes(confirmMsg.ToJson()), serverEp);
                    }
                }
            }
            catch (SocketException) when (!_isRunning)
            {
                // Нормальное завершение
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА ПРИЕМА: {ex.Message}");
            }
        }
    }
}
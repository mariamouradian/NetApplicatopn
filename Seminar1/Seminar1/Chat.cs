using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Seminar1
{
    internal class Chat
    {
        private const int ServerPort = 12345;
        private const int AckPort = 12346;
        private const int TimeoutMs = 2000;

        public static void Server()
        {
            // Сервер слушает основной порт для сообщений
            using var serverUdp = new UdpClient(ServerPort);
            // Отдельный клиент для отправки подтверждений
            using var ackSender = new UdpClient();

            Console.WriteLine("Сервер запущен и ожидает сообщения...");

            while (true)
            {
                try
                {
                    // Получение сообщения
                    IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] buffer = serverUdp.Receive(ref clientEP);
                    string json = Encoding.UTF8.GetString(buffer);

                    // Обработка сообщения
                    var message = Message.FromJson(json);
                    if (message != null)
                    {
                        Console.WriteLine(message.ToString());

                        // Отправка подтверждения на клиентский порт для подтверждений
                        byte[] ackMsg = Encoding.UTF8.GetBytes("ACK");
                        IPEndPoint ackEP = new IPEndPoint(clientEP.Address, AckPort);
                        ackSender.Send(ackMsg, ackMsg.Length, ackEP);
                    }
                    else
                    {
                        Console.WriteLine("Получено некорректное сообщение");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка сервера: {ex.Message}");
                }
            }
        }

        public static void Client(string nickname)
        {
            using var clientUdp = new UdpClient();
            // Клиент слушает порт для подтверждений
            using var ackListener = new UdpClient(AckPort);
            ackListener.Client.ReceiveTimeout = TimeoutMs;

            var serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), ServerPort);

            while (true)
            {
                Console.Write("Введите сообщение: ");
                string text = Console.ReadLine();

                if (string.IsNullOrEmpty(text))
                    break;

                try
                {
                    // Отправка сообщения
                    var message = new Message(nickname, text);
                    byte[] data = Encoding.UTF8.GetBytes(message.ToJson());
                    clientUdp.Send(data, data.Length, serverEP);

                    // Ожидание подтверждения
                    IPEndPoint senderEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] ackBuffer = ackListener.Receive(ref senderEP);
                    string ack = Encoding.UTF8.GetString(ackBuffer);

                    if (ack == "ACK")
                    {
                        Console.WriteLine("Сервер подтвердил получение сообщения");
                    }
                    else
                    {
                        Console.WriteLine("Получен некорректный ответ от сервера");
                    }
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    Console.WriteLine("⚠ Превышено время ожидания подтверждения. Проверьте:");
                    Console.WriteLine("1. Сервер запущен и работает");
                    Console.WriteLine("2. Нет блокировки брандмауэром");
                    Console.WriteLine("3. Порты {0} и {1} доступны", ServerPort, AckPort);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка клиента: {ex.Message}");
                }
            }
        }
    }
}
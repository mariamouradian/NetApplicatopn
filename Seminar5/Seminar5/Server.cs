﻿using Seminar5.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Seminar5
{
    public class Server
    {
        private readonly Dictionary<string, IPEndPoint> _clients = new();
        private UdpClient _udpClient;

        public void Work(int port = 0)
        {
            try
            {
                _udpClient = new UdpClient(port);
                Console.WriteLine($"SERVER: Запущен на порту {((IPEndPoint)_udpClient.Client.LocalEndPoint).Port}");
                Console.WriteLine("SERVER: Ожидание сообщений... (Ctrl+C для выхода)");

                IPEndPoint remoteEp = new(IPAddress.Any, 0);
                while (true)
                {
                    try
                    {
                        byte[] data = _udpClient.Receive(ref remoteEp);
                        var message = MessageUdp.FromJson(Encoding.ASCII.GetString(data));

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\nSERVER: Получено {message.Command} от {message.FromName}");
                        Console.ResetColor();

                        ProcessMessage(message, remoteEp);
                    }
                    catch (SocketException ex) when (ex.ErrorCode == 10054)
                    {
                        Console.WriteLine("SERVER: Клиент отключился");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"SERVER ERROR: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
            finally
            {
                _udpClient?.Close();
            }
        }

        private void ProcessMessage(MessageUdp message, IPEndPoint fromEp)
        {
            switch (message.Command)
            {
                case Command.Register:
                    RegisterClient(message, fromEp);
                    break;

                case Command.Message:
                    Console.WriteLine($"Пересылка сообщения для {message.ToName}");
                    RelayMessage(message);
                    break;

                case Command.Confirmation:
                    Console.WriteLine($"Подтверждение получения сообщения ID={message.Id}");
                    ConfirmDelivery(message.Id);
                    break;

                case Command.List:
                    Console.WriteLine($"Запрос непрочитанных сообщений для {message.FromName}");
                    SendUnreadMessages(message.FromName, fromEp);
                    break;
            }
        }

        private void RegisterClient(MessageUdp message, IPEndPoint fromEp)
        {
            _clients[message.FromName] = fromEp;

            using var ctx = new Context();
            if (!ctx.Users.Any(u => u.Name == message.FromName))
            {
                ctx.Users.Add(new User { Name = message.FromName });
                ctx.SaveChanges();
                Console.WriteLine($"Зарегистрирован новый пользователь: {message.FromName}");
            }
        }

        private void RelayMessage(MessageUdp message)
        {
            if (!_clients.TryGetValue(message.ToName, out var targetEp))
            {
                Console.WriteLine($"Ошибка: Пользователь {message.ToName} не найден");
                return;
            }

            using var ctx = new Context();
            var fromUser = ctx.Users.First(u => u.Name == message.FromName);
            var toUser = ctx.Users.First(u => u.Name == message.ToName);

            var dbMessage = new Message
            {
                FromUser = fromUser,
                ToUser = toUser,
                Text = message.Text,
                Received = false
            };

            ctx.Messages.Add(dbMessage);
            ctx.SaveChanges();

            var forwardMessage = new MessageUdp
            {
                Id = dbMessage.Id,
                Command = Command.Message,
                FromName = message.FromName,
                ToName = message.ToName,
                Text = message.Text
            };

            byte[] bytes = Encoding.ASCII.GetBytes(forwardMessage.ToJson());
            _udpClient.Send(bytes, bytes.Length, targetEp);
        }

        private void ConfirmDelivery(int? messageId)
        {
            if (messageId == null) return;

            using var ctx = new Context();
            var message = ctx.Messages.FirstOrDefault(m => m.Id == messageId);
            if (message != null)
            {
                message.Received = true;
                ctx.SaveChanges();
            }
        }

        private void SendUnreadMessages(string userName, IPEndPoint targetEp)
        {
            using var ctx = new Context();
            var user = ctx.Users
                .Include(u => u.ToMessages)
                .FirstOrDefault(u => u.Name == userName);

            if (user == null) return;

            foreach (var message in user.ToMessages.Where(m => !m.Received))
            {
                var msg = new MessageUdp
                {
                    Id = message.Id,
                    Command = Command.Message,
                    FromName = message.FromUser.Name,
                    ToName = userName,
                    Text = message.Text
                };

                byte[] bytes = Encoding.ASCII.GetBytes(msg.ToJson());
                _udpClient.Send(bytes, bytes.Length, targetEp);
                message.Received = true;
            }
            ctx.SaveChanges();
        }
    }
}
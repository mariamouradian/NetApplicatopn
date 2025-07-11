using Seminar4_HW.Common.Interfaces;
using Seminar4_HW.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Seminar4_HW.Server.Commands;

namespace Seminar4_HW.Server.Sevices
{
    public sealed class ChatServer : IChatSubject<Message>
    {
        private static readonly Lazy<ChatServer> _instance = new(() => new ChatServer());
        public static ChatServer Instance => _instance.Value;

        private readonly UdpClient _udpServer;
        private readonly Dictionary<string, ClientInfo> _clients = new();
        private readonly List<IChatObserver<Message>> _observers = new();
        private readonly ChatCommandProcessor _commandProcessor = new();
        private readonly int _serverPort = 12345;

        private ChatServer()
        {
            _udpServer = new UdpClient(_serverPort);
            Console.WriteLine($"Сервер запущен на порту {_serverPort}");
        }

        public void Start()
        {
            while (true)
            {
                try
                {
                    IPEndPoint clientEP = new(IPAddress.Any, 0);
                    byte[] buffer = _udpServer.Receive(ref clientEP);
                    string json = Encoding.UTF8.GetString(buffer);
                    var message = Message.FromJson(json);
                    message.SenderEndpoint = clientEP;

                    ProcessMessage(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка сервера: {ex.Message}");
                }
            }
        }

        public IEnumerable<string> GetClientsList()
        {
            return _clients.Keys.ToList();
        }

        public void SendToClient(Message message)
        {
            if (_clients.TryGetValue(message.ToName, out var recipient))
            {
                byte[] data = Encoding.UTF8.GetBytes(message.ToJson());
                _udpServer.Send(data, data.Length, recipient.Endpoint);
            }
        }

        private void ProcessMessage(Message message)
        {
            NotifyObservers(message);

            if (message.ToName.Equals("Server", StringComparison.OrdinalIgnoreCase))
            {
                var command = _commandProcessor.GetCommand(message);
                command?.Execute();
            }
            else
            {
                SendToClient(message);
            }
        }

        public void RegisterClient(string name, IPEndPoint endpoint)
        {
            _clients[name] = new ClientInfo { Name = name, Endpoint = endpoint, LastActivity = DateTime.Now };
            Console.WriteLine($"Клиент зарегистрирован: {name} ({endpoint})");
        }

        public void UnregisterClient(string name)
        {
            if (_clients.Remove(name))
            {
                Console.WriteLine($"Клиент отключен: {name}");
            }
        }

        public void RegisterObserver(IChatObserver<Message> observer) => _observers.Add(observer);
        public void RemoveObserver(IChatObserver<Message> observer) => _observers.Remove(observer);
        public void NotifyObservers(Message message)
        {
            foreach (var observer in _observers)
            {
                observer.Update(message);
            }
        }
    }
}

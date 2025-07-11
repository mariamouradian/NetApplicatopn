using Seminar4_HW.Common.Interfaces;
using Seminar4_HW.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Client.Services
{
    public class ChatClient : IChatObserver<Message>
    {
        private readonly string _name;
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _serverEndpoint;
        private readonly IChatSubject<Message> _messageSubject;

        public string Name => _name;

        public ChatClient(string name, IChatSubject<Message> messageSubject)
        {
            _name = name;
            _messageSubject = messageSubject;
            _udpClient = new UdpClient(0); 
            _serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);

            _messageSubject.RegisterObserver(this);
            RegisterWithServer();
        }

        private void RegisterWithServer()
        {
            var message = new Message
            {
                FromName = _name,
                ToName = "Server",
                Text = "register",
                Time = DateTime.Now
            };

            SendMessage(message);
        }

        public void SendMessage(Message message)
        {
            message.FromName = _name;
            message.Time = DateTime.Now;

            var json = message.ToJson();
            var bytes = Encoding.UTF8.GetBytes(json);
            _udpClient.Send(bytes, bytes.Length, _serverEndpoint);
        }

        public void Listen()
        {
            while (true)
            {
                try
                {
                    var senderEP = new IPEndPoint(IPAddress.Any, 0);
                    var data = _udpClient.Receive(ref senderEP);
                    var json = Encoding.UTF8.GetString(data);
                    var message = Message.FromJson(json);
                    _messageSubject.NotifyObservers(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        public void Update(Message message)
        {
            Console.WriteLine(message);
        }
    }
}

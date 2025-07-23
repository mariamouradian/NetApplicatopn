using Seminar5.Abstraction;
using Seminar5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Seminar5
{
    class MessageSource : IMessageSource
    {
        private readonly UdpClient _udpClient;

        public UdpClient UdpClient => _udpClient;

        public MessageSource(int port)
        {
            _udpClient = new UdpClient(port);
        }

        public MessageUdp ReceiveMessage(ref IPEndPoint endPoint)
        {
            try
            {
                byte[] data = _udpClient.Receive(ref endPoint);
                string json = Encoding.UTF8.GetString(data);
                return MessageUdp.FromJson(json);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка сокета: {ex.Message}");
                throw;
            }
        }

        public void SendMessage(MessageUdp message, IPEndPoint endPoint)
        {
            string json = message.ToJson();
            byte[] data = Encoding.UTF8.GetBytes(json);
            _udpClient.Send(data, data.Length, endPoint);
        }
    }
}

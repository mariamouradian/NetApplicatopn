using Seminar5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Seminar5.Abstraction
{
    public interface IMessageSource
    {
        UdpClient UdpClient { get; }
        MessageUdp? ReceiveMessage(ref IPEndPoint? endPoint);
        void SendMessage(MessageUdp message, IPEndPoint endPoint);
    }
}

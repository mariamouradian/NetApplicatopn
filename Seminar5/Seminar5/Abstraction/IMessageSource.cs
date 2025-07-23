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

        void SendMessage(MessageUdp message, IPEndPoint endPoint);

        MessageUdp ReceiveMessage(ref IPEndPoint endPoint);

    }
}

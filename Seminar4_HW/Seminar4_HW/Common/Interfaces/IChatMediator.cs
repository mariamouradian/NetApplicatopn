using Seminar4_HW.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Common.Interfaces
{
    public interface IChatMediator
    {
        void Register(string clientName, IPEndPoint endpoint);
        void Unregister(string clientName);
        void SendMessage(Message message);
    }
}

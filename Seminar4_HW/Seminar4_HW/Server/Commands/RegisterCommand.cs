using Seminar4_HW.Common.Interfaces;
using Seminar4_HW.Server.Sevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Server.Commands
{
    public class RegisterCommand : IChatCommand
    {
        private readonly string _clientName;
        private readonly IPEndPoint _clientEndpoint;

        public RegisterCommand(string clientName, IPEndPoint clientEndpoint)
        {
            _clientName = clientName;
            _clientEndpoint = clientEndpoint;
        }

        public void Execute()
        {
            ChatServer.Instance.RegisterClient(_clientName, _clientEndpoint);
        }
    }
}

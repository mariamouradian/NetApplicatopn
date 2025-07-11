using Seminar4_HW.Common.Interfaces;
using Seminar4_HW.Server.Sevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Server.Commands
{
    public class UnregisterCommand : IChatCommand
    {
        private readonly string _clientName;

        public UnregisterCommand(string clientName)
        {
            _clientName = clientName;
        }

        public void Execute()
        {
            ChatServer.Instance.UnregisterClient(_clientName);
        }
    }
}

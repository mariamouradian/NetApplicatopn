using Seminar4_HW.Common.Interfaces;
using Seminar4_HW.Common.Models;
using Seminar4_HW.Server.Sevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Server.Commands
{
    public class ListClientsCommand : IChatCommand
    {
        private readonly string _requesterName;

        public ListClientsCommand(string requesterName)
        {
            _requesterName = requesterName;
        }

        public void Execute()
        {
            var server = ChatServer.Instance;
            var clientsList = string.Join("\n", server.GetClientsList());
            var response = new Message
            {
                FromName = "Server",
                ToName = _requesterName,
                Text = $"Список клиентов:\n{clientsList}",
                Time = DateTime.Now
            };

            server.SendToClient(response);
        }
    }
}

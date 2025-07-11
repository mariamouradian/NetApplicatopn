using Seminar4_HW.Common.Interfaces;
using Seminar4_HW.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Server.Commands
{
    public class ChatCommandProcessor
    {
        private readonly Dictionary<string, Func<Message, IChatCommand>> _commandFactories;

        public ChatCommandProcessor()
        {
            _commandFactories = new Dictionary<string, Func<Message, IChatCommand>>(StringComparer.OrdinalIgnoreCase)
            {
                ["register"] = msg => new RegisterCommand(msg.FromName, msg.SenderEndpoint),
                ["unregister"] = msg => new UnregisterCommand(msg.FromName),
                ["list"] = msg => new ListClientsCommand(msg.FromName)
            };
        }

        public IChatCommand GetCommand(Message message)
        {
            if (_commandFactories.TryGetValue(message.Text, out var factory))
            {
                return factory(message);
            }
            return null;
        }
    }
}

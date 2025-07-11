using Seminar4_HW.Client.Services;
using Seminar4_HW.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seminar4_HW.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ChatApp.Client.exe <username>");
                return;
            }

            Console.Title = $"Chat Client - {args[0]}";
            var messageSubject = new MessageSubject();
            var client = new ChatClient(args[0], messageSubject);

            var listenerThread = new Thread(client.Listen);
            listenerThread.Start();

            while (true)
            {
                Console.Write("To: ");
                var to = Console.ReadLine();

                Console.Write("Message: ");
                var text = Console.ReadLine();

                var message = new Message
                {
                    ToName = to,
                    Text = text
                };

                client.SendMessage(message);
            }
        }
    }
}

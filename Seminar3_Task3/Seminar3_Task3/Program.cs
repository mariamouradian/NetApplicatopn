using System;
using System.Threading.Tasks;

namespace Seminar3_Task3
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                await Server.AcceptMesg();
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    await Client.SendMsg($"args[0] {i}");
                }
            }    
        }
    }
}
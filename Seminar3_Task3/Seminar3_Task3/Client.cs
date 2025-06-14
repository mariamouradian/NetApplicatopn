using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Seminar3_Task3
{
    internal class Client
    {
        public static async Task SendMsg(string name)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 16874);
            UdpClient udpClient = new UdpClient();
            while (true)
            {
                Console.Write("Print message (or 'Exit' to stop): ");
                string text = Console.ReadLine();

                //if (text.Equals("Exit", StringComparison.OrdinalIgnoreCase))
                //    break;

                Message msg = new Message(name, text);
                string responseMsgJs = msg.ToJson();
                byte[] responseData = Encoding.UTF8.GetBytes(responseMsgJs);
                await udpClient.SendAsync(responseData, responseData.Length, ep);

                byte[] answerData = udpClient.Receive(ref ep);
                string answerMsgJs = Encoding.UTF8.GetString(answerData);
                Message answerMsg = Message.FromJson(answerMsgJs);
                Console.WriteLine(answerMsg.ToString());
                
            }
        }

    }
}

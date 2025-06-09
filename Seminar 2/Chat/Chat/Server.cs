using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    internal class Server
    {
        public static void AcceptMesg()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            using (UdpClient udpClient = new UdpClient(16874))
            {
                Console.WriteLine("Server is waiting for a message (Press any key to stop)...");

                var cancelThread = new Thread(() =>
                {
                    Console.ReadKey();
                    udpClient.Close();
                });
                cancelThread.Start();

                try
                {
                    while (true)
                    {
                        byte[] buffer = udpClient.Receive(ref ep);
                        string data = Encoding.UTF8.GetString(buffer);
                        Message msg = Message.FromJson(data);
                        Console.WriteLine(msg.ToString());

                        Message responseMsg = new Message("Server", "Message accepted by the server");
                        string responseMsgJs = responseMsg.ToJson();
                        byte[] responseDate = Encoding.UTF8.GetBytes(responseMsgJs);
                        udpClient.Send(responseDate, responseDate.Length, ep);
                    }
                }
                catch (SocketException)
                {
                    Console.WriteLine("Server stopped");
                }
            }
        }
    }
}

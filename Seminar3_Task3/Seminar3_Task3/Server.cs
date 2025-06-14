using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Seminar3_Task3
{
    internal class Server
    {
        public static async Task AcceptMsg()
        {
            using var cts = new CancellationTokenSource();
            var exitTask = Task.Run(() =>
            {
                Console.WriteLine("Server is running. Press any key to stop...");
                Console.ReadKey();
                cts.Cancel();
            });

            try
            {
                await ListenForMessages(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Server stopped gracefully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }

        private static async Task ListenForMessages(CancellationToken token)
        {
            using var udpClient = new UdpClient(16874);
            var ep = new IPEndPoint(IPAddress.Any, 0);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var receiveTask = udpClient.ReceiveAsync();
                    var timeoutTask = Task.Delay(1000, token); 

                    var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
                    if (completedTask == timeoutTask)
                        continue;

                    var data = receiveTask.Result;
                    string data1 = Encoding.UTF8.GetString(data.Buffer);

                    await ProcessMessage(udpClient, data.RemoteEndPoint, data1);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
                {
                    break;
                }
            }
        }

        private static async Task ProcessMessage(UdpClient udpClient, IPEndPoint remoteEp, string messageJson)
        {
            try
            {
                Message msg = Message.FromJson(messageJson);
                if (msg != null)
                {
                    Console.WriteLine(msg.ToString());

                    var responseMsg = new Message("Server", "Message accepted by the server");
                    string responseMsgJs = responseMsg.ToJson();
                    byte[] responseData = Encoding.UTF8.GetBytes(responseMsgJs);

                    await udpClient.SendAsync(responseData, responseData.Length, remoteEp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }
    }
}
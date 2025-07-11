namespace Seminar4_HW.Server.Sevices
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Chat Server";
            ChatServer.Instance.Start();
        }
    }
}

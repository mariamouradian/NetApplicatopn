namespace Chat
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Server.AcceptMesg();
            }
            else
            {
                Client.SendMsg(args[0]);
            }

        }
    }
}

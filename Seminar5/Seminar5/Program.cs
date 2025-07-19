namespace Seminar5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--client")
            {
                Client.Run(args.Length > 1 ? args[1] : null);
            }
            else
            {
                new Server().Work(args.Length > 0 ? int.Parse(args[0]) : 0);
            }
        }
    }
}
// Код для семинара 6 ниже:
using System.Net;

namespace Seminar5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("--client"))
            {
                int serverPort = args.Length > 1 ? int.Parse(args[1]) : 12345;
                var messageSource = new MessageSource(0);
                var serverEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverPort);

                Console.Write("Введите ваше имя: ");
                string name = Console.ReadLine() ?? "DefaultUser";

                var client = new Client(messageSource, serverEp, name);
                client.Run();
            }
            else
            {
                int port = args.Length > 0 ? int.Parse(args[0]) : 12345;
                var messageSource = new MessageSource(port);
                Console.WriteLine($"Сервер запущен на порту: {port}");
                new Server(messageSource).Work();
            }
        }
    }
}


//// Код для семинара 5 ниже:

//namespace Seminar5
//{
//    internal class Program
//    {
//        static void Main(string[] args)
//        {
//            if (args.Length > 0 && args[0] == "--client")
//            {
//                Client.Run(args.Length > 1 ? args[1] : null);
//            }
//            else
//            {
//                new Server().Work(args.Length > 0 ? int.Parse(args[0]) : 0);
//            }
//        }
//    }
//}
﻿namespace Seminar1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Chat.Server();
            }
            else
            {
                Chat.Client(args[0]);
            }
        }
    }
}
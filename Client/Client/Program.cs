using System;
using System.Threading;


namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 19; i++)
            {
                new Thread(ClientSocket.Connect).Start();
            }
        }
    }
}

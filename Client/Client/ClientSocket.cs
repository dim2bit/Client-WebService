using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Client
{
    class ClientSocket
    {
        private static readonly int _port = 8080;
        private static readonly string _localIp = "127.0.0.1";

        public ClientSocket()
        {
        }   

        public static void Connect()
        {
            try
            {
                //Console.Write("Your id: ");
                //string id = Console.ReadLine();

                //Console.Write("Your surname: ");
                //string surname = Console.ReadLine();

                string id = "1";
                string surname = "Surname";

                string message = "{ 'id':'" + id + "', 'surname':'" + surname + "' }";
                byte[] sendBuffer = Encoding.ASCII.GetBytes(message);

                using (TcpClient client = new TcpClient(_localIp, _port))
                {
                    Console.WriteLine("Connected");

                    NetworkStream stream = client.GetStream();

                    stream.WriteAsync(sendBuffer, 0, sendBuffer.Length);

                    byte[] recieveBuffer = new byte[client.ReceiveBufferSize];

                    string data = null;
                    do
                    {
                        int bytesRead = stream.Read(recieveBuffer, 0, client.ReceiveBufferSize);
                        data += Encoding.ASCII.GetString(recieveBuffer, 0, bytesRead);
                    }
                    while (stream.DataAvailable);

                    if (data[0] == '<')      // XML is recieved
                        Console.WriteLine("Recieved XML: \n" + data);
                    else if (data[0] == '{') // JSON is recieved
                        Console.WriteLine("Recieved JSON: \n" + data);
                    else
                        Console.WriteLine("Recieved data: \n" + data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}






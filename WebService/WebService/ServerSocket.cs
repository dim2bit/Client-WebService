using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;


namespace WebService
{
    [Serializable] // serializable to XML or JSON formats
    public class Message
    {
        public List<string> Surnames = new List<string>();
        public List<DateTime> ConnectionDates = new List<DateTime>();
        public DateTime TimerStartDate;

        public Message()
        {
        }

        public Message(List<string> surnames,
                       List<DateTime> connectionDates,
                       DateTime timerStartDate)
        {
            Surnames = surnames;
            ConnectionDates = connectionDates;
            TimerStartDate = timerStartDate;
        }
    }

    public class ServerSocket
    {
        private static ManualResetEvent _isConnected = new ManualResetEvent(false);
        private static Message _message = new Message();
        private static bool _isTheFirstClient = true;

        private static System.Timers.Timer _aTimer;

        private static Queue<TcpClient> _clients = new Queue<TcpClient>();

        private static Queue<KeyValuePair<NetworkStream, Message>> _streams =
            new Queue<KeyValuePair<NetworkStream, Message>>(256);


        public ServerSocket()
        {
        }

        public static void StartListening()
        {
            const int port = 8080;
            const string localIp = "127.0.0.1";

            TcpListener server = null;
            try
            {
                IPAddress localAddr = IPAddress.Parse(localIp);
                server = new TcpListener(localAddr, port);
                server.Start();

                while (true)
                {
                    Console.WriteLine("Waiting for connection...");

                    _isConnected.Reset();

                    server.BeginAcceptTcpClient(
                        new AsyncCallback(AcceptCallback),
                        server);

                    _isConnected.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                }
            }
        }

        public static void AcceptCallback(IAsyncResult asyncRes)
        {
            TcpListener server = (TcpListener)asyncRes.AsyncState;

            TcpClient client = server.EndAcceptTcpClient(asyncRes);
            _clients.Enqueue(client);
            Console.WriteLine("Connected");
            DateTime connectionDate = DateTime.Now;

            if (_isTheFirstClient)
            {
                const int interval = 19000;
                _aTimer = new System.Timers.Timer(interval);

                _aTimer.Elapsed += new ElapsedEventHandler((source, e) =>
                {
                    Send(_streams);
                    CloseConnection();
                });

                _aTimer.AutoReset = false;

                _aTimer.Enabled = true;
                _message.TimerStartDate = DateTime.Now;

                _isTheFirstClient = false;
            }

            NetworkStream stream = client.GetStream();

            _isConnected.Set();

            (string id, string surname) = Recieve(client, stream);

            Console.WriteLine("client id: " + id + ", client surname: " + surname);

            _message.Surnames.Add(surname);
            _message.ConnectionDates.Add(connectionDate);

            _streams.Enqueue(new KeyValuePair<NetworkStream, Message>(stream, _message));
        }

        public static (string, string) Recieve(TcpClient client, NetworkStream stream)
        {
            byte[] buffer = new byte[client.ReceiveBufferSize];
            string data = null;
            do
            {
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
                data += Encoding.ASCII.GetString(buffer, 0, bytesRead);
            }
            while (stream.DataAvailable);

            var definition = new { id = "", surname = "" };
            var clientMessage = JsonConvert.DeserializeAnonymousType(data, definition);

            return (clientMessage.id, clientMessage.surname);
        }

        public static void Send(Queue<KeyValuePair<NetworkStream, Message>> streams)
        {
            try
            {
                Queue<string> surnames_queue = new Queue<string>(streams.Peek().Value.Surnames);

                for (; streams.Count != 0;)
                {
                    KeyValuePair<NetworkStream, Message> streamData = streams.Dequeue();

                    byte[] buffer;

                    // serialize to JSON
                    if (surnames_queue.Dequeue()[0] <= 'O')
                    {
                        string document = Serializator.SerializeToJson(streamData.Value);
                        buffer = Encoding.ASCII.GetBytes(document);
                    }
                    else // serialize to XML
                    {
                        XmlDocument document = Serializator.SerializeToXml(streamData.Value);
                        buffer = Encoding.ASCII.GetBytes(document.OuterXml);
                    }

                    streamData.Key.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // dispose all TcpClient objects
        public static void CloseConnection()
        {
            foreach (TcpClient client in _clients)
            {
                if (client.Connected)
                {
                    client.Close();
                }
            }
        }
    }
}

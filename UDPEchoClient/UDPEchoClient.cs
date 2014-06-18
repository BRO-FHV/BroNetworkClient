using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPEchoClient
{
    class UDPEchoClient
    {
        private static int PORT = 2000;

        static void Main(string[] args)
        {
            //create a new server
            var server = new UdpListener();

            //start listening for messages and copy the messages back to the client
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var received = await server.Receive();
                    Console.WriteLine("received: " + received.Message);
                }
            });

            //create a new client
            var client = UdpUser.ConnectTo("192.168.0.7", PORT);

            //wait for reply messages from server and send them to console 
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        var received = await client.Receive();
                        Console.WriteLine(received.Message);
                        if (received.Message.Contains("quit"))
                            break;
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex);
                    }
                }
            });

            //type ahead :-)
            string read;
            do
            {
                read = Console.ReadLine();
                client.Send(read);
            } while (read != "quit");
        }

        public struct Received
        {
            public IPEndPoint Sender;
            public string Message;
        }

        abstract class UdpBase
        {
            protected UdpClient Client;

            protected UdpBase()
            {
                Client = new UdpClient();
            }

            public async Task<Received> Receive()
            {
                var result = await Client.ReceiveAsync();
                return new Received()
                {
                    Message = Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length),
                    Sender = result.RemoteEndPoint
                };
            }
        }

        //Server
        class UdpListener : UdpBase
        {
            private IPEndPoint _listenOn;

            public UdpListener()
                : this(new IPEndPoint(IPAddress.Any, PORT))
            {
            }

            public UdpListener(IPEndPoint endpoint)
            {
                _listenOn = endpoint;
                Client = new UdpClient(_listenOn);
            }

            public void Reply(string message, IPEndPoint endpoint)
            {
                var datagram = Encoding.ASCII.GetBytes(message);
                Client.Send(datagram, datagram.Length, endpoint);
            }

        }

        //Client
        class UdpUser : UdpBase
        {
            private UdpUser() { }

            public static UdpUser ConnectTo(string hostname, int port)
            {
                var connection = new UdpUser();
                connection.Client.Connect(hostname, port);
                return connection;
            }

            public void Send(string message)
            {
                var datagram = Encoding.ASCII.GetBytes(message);
                Client.Send(datagram, datagram.Length);
            }

        }
    }
}
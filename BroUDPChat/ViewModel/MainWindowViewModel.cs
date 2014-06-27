using BroUDPChat.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BroUDPChat
{
    public class MainWindowViewModel : INotifyPropertyChanged, ISendMessageCmdVm, ISetUserNameCmdVm, ISetLEDVM
    {
        private string _userName= "";
        private string _text;
        private bool _led1;
        private bool _led2;
        private bool _led3;
        private bool _led4;
        private string _textToSend;

        private static IPAddress IP_ADDRESS = IPAddress.Parse("192.168.0.7");
        private static int PORT = 2000;
        private static IPEndPoint END_POINT = new IPEndPoint(IP_ADDRESS, PORT);
        private static byte LED = 0x01;
        public MainWindowViewModel()
        {
            CommandSendMessage = new SendMessageCmd(this);
            CommandSetUserName = new SetUserNameCmd(this);
            CommandLED1 = new SetLEDCommand(this, 1);
            CommandLED2 = new SetLEDCommand(this, 2);
            CommandLED3 = new SetLEDCommand(this, 3);
            CommandLED4 = new SetLEDCommand(this, 4);

            CreateUdpReadThread();
        }


        public string TextToSend
        {
            get { return _textToSend; }
            set { _textToSend = value; NotifyPorpertyChanged(); }
        }


        public string UserName
        {
            get { return _userName; }
            set {
                _userName = value; 
                NotifyPorpertyChanged(); 
                NotifyPorpertyChanged("UserNameNotNull");
            }
        }

        public string History
        {
            get { return _text; }
            set { _text += value; NotifyPorpertyChanged(); }
        }

        public bool Led1
        {
            get { return _led1; }
            set { _led1 = value; NotifyPorpertyChanged(); }
        }

        public bool Led2
        {
            get { return _led2; }
            set { _led2 = value; NotifyPorpertyChanged(); }
        }

        public bool Led3
        {
            get { return _led3; }
            set { _led3 = value; NotifyPorpertyChanged(); }
        }


        public bool Led4
        {
            get { return _led4; }
            set { _led4 = value; NotifyPorpertyChanged(); }
        }

        public bool UserNameNotNull { get { return !string.IsNullOrEmpty(UserName);} }

        public SendMessageCmd CommandSendMessage { get; set; }

        public SetUserNameCmd CommandSetUserName { get; set; }

        public SetLEDCommand CommandLED1 { get; set; }
        public SetLEDCommand CommandLED2 { get; set; }
        public SetLEDCommand CommandLED3 { get; set; }
        public SetLEDCommand CommandLED4 { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        private void NotifyPorpertyChanged([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        

        public void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(TextToSend))
                Send(TextToSend);
        }

        public void SetUserName()
        {
            UserName = UserName;
        }

        private void Send(string text)
        {
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                byte[] send_buffer = Encoding.ASCII.GetBytes(UserName + ": " + text);
                sock.SendTo(send_buffer, END_POINT);
            }
        }

        private void Send(byte[] data)
        {
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                byte[] username = Encoding.ASCII.GetBytes(UserName + ": ");
                byte[] buffer = new byte[7+data.Length] ;
                for (int i = 0; i < username.Length; i++)
                {
                    buffer[i]= username[i];
                }
                for (int i = 0; i < data.Length; i++)
                {
                    buffer[i + 7] = data[i];
                }
                sock.SendTo(buffer, END_POINT);
            }
        }

        public void EnableLED(int num)
        {
            byte status = 1;
            byte LEDnum = (byte)num;
            byte[] data = new byte[] { LED, LEDnum, status };
            Send(data);
        }

        public void DisableLED(int num)
        {
            byte status = 0;
            byte LEDnum = (byte)num;
            byte[] data = new byte[] { LED, LEDnum, status };
            Send(data);
        }

        private void CreateUdpReadThread()
        {
            //create a new server
            var server = new UdpListener();

            //start listening for messages and copy the messages back to the client
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var received = await server.Receive();
                    propcessText(received.Message);

                    Console.WriteLine("received: " + received.Message);
                }
            });
        }

        private void propcessText(string p)
        {
            if (p.StartsWith("SetLED="))
            {
                string LED = p.Split('=')[1];
                int LEDNum = int.Parse(LED.Split(':')[0]);
                bool on = bool.Parse(LED.Split(':')[1]);
                switch (LEDNum)
                {
                    case 1:
                        Led1 = on;
                        break;
                    case 2:
                        Led2 = on;
                        break;
                    case 3:
                        Led3 = on;
                        break;
                    case 4:
                        Led4 = on;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                History = p + "\n";
            }
        }

        public class DataEventArgs : EventArgs
        {
            public byte[] Data { get; private set; }
            public IPAddress IpAddress { get; private set; }

            public DataEventArgs(IPAddress ipaddress, byte[] data)
            {
                IpAddress = ipaddress;
                Data = data;
            }
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
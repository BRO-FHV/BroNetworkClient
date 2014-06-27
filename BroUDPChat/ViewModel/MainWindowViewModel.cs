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
        private string _userName;
        private string _text;
        private bool _led1;
        private bool _led2;
        private bool _led3;
        private bool _led4;
        private string _textToSend;

        private static const IPAddress IP_ADDRESS = IPAddress.Parse("192.168.0.7");

        private static const IPEndPoint endPoint = new IPEndPoint(IP_ADDRESS, 2000);


        public MainWindowViewModel()
        {
            CommandSendMessage = new SendMessageCmd(this);
            CommandSetUserName = new SetUserNameCmd(this);
            CommandLED1 = new SetLEDCommand(this, 1);
            CommandLED2 = new SetLEDCommand(this, 2);
            CommandLED3 = new SetLEDCommand(this, 3);
            CommandLED4 = new SetLEDCommand(this, 4);


        }


        public string TextToSend
        {
            get { return _textToSend; }
            set { _textToSend = value; NotifyPorpertyChanged(); }
        }


        public string UserName
        {
            get { return _userName; }
            set { _userName = value; NotifyPorpertyChanged(); }
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
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                byte[] send_buffer = Encoding.ASCII.GetBytes(TextToSend);
                sock.SendTo(send_buffer, endPoint);
            }
        }

        public void SetUserName()
        {
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                byte[] send_buffer = Encoding.ASCII.GetBytes("UserName=" + UserName);
                sock.SendTo(send_buffer, endPoint);
            }
        }

        public void EnableLED(int num)
        {
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                byte[] send_buffer = Encoding.ASCII.GetBytes("EnableLED=" + num);
                sock.SendTo(send_buffer, endPoint);
            }
        }

        public void DisableLED(int num)
        {
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                byte[] send_buffer = Encoding.ASCII.GetBytes("DisableLED=" + num);
                sock.SendTo(send_buffer, endPoint);
            }
        }


        private Thread _udpReadThread;
        private volatile bool _terminateThread;

        public event DataEventHandler OnDataReceived;
        public delegate void DataEventHandler(object sender, DataEventArgs e);

        private void CreateUdpReadThread()
        {
            _udpReadThread = new Thread(UdpReadThread) { Name = "UDP Read thread" };
            _udpReadThread.Start(endPoint);
        }

        private void UdpReadThread(object endPoint)
        {
            var myEndPoint = (EndPoint)endPoint;
            var udpListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Important to specify a timeout value, otherwise the socket ReceiveFrom() 
            // will block indefinitely if no packets are received and the thread will never terminate
            udpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 100);
            udpListener.Bind(myEndPoint);

            try
            {
                while (!_terminateThread)
                {
                    try
                    {
                        var buffer = new byte[1024];
                        var size = udpListener.ReceiveFrom(buffer, ref myEndPoint);
                        Array.Resize(ref buffer, size);

                        // Let any consumer(s) handle the data via an event
                        FireOnDataReceived(((IPEndPoint)(myEndPoint)).Address, buffer);
                    }
                    catch (SocketException socketException)
                    {
                        // Handle socket errors
                    }
                }
            }
            finally
            {
                // Close Socket
                udpListener.Shutdown(SocketShutdown.Both);
                udpListener.Close();
            }
        }

        public void FireOnDataReceived(IPAddress address, byte[] buffer)
        {
            if (address == IP_ADDRESS)
            {
                propcessText(Encoding.ASCII.GetString(buffer));
            }
            if (OnDataReceived != null)
            {
                OnDataReceived(this, new DataEventArgs(address, buffer));
            }
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectorHub
{
    public class ConnectorHub
    {
        public delegate void StartRecordingDelegate(object sender);
        public event StartRecordingDelegate startRecordingEvent;

        public delegate void StopRecordingDelegate(object sender);
        public event StopRecordingDelegate stopRecordingEvent;

        string IamReady = "<I AM READY>";
        string StartRecording = "<START RECORDING>";
        string StopRecording = "<STOP RECORDING>";

        private int TCPListenerPort { get; set; }
        private int TCPSenderPort { get; set; }
        private int UDPListenerPort { get; set; }
        private int UDPSenderPort { get; set; }
        private string HupIPAddress { get; set; }

        private TcpClient tcpClientSocket;

        private TcpListener myTCPListener;
        private Thread tcpListenerThread;

        private bool IamRunning = true;

        public void init()
        {
            string path = System.IO.Directory.GetCurrentDirectory();

            try
            {
                string[] text = System.IO.File.ReadAllLines(path + "\\portConfig.txt");
                TCPSenderPort = Int32.Parse(text[0]);
                TCPListenerPort = Int32.Parse(text[1]);
                UDPSenderPort = Int32.Parse(text[2]);
                UDPListenerPort = Int32.Parse(text[3]);
                HupIPAddress = text[4];

                createSockets();
            }
            catch (Exception e)
            {
                Console.WriteLine("error opening portConfig.txt file");
            }
            
        }

        private void createSockets()
        {
            tcpListenerThread = new Thread(new ThreadStart(tcpListenersStart));
            tcpListenerThread.Start();
          
        }

       

        #region tcpListeners
        private void tcpListenersStart()
        {
            try
            {
                myTCPListener = new TcpListener(IPAddress.Any, TCPListenerPort);
                myTCPListener.Start();
                while (IamRunning == true)
                {
                    Console.WriteLine("The server is running at port 12001...");
                    Console.WriteLine("The local End point is  :" +
                                      myTCPListener.LocalEndpoint);
                    Console.WriteLine("Waiting for a connection.....");

                    Socket s = myTCPListener.AcceptSocket();
                    Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                    byte[] b = new byte[100];

                    int k = s.Receive(b);
                    Console.WriteLine("Recieved...");
                    string receivedString = System.Text.Encoding.UTF8.GetString(b);

                    if (receivedString.Contains(StartRecording))
                    {
                        startRecordingEvent(this);
                    }
                    else if (receivedString.Contains(StopRecording))
                    {
                        stopRecordingEvent(this);
                    }
                }
            }
            catch (Exception e)
            {

            }
            
        }
        #endregion

        #region interfaces

        public void sendReady()
        {
            sendTCPAsync(IamReady);
        }

        public async void sendTCPAsync(string message)
        {
            try
            {
                tcpClientSocket = new TcpClient(HupIPAddress, TCPSenderPort);
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                NetworkStream stream = tcpClientSocket.GetStream();

                // Send the message to the connected TcpServer. 
                await stream.WriteAsync(data, 0, data.Length);
                //stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Close everything.
                stream.Close();

            }
            catch
            {
                Console.WriteLine("error sending TCP message");
            }
        }

        public void close()
        {
            IamRunning = false;
            myTCPListener.Stop();
        }

        #endregion

    }
}

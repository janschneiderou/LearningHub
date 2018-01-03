using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;

namespace HubDesktop
{
    public class ApplicationClass
    {
        string IamReady = "<I AM READY>";
        string StartRecording = "<START RECORDING>";
        string StopRecording = "<STOP RECORDING>";

        private UdpClient receivingUdp;
        private Thread myRunningThread;

        private TcpListener myTCPListener;
        private Thread tcpListenerThread;

        public bool IamRunning = true;
        public bool isRunning = false;
        public bool isEnabled = false;
        bool newPackage = false;

        private TcpClient tcpClientSocket;
        int listeningPort;
        public string Path { get; set; }
        public bool remoteBool { get; set; }
        public int TCPListenerPort { get; set; }
        public int TCPSenderPort { get; set; }
        public int UDPListenerPort { get; set; }
        public int UDPSenderPort { get; set; }
        public bool usedBool { get; set; }
        public string Name { get; set; }
        string currentString { get; set; }
      

        MainWindow Parent;
        public bool isREady = false;

        #region initialization
        public ApplicationClass(string applicationName, string filePath, 
            bool remoteBool, int TCPListener, int TCPSender,  int UDPListener, 
            int UDPSender, bool usedBool, MainWindow Parent)
        {
            
            this.Path = filePath;
            this.Name = applicationName;
            this.Parent = Parent;
            this.remoteBool = remoteBool;
            this.TCPListenerPort = TCPListener;
            this.TCPSenderPort = TCPSender;
            this.UDPListenerPort = UDPListener;
            this.UDPSenderPort = UDPSender;
            this.usedBool = usedBool;

            createSockets();
            //receivingUdp = new UdpClient(this.listeningPort);
        }

        private void createSockets()
        {
            tcpListenerThread = new Thread(new ThreadStart(tcpListenersStart));
            tcpListenerThread.Start();
            
        }



        #endregion

        #region TCPSendingStuff

       

        public void sendStartRecording()
        {
            sendTCPAsync(StartRecording);
        }

        public void sendStopRecording()
        {
            sendTCPAsync(StopRecording);
        }

        public async void sendTCPAsync(string message)
        {
            try
            {
                string IPSendingAddress;
                if (remoteBool == false)
                {
                    IPSendingAddress = "127.0.0.1";
                }
                else
                {
                    IPSendingAddress = Path;
                }

                tcpClientSocket = new TcpClient(IPSendingAddress, TCPSenderPort);
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                NetworkStream stream = tcpClientSocket.GetStream();

                // Send the message to the connected TcpServer. 
                await stream.WriteAsync(data, 0, data.Length);
                //stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                stream.Close();
                tcpClientSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("error sending TCP message");
            }
        }

        #endregion


        #region TCPLinsteningStuff
        private void tcpListenersStart()
        {
            myTCPListener = new TcpListener(IPAddress.Any, TCPListenerPort);
            myTCPListener.Start();
            while (IamRunning==true)
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

                if (receivedString.Contains(IamReady))
                {
                    isREady = true;
                   
                }
            }
            myTCPListener.Stop();
        }
         #endregion

        public bool hasNewMessage()
        {
            return newPackage;
        }
        public string getCurrentString()
        {
            newPackage = false;
            return currentString;
        }
        //Starts application and reader for the UDP thread
        public void startApp()
        {
            try
            {
                
                string path = System.IO.Directory.GetCurrentDirectory();
                try
                {
                    if (Path.Equals("remoteApp"))
                    {
                        Console.WriteLine("application might be running remotely so thread and listener started");
                    }
                    else
                    {
                        string configfile = Path.Substring(0, Path.LastIndexOf("\\"))+"\\portConfig.txt";
                        if (File.Exists(configfile))
                        {
                            File.Delete(configfile);
                        }

                        // Create a new file 
                        using (StreamWriter sw = File.CreateText(configfile))
                        {
                            sw.WriteLine(TCPListenerPort);
                            sw.WriteLine(TCPSenderPort);
                            sw.WriteLine(UDPListenerPort);
                            sw.WriteLine(UDPSenderPort);
                            sw.WriteLine("127.0.0.1");

                        }
                        Directory.SetCurrentDirectory(Path.Substring(0, Path.LastIndexOf("\\")));
                        System.Diagnostics.Process.Start(Path);
                        
                       
                    }

                }
                catch
                {
                    Console.WriteLine("application might be running remotely so thread and listener started");
                }
                isRunning = true;
             //   myRunningThread = new Thread(new ThreadStart(myThreadFunction));
             //   myRunningThread.Start();
            }
            catch (Exception xx)
            {
                Console.WriteLine(xx);
            }
        }
        public void close()
        {
            IamRunning = false;
        }
        //closes application
        public void closeApp()
        {
            try
            {
                myRunningThread.Abort();
                if (Path.Equals("remoteApp"))
                {

                }
                else
                {
                    System.Diagnostics.Process[] pp1 = System.Diagnostics.Process.GetProcessesByName(Name);
                    pp1[0].CloseMainWindow();
                }

            }
            catch (Exception xx)
            {
                Console.WriteLine("I got an exception after closing App" + xx);
            }
            isRunning = false;
            IamRunning = false;
        }

        /// <summary>
        /// Thread receiving the UDP packages and forwarding them to the main class
        /// </summary>
        private void myThreadFunction()
        {
            while (isRunning == true)
            {
                //Creates an IPEndPoint to record the IP Address and port number of the sender. 
                // The IPEndPoint will allow you to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, listeningPort);
                try
                {

                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = receivingUdp.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    Console.WriteLine("This is the message you received " +
                                                 returnData);

                    currentString = returnData.ToString();
                    newPackage = true;
                    //if (Parent.directPush == true)
                    //{
                    //    Parent.storeString(currentString);
                    //}
                }

                catch (Exception e)
                {
                    Console.WriteLine("I got an exception in the Pen thread" + e.ToString());
                }
            }
        }
    }
}
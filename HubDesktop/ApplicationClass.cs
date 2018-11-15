/**
 * ****************************************************************************
 * Copyright (C) 2018 Das Deutsche Institut für Internationale Pädagogische Forschung (DIPF)
 * <p/>
 * This library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * <p/>
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * <p/>
 * You should have received a copy of the GNU Lesser General Public License
 * along with this library.  If not, see <http://www.gnu.org/licenses/>.
 * <p/>
 * Contributors: Jan Schneider
 * ****************************************************************************
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HubDesktop
{
    public class ApplicationClass
    {
        private readonly string IamReady = "<I AM READY>";
        private readonly string areYouReady = "<ARE YOU READY?>";
        private readonly string StartRecording = "<START RECORDING>";
        private readonly string endStartRecording = "</START RECORDING>";
        private readonly string StopRecording = "<STOP RECORDING>";
        private readonly string SendFile = "<SEND FILE>";
        private string currentFileName;
        private UdpClient receivingUdp;
        private TcpListener myTCPListener;

        private Thread tcpListenerThread;
        private Thread udpListenerThread;
        private Thread tcpFileThread;

        public bool iAmRunning = true;
        public bool isRunning = false;
        public bool isEnabled = false;
        public bool uploadReady = false;
        private bool newPackage = false;
        private readonly int TCPFileBufferSize = 1024;
        private TcpClient tcpClientSocket;
        public string Path { get; set; }
        public bool OneExecutableBool { get; set; }
        public bool RemoteBool { get; set; }
        public string Parameter { get; set; }
        public int TCPListenerPort { get; set; }
        public int TCPSenderPort { get; set; }
        public int TCPFile { get; set; }
        public int UDPListenerPort { get; set; }
        public int UDPSenderPort { get; set; }
        public bool UsedBool { get; set; }
        public bool IsVideo { get; set; }
        public string Name { get; set; }
        public string OneExeName { get; set; }
        private string CurrentUDPString { get; set; }

        private string recordingID;
        private MainWindow Parent;
        public bool isReady = false;

        #region initialization
        public ApplicationClass(string applicationName, string filePath, bool oneExecutableBool, string parameter,
            bool remoteBool, int TCPListener, int TCPSender, int tCPFile, int UDPListener,
            int UDPSender, bool usedBool, bool isVideo, MainWindow Parent)
        {

            Path = filePath;
            Name = applicationName;
            this.Parent = Parent;
            OneExecutableBool = oneExecutableBool;
            Parameter = parameter;
            RemoteBool = remoteBool;
            TCPListenerPort = TCPListener;
            TCPSenderPort = TCPSender;
            TCPFile = tCPFile;
            UDPListenerPort = UDPListener;
            UDPSenderPort = UDPSender;
            UsedBool = usedBool;
            IsVideo = isVideo;

            CreateSockets();
            //receivingUdp = new UdpClient(this.listeningPort);
        }

        private void CreateSockets()
        {
            tcpListenerThread = new Thread(new ThreadStart(TcpListenersStart))
            {
                IsBackground = true
            };
            tcpListenerThread.Start();

        }

        private void CreateUDPSockets()
        {
            receivingUdp = new UdpClient(UDPListenerPort);
            udpListenerThread = new Thread(new ThreadStart(MyUDPThreadFunction))
            {
                IsBackground = true
            };
            udpListenerThread.Start();
        }


        #endregion

        #region TCPSendingStuff



        public void SendStartRecording(string recordingID)
        {
            this.recordingID = recordingID;
            if (OneExecutableBool)
            {
                SendTCPAsync(StartRecording + recordingID + "," + Name + "_" + OneExeName + endStartRecording);
            }
            else
            {
                SendTCPAsync(StartRecording + recordingID + "," + Name + endStartRecording);
            }
        }



        public void SendStopRecording()
        {
            // Thread tcpFileThread;
            uploadReady = false;
            if (Directory.Exists(MainWindow.workingDirectory + "\\" + recordingID))
            {

            }
            else
            {
                DirectoryInfo di = Directory.CreateDirectory(MainWindow.workingDirectory + "\\" + recordingID);

            }
            if (IsVideo == false)
            {
                if (OneExecutableBool)
                {
                    currentFileName = MainWindow.workingDirectory + "\\" + recordingID + "\\" + recordingID + Name + OneExeName + ".json";
                }
                else
                {
                    currentFileName = MainWindow.workingDirectory + "\\" + recordingID + "\\" + recordingID + Name + ".json";
                }
                tcpFileThread = new Thread(new ThreadStart(ReceiveFileTCP));
                tcpFileThread.Start();
            }
            SendTCPAsync(StopRecording);
        }

        public async void SendTCPAsync(string message)
        {
            try
            {
                string IPSendingAddress;
                if (RemoteBool == false)
                {
                    IPSendingAddress = "127.0.0.1";
                }
                else
                {
                    IPSendingAddress = Path;
                }

                tcpClientSocket = new TcpClient(IPSendingAddress, TCPSenderPort);
                // Translate the passed message into ASCII and store it as a Byte array.
                byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

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
                Console.WriteLine(e);
                Console.WriteLine("error sending TCP message");
            }
        }

        #endregion


        #region TCPLinsteningStuff
        private void TcpListenersStart()
        {

            myTCPListener = new TcpListener(IPAddress.Any, TCPListenerPort);
            try
            {
                myTCPListener.Start();
            }
            catch
            {

            }

            while (iAmRunning)
            {
                try
                {
                    //client = myTCPListener.AcceptTcpClient();
                    Console.WriteLine("Waiting for a connection.....");

                    Socket s = myTCPListener.AcceptSocket();
                    Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                    byte[] b = new byte[100];

                    int k = s.Receive(b);
                    Console.WriteLine("Recieved...");
                    string receivedString = System.Text.Encoding.UTF8.GetString(b);

                    if (receivedString.Contains(IamReady))
                    {

                        isReady = true;
                        for (int i = 0; i < Parent.myEnabledApps.Count; i++)
                        {
                            if (Parent.myEnabledApps[i].Name == Name)
                            {
                                Parent.myEnabledApps[i].isReady = isReady;
                                break;
                            }
                        }

                    }
                    if (receivedString.Contains(SendFile))
                    {
                        HandleSendFileMessage(receivedString);
                    }
                    s.Close();
                    //  client.Close();
                }
                catch
                {

                }


            }
            myTCPListener.Stop();

            // myTCPListener.clo
        }

        private void HandleSendFileMessage(string receivedString)
        {
            uploadReady = false;
            //Thread tcpFileThread;
            //"<SEND FILE>myFile.avi</SEND FILE>"
            int startIndex = receivedString.IndexOf(">") + 1;
            int startIndex2 = receivedString.IndexOf("</") + 1;
            int startIndex3 = receivedString.IndexOf("");
            int length = startIndex2 - startIndex;
            string filename = receivedString.Substring(startIndex, length - 1);

            if (recordingID == null)
            {
                for (int i = 0; i < Parent.myEnabledApps.Count; i++)
                {
                    if (Parent.myEnabledApps[i].Name == Name)
                    {
                        recordingID = Parent.myEnabledApps[i].recordingID;
                        break;
                    }
                }
            }


            if (Directory.Exists(MainWindow.workingDirectory + "\\" + recordingID))
            {

            }
            else
            {
                DirectoryInfo di = Directory.CreateDirectory(MainWindow.workingDirectory + "\\" + recordingID);

            }

            currentFileName = MainWindow.workingDirectory + "\\" + recordingID + "\\" + filename;

            try
            {
                if (tcpFileThread != null)
                {
                    tcpFileThread.Abort();
                }
            }
            catch
            {

            }

            tcpFileThread = new Thread(new ThreadStart(ReceiveFileTCP));
            tcpFileThread.Start();
            SendTCPAsync(receivedString);


        }
        #endregion

        public bool HasNewMessage()
        {
            return newPackage;
        }
        public string GetCurrentString()
        {
            newPackage = false;
            return CurrentUDPString;
        }

        #region startStopapps
        //Starts application 
        public void StartApp()
        {
            try
            {

                string path = System.IO.Directory.GetCurrentDirectory();
                try
                {
                    if (RemoteBool)
                    {
                        SendTCPAsync(areYouReady);
                        Console.WriteLine("application might be running remotely so thread and listener started");
                        CreateUDPSockets();
                    }
                    else
                    {
                        if (OneExecutableBool)
                        {
                            string configfile = Path.Substring(0, Path.LastIndexOf("\\")) + "\\portConfig" + OneExeName + ".txt";
                            if (File.Exists(configfile))
                            {
                                File.Delete(configfile);
                            }

                            // Create a new file with an extra parameter using in distinguising one executable being using multiple times.
                            using (StreamWriter sw = File.CreateText(configfile))
                            {
                                sw.WriteLine(OneExeName);
                                sw.WriteLine(TCPListenerPort);
                                sw.WriteLine(TCPSenderPort);
                                sw.WriteLine(TCPFile);
                                sw.WriteLine(UDPListenerPort);
                                sw.WriteLine(UDPSenderPort);
                                sw.WriteLine("127.0.0.1");
                            }
                            Directory.SetCurrentDirectory(Path.Substring(0, Path.LastIndexOf("\\")));
                        }

                        else
                        {
                            string configfile = Path.Substring(0, Path.LastIndexOf("\\")) + "\\portConfig.txt";
                            if (File.Exists(configfile))
                            {
                                File.Delete(configfile);
                            }

                            // Create a new file 
                            using (StreamWriter sw = File.CreateText(configfile))
                            {
                                sw.WriteLine(TCPListenerPort);
                                sw.WriteLine(TCPSenderPort);
                                sw.WriteLine(TCPFile);
                                sw.WriteLine(UDPListenerPort);
                                sw.WriteLine(UDPSenderPort);
                                sw.WriteLine("127.0.0.1");
                            }
                            Directory.SetCurrentDirectory(Path.Substring(0, Path.LastIndexOf("\\")));
                        }

                        if (Parameter == "")
                        {
                            Process.Start(Path); //Very important line for Debug
                        }
                        else
                        {
                            Process p = new Process();
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.RedirectStandardError = true;
                            p.StartInfo.FileName = Path;
                            p.StartInfo.Arguments = Parameter;
                            p.Start();
                        }
                        CreateUDPSockets();
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

        public void Close()
        {
            myTCPListener.Stop();
        }

        public void CheckStartupPar()
        {
            string[] startupPar = Parameter.Split(null);
            if (startupPar.Any(s => s.Contains("-oen")))
            {
                int parIndex = Array.IndexOf(startupPar, "-oen");
                OneExeName = startupPar[parIndex + 1];
            }
        }

        private void CloseListenerThreads()
        {
            tcpListenerThread.Abort();
            udpListenerThread.Abort();
        }

        //closes application
        public void CloseApp()
        {
            Process[] pp1 = Process.GetProcessesByName(Name);
            Close();
            try
            {
                // myRunningThread.Abort();
                if (!Path.Equals("remoteApp") || !RemoteBool)
                {
                    pp1[0].CloseMainWindow();
                    pp1[0].WaitForExit();
                }
            }
            catch (Exception xx)
            {
                Console.WriteLine("I got an exception after closing App" + xx);
            }
            isRunning = false;
            iAmRunning = false;
            if (pp1.Length == 0)
            {
                CloseListenerThreads();
            }
        }
        #endregion

        #region UDPListeningStuff

        /// <summary>
        /// Thread receiving the UDP packages and forwarding them to the main class
        /// </summary>
        private void MyUDPThreadFunction()
        {
            while (isRunning)
            {
                //Creates an IPEndPoint to record the IP Address and port number of the sender. 
                // The IPEndPoint will allow you to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, UDPListenerPort);
                try
                {

                    // Blocks until a message returns on this socket from a remote host.
                    byte[] receiveBytes = receivingUdp.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    Console.WriteLine("This is the message you received " +
                                                 returnData);

                    CurrentUDPString = returnData.ToString();
                    newPackage = true;
                    HandleUDPPackage();
                }

                catch (Exception e)
                {
                    Console.WriteLine("I got an exception in the Pen thread" + e.ToString());
                }
            }
        }

        private void HandleUDPPackage()
        {
            Parent.HandleFeedback(CurrentUDPString);
        }
        #endregion

        #region receivingFiles
        public void ReceiveFileTCP()
        {
            TcpListener Listener = null;
            try
            {
                Listener = new TcpListener(IPAddress.Any, TCPFile);
                Listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            byte[] RecData = new byte[TCPFileBufferSize];
            int RecBytes;

            for (; ; )
            {
                TcpClient client = null;
                NetworkStream netstream = null;

                try
                {


                    if (Listener.Pending())
                    {
                        client = Listener.AcceptTcpClient();
                        netstream = client.GetStream();

                        int totalrecbytes = 0;
                        FileStream Fs = new FileStream
                        (currentFileName, FileMode.OpenOrCreate, FileAccess.Write);
                        while ((RecBytes = netstream.Read
                        (RecData, 0, RecData.Length)) > 0)
                        {
                            Fs.Write(RecData, 0, RecBytes);
                            totalrecbytes += RecBytes;
                        }
                        Fs.Close();


                        netstream.Close();
                        client.Close();
                        for (int i = 0; i < Parent.myEnabledApps.Count; i++)
                        {
                            if (Parent.myEnabledApps[i].Name == Name)
                            {
                                uploadReady = true;
                                Parent.myEnabledApps[i].uploadReady = true;
                                break;
                            }
                        }


                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //netstream.Close();
                }
            }
        }
        #endregion

    }
}
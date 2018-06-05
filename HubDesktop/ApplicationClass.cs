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
        string areYouReady = "<ARE YOU READY?>";
        string StartRecording = "<START RECORDING>";
        string endStartRecording = "</START RECORDING>";
        string StopRecording = "<STOP RECORDING>";
        string SendFile = "<SEND FILE>";
        string endSendFile = "</SEND FILE>";

        string currentFileName;
        private UdpClient receivingUdp;
        

        private TcpListener myTCPListener;
        private Thread tcpListenerThread;

        private Thread udpListenerThread;

        public bool IamRunning = true;
        public bool isRunning = false;
        public bool isEnabled = false;
        public bool uploadReady = false;
        bool newPackage = false;

        int TCPFileBufferSize = 1024;
        private TcpClient tcpClientSocket;
        int listeningPort;
        public string Path { get; set; }
        public bool remoteBool { get; set; }
        public int TCPListenerPort { get; set; }
        public int TCPSenderPort { get; set; }
        public int TCPFile { get; set; }
        public int UDPListenerPort { get; set; }
        public int UDPSenderPort { get; set; }
        public bool usedBool { get; set; }
        public string Name { get; set; }
        string currentUDPString { get; set; }
        string recordingID;
      

        MainWindow Parent;
        public bool isREady = false;

        #region initialization
        public ApplicationClass(string applicationName, string filePath, 
            bool remoteBool, int TCPListener, int TCPSender, int tCPFile,  int UDPListener, 
            int UDPSender, bool usedBool, MainWindow Parent)
        {
            
            this.Path = filePath;
            this.Name = applicationName;
            this.Parent = Parent;
            this.remoteBool = remoteBool;
            this.TCPListenerPort = TCPListener;
            this.TCPSenderPort = TCPSender;
            this.TCPFile = tCPFile;
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

        private void createUDPSockets()
        {
            receivingUdp = new UdpClient(this.UDPListenerPort);
            udpListenerThread = new Thread(new ThreadStart(myUDPThreadFunction));
            udpListenerThread.Start();
        }


        #endregion

        #region TCPSendingStuff

       

        public void sendStartRecording(string recordingID)
        {
            this.recordingID = recordingID;
            sendTCPAsync(StartRecording+recordingID+","+Name+endStartRecording);
        }

        

        public void sendStopRecording()
        {
            Thread tcpFileThread;
            uploadReady = false;
            if (Directory.Exists(MainWindow.workingDirectory + "\\" + recordingID))
            {
                
            }
            else
            {
                DirectoryInfo di = Directory.CreateDirectory(MainWindow.workingDirectory + "\\" + recordingID);

            }

            currentFileName = MainWindow.workingDirectory +"\\"+ recordingID + "\\"+recordingID+ Name +".json";
            tcpFileThread = new  Thread(new ThreadStart(ReceiveFileTCP));
            tcpFileThread.Start();
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
                try
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
                    if (receivedString.Contains(SendFile))
                    {
                        handleSendFileMessage(receivedString);
                    }
                }
                catch
                {

                }
                
            }
            myTCPListener.Stop();
           // myTCPListener.clo
        }

        private void handleSendFileMessage(string receivedString)
        {
            uploadReady = false;
            Thread tcpFileThread;
            //"<SEND FILE>myFile.avi</SEND FILE>"
            int startIndex = receivedString.IndexOf(">") + 1;
            int startIndex2 = receivedString.IndexOf("</") + 1;
            int startIndex3 = receivedString.IndexOf("");
            int length = startIndex2 - startIndex;
            string filename = receivedString.Substring(startIndex, length - 1);

            if (Directory.Exists(MainWindow.workingDirectory + "\\" + recordingID))
            {

            }
            else
            {
                DirectoryInfo di = Directory.CreateDirectory(MainWindow.workingDirectory + "\\" + recordingID);

            }

            currentFileName = MainWindow.workingDirectory + "\\" + recordingID + "\\" + filename;
            tcpFileThread = new Thread(new ThreadStart(ReceiveFileTCP));
            tcpFileThread.Start();
            sendTCPAsync(receivedString);
            

        }
        #endregion

        public bool hasNewMessage()
        {
            return newPackage;
        }
        public string getCurrentString()
        {
            newPackage = false;
            return currentUDPString;
        }

        #region startStopapps
        //Starts application 
        public void startApp()
        {
            try
            {
                
                string path = System.IO.Directory.GetCurrentDirectory();
                try
                {
                    if (remoteBool==true)
                    {
                        sendTCPAsync(areYouReady);
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
                            sw.WriteLine(TCPFile);
                            sw.WriteLine(UDPListenerPort);
                            sw.WriteLine(UDPSenderPort);
                            sw.WriteLine("127.0.0.1");
                        }

                        
                        Directory.SetCurrentDirectory(Path.Substring(0, Path.LastIndexOf("\\")));
                       System.Diagnostics.Process.Start(Path); //Very important line for Debug
                        createUDPSockets();


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
            myTCPListener.Stop();
            myTCPListener.ExclusiveAddressUse = false;
        }
        //closes application
        public void closeApp()
        {
            close();
            try
            {
               // myRunningThread.Abort();
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
        #endregion

        #region UDPListeningStuff

        /// <summary>
        /// Thread receiving the UDP packages and forwarding them to the main class
        /// </summary>
        private void myUDPThreadFunction()
        {
            while (isRunning == true)
            {
                //Creates an IPEndPoint to record the IP Address and port number of the sender. 
                // The IPEndPoint will allow you to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, this.UDPListenerPort);
                try
                {

                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = receivingUdp.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    Console.WriteLine("This is the message you received " +
                                                 returnData);

                    currentUDPString = returnData.ToString();
                    newPackage = true;
                    handleUDPPackage();
                }

                catch (Exception e)
                {
                    Console.WriteLine("I got an exception in the Pen thread" + e.ToString());
                }
            }
        }

        private void handleUDPPackage()
        {
            Parent.handleFeedback(currentUDPString);
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
                        uploadReady = true;

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
﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace ConnectorHubUW
{
    public class ConnectorHub
    {
        public delegate void StartRecordingDelegate(object sender);
        public event StartRecordingDelegate startRecordingEvent;

        public delegate void StopRecordingDelegate(object sender);
        public event StopRecordingDelegate stopRecordingEvent;

        Socket udpSendingSocket;
        IPEndPoint UDPendPoint;

        public bool startedByHub = false;

        public string areYouReady = "<ARE YOU READY?>";
        public string IamReady = "<I AM READY>";
        public string StartRecording = "<START RECORDING>";
        public string StopRecording = "<STOP RECORDING>";
        public string SendFile = "<SEND FILE>";
        public string endSendFile = "</SEND FILE>";

        private int TCPListenerPort { get; set; }
        private int TCPSenderPort { get; set; }
        private int TCPFilePort { get; set; }
        private int UDPListenerPort { get; set; }
        private int UDPSenderPort { get; set; }
        private string HupIPAddress { get; set; }

        private StreamSocket tcpClientSocket;

        private TcpListener myTCPListener;
        private Task tcpListenerThread;
        int TCPFileBufferSize = 1024;
        System.DateTime startRecordingTime;
        public string recordingID;
        string applicationName;
        private RecordingObject myRecordingObject;

        List<string> valuesNameDefinition;
        List<FrameObject> frames;

        private bool IamRunning = true;

        public void init()
        {

            string path = System.IO.Directory.GetCurrentDirectory();
            valuesNameDefinition = new List<string>();
            frames = new List<FrameObject>();
            Path.Combine(path, "portConfig.txt");
            string fileName = Path.Combine(path, "portConfig.txt");



            try
            {
                string[] text = File.ReadAllLines(fileName);
                TCPSenderPort = Int32.Parse(text[0]);
                TCPListenerPort = Int32.Parse(text[1]);
                TCPFilePort = Int32.Parse(text[2]);
                UDPSenderPort = Int32.Parse(text[3]);
                UDPListenerPort = Int32.Parse(text[4]);
                HupIPAddress = text[5];

                createSockets();
            }
            catch (Exception e)
            {
                try
                {



                    string[] text = File.ReadAllLines("portConfig");
                    TCPSenderPort = Int32.Parse(text[0]);
                    TCPListenerPort = Int32.Parse(text[1]);
                    TCPFilePort = Int32.Parse(text[2]);
                    UDPSenderPort = Int32.Parse(text[3]);
                    UDPListenerPort = Int32.Parse(text[4]);
                    HupIPAddress = text[5];

                    createSockets();
                }
                catch (Exception et)
                {
                    //Console.WriteLine("error opening portConfig.txt file");
                }
               // Console.WriteLine("error opening portConfig.txt file");
            }


        }

        private void createSockets()
        {
            udpSendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse(HupIPAddress);

            UDPendPoint = new IPEndPoint(serverAddr, UDPSenderPort);
            IamRunning = true;
            tcpListenerThread = Task.Factory.StartNew(() => tcpListenersStartAsync());

        }

        #region tcpListeners
        private async Task tcpListenersStartAsync()
        {
            try
            {
                myTCPListener = new TcpListener(IPAddress.Any, TCPListenerPort);
                myTCPListener.Start();
                while (IamRunning == true)
                {
                  

                    Socket s = await myTCPListener.AcceptSocketAsync();
                  //  Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                    byte[] b = new byte[100];

                    int k = s.Receive(b);
                   // Console.WriteLine("Recieved...");
                    string receivedString = System.Text.Encoding.UTF8.GetString(b);

                    if (receivedString.Contains(StartRecording))
                    {
                        doStartStuff(receivedString);

                    }
                    else if (receivedString.Contains(StopRecording))
                    {

                        doStopStuff();
                    }
                    else if (receivedString.Contains(SendFile))
                    {
                        handleSendFile(receivedString);
                    }
                    else if (receivedString.Contains(areYouReady))
                    {
                        sendReady();
                    }
                }
            }
            catch (Exception e)
            {

            }

        }


        private void doStopStuff()
        {
            int x = frames.Count;
            stopRecordingEvent(this);
            try
            {
                if (myRecordingObject.applicationName.Equals("ScreencaptureTest"))
                {

                }
                else
                {
                    string json = JsonConvert.SerializeObject(myRecordingObject, Formatting.Indented);
                    string path = Directory.GetCurrentDirectory();
                    string fileName = path + "\\" + myRecordingObject.recordingID + myRecordingObject.applicationName + ".json";
                    File.WriteAllText(fileName, json);
                    sendFileTCP(fileName);
                    x++;

                }
            }
            catch (Exception e)
            {
                int z = 0;
                z++;
            }
        }

        private void doStartStuff(string receivedString)
        {
            startRecordingTime = DateTime.Now;
            //"<START RECORDING>recordinID,ApplicationID</START RECORDING>"
            int startIndex = receivedString.IndexOf(">") + 1;
            int startIndex2 = receivedString.IndexOf(",") + 1;
            int startIndex3 = receivedString.IndexOf("</");
            int length = startIndex2 - startIndex;
            int length2 = startIndex3 - startIndex2;
            myRecordingObject = new RecordingObject();
            myRecordingObject.recordingID = receivedString.Substring(startIndex, length - 1);
            myRecordingObject.applicationName = receivedString.Substring(startIndex2, length2);
            startRecordingEvent(this);
            startedByHub = true;
        }

        private void handleSendFile(string receivedString)
        {
            //"<SEND FILE>myFile.avi</SEND FILE>"
            int startIndex = receivedString.IndexOf(">") + 1;
            int startIndex2 = receivedString.IndexOf("</") + 1;

            int length = startIndex2 - startIndex;
            string filename = receivedString.Substring(startIndex, length - 1);
            sendFileTCP(filename);
        }

        #endregion

        #region sendFile
        private async void sendFileTCP(string fileName)
        {
            byte[] SendingBuffer = null;
            StreamSocket client = null;

            NetworkStream netstream = null;
            try
            {
                client = new StreamSocket();

                Windows.Networking.HostName serverHost = new Windows.Networking.HostName(HupIPAddress);

                
                await client.ConnectAsync(serverHost, TCPFilePort.ToString());

                netstream = (NetworkStream)client.OutputStream;
                
                FileStream Fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                int NoOfPackets = Convert.ToInt32
             (Math.Ceiling(Convert.ToDouble(Fs.Length) / Convert.ToDouble(TCPFileBufferSize)));

                int TotalLength = (int)Fs.Length, CurrentPacketLength, counter = 0;
                for (int i = 0; i < NoOfPackets; i++)
                {
                    if (TotalLength > TCPFileBufferSize)
                    {
                        CurrentPacketLength = TCPFileBufferSize;
                        TotalLength = TotalLength - CurrentPacketLength;
                    }
                    else
                        CurrentPacketLength = TotalLength;
                    SendingBuffer = new byte[CurrentPacketLength];
                    Fs.Read(SendingBuffer, 0, CurrentPacketLength);
                    netstream.Write(SendingBuffer, 0, (int)SendingBuffer.Length);

                }

                Fs.Dispose();
                //Fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                netstream.Dispose();
                client.Dispose();

            }
        }

        #endregion

        #region interfaces

        public void sendFeedback(string feedback)
        {
            FeedbackObject f = new FeedbackObject(startRecordingTime, feedback, myRecordingObject.applicationName);
            string json = JsonConvert.SerializeObject(f, Formatting.Indented);
            byte[] send_buffer = Encoding.ASCII.GetBytes(json);
            udpSendingSocket.SendTo(send_buffer, UDPendPoint);
        }

        public void sendReady()
        {
            sendTCPAsync(IamReady);
        }

        public void setValuesName(List<string> valuesNameDefinition)
        {
            this.valuesNameDefinition = valuesNameDefinition;
        }
        public void storeFrame(List<string> frameValues)
        {
            try
            {
                FrameObject f = new FrameObject(startRecordingTime, valuesNameDefinition, frameValues);
                myRecordingObject.frames.Add(f);
            }
            catch
            {

            }

        }
        public async void sendTCPAsync(string message)
        {
            try
            {
                tcpClientSocket = new StreamSocket();

                Windows.Networking.HostName serverHost = new Windows.Networking.HostName(HupIPAddress);


                await tcpClientSocket.ConnectAsync(serverHost, TCPFilePort.ToString());

               

              //  tcpClientSocket = new TcpClient(HupIPAddress, TCPSenderPort);
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                NetworkStream stream = (NetworkStream)tcpClientSocket.OutputStream;

                // Send the message to the connected TcpServer. 
                await stream.WriteAsync(data, 0, data.Length);
                //stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Close everything.
                stream.Dispose();

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
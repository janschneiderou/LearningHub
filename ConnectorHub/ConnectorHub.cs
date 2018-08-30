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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace ConnectorHub
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

        private TcpClient tcpClientSocket;

        private TcpListener myTCPListener;
        private Thread tcpListenerThread;
        int TCPFileBufferSize = 1024;
        System.DateTime startRecordingTime;
        public string recordingID;
        string applicationName;
        private RecordingObject myRecordingObject;

        public bool amIvideo = false;

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
                    Console.WriteLine("error opening portConfig.txt file");
                }
                Console.WriteLine("error opening portConfig.txt file");
            }
            
        }

        private void createSockets()
        {
            udpSendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse(HupIPAddress);

            UDPendPoint = new IPEndPoint(serverAddr, UDPSenderPort);
            tcpListenerThread = new Thread(new ThreadStart(tcpListenersStart));
            tcpListenerThread.IsBackground = true;
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
                        doStartStuff(receivedString);
                        
                    }
                    else if (receivedString.Contains(StopRecording))
                    {
                        
                        doStopStuff();
                    }
                    else if(receivedString.Contains(SendFile))
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
                int x = 1;
                x++;
            }
            
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

        private void doStopStuff()
        {
            int x = frames.Count;
            stopRecordingEvent(this);
            try
            {
                if(myRecordingObject.applicationName.Equals("ScreencaptureTest")|| amIvideo==true)
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
            catch(Exception e)
            {
                int z = 0;
                z++;
            }
        }

        private void doStartStuff(string receivedString)
        {
            startRecordingTime = DateTime.Now;
            //"<START RECORDING>recordinID,ApplicationID</START RECORDING>"
            int startIndex = receivedString.IndexOf(">")+1;
            int startIndex2 = receivedString.IndexOf(",") + 1;
            int startIndex3 = receivedString.IndexOf("</");
            int length = startIndex2- startIndex;
            int length2 = startIndex3 - startIndex2;
            myRecordingObject = new RecordingObject();
            myRecordingObject.recordingID = receivedString.Substring(startIndex,length-1);
            myRecordingObject.applicationName = receivedString.Substring(startIndex2, length2);
            startRecordingEvent(this);
            startedByHub = true;
        }
        #endregion

        #region sendFile
        private void sendFileTCP(string fileName)
        {
            byte[] SendingBuffer = null;
            TcpClient client = null;
            
            NetworkStream netstream = null;
            try
            {
                client = new TcpClient(HupIPAddress, TCPFilePort);
                
                netstream = client.GetStream();
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

               
                     Fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                netstream.Close();
                client.Close();
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

        public void setValuesName( List<string> valuesNameDefinition)
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
            tcpListenerThread.Abort();
        }

        #endregion

    }
}

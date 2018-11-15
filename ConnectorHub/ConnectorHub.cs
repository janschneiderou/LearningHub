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

namespace ConnectorHub
{
    public class ConnectorHub
    {
        public delegate void StartRecordingDelegate(object sender);
        public event StartRecordingDelegate StartRecordingEvent;

        public delegate void StopRecordingDelegate(object sender);
        public event StopRecordingDelegate StopRecordingEvent;

        private Socket udpSendingSocket;
        private IPEndPoint UDPendPoint;

        public bool startedByHub = false;

        public string areYouReady = "<ARE YOU READY?>";
        public string IamReady = "<I AM READY>";
        public string StartRecording = "<START RECORDING>";
        public string StopRecording = "<STOP RECORDING>";
        public string SendFile = "<SEND FILE>";
        public string endSendFile = "</SEND FILE>";

        private bool oneExeBool;
        private string oneExePar;
        private string OneExeName { get; set; }
        private int TCPListenerPort { get; set; }
        private int TCPSenderPort { get; set; }
        private int TCPFilePort { get; set; }
        private int UDPListenerPort { get; set; }
        private int UDPSenderPort { get; set; }
        private string HupIPAddress { get; set; }

        private TcpClient tcpClientSocket;

        private TcpListener myTCPListener;
        private Thread tcpListenerThread;
        private readonly int TCPFileBufferSize = 1024;
        private System.DateTime startRecordingTime;
        public string recordingID;
        private RecordingObject myRecordingObject;
        public bool amIvideo = false;
        private List<string> valuesNameDefinition;
        private List<FrameObject> frames;

        private bool iAmRunning = true;

        public void Init()
        {
            bool oenBool = false;
            string[] startupPar = Environment.GetCommandLineArgs();
            valuesNameDefinition = new List<string>();
            frames = new List<FrameObject>();

            if (startupPar.Any(s => s.Contains("-oen")))
            {
                try
                {
                    CheckStartupPar(startupPar);
                    oenBool = true;
                    ReadPortConfig(oenBool);
                    CreateSockets();
                }
                catch (Exception e)
                {
                    Console.WriteLine("error opening portConfig.txt file");
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                try
                {
                    ReadPortConfig(oenBool);
                    CreateSockets();
                }
                catch (Exception e)
                {
                    Console.WriteLine("error opening portConfig.txt file");
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private void ReadPortConfig(bool oenBool)
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            string fileName;
            string[] text;
            if (oenBool)
            {
                fileName = Path.Combine(path, "portConfig" + oneExePar + ".txt");
                text = File.ReadAllLines(fileName);
                OneExeName = text[0].ToString();
                TCPSenderPort = int.Parse(text[1]);
                TCPListenerPort = int.Parse(text[2]);
                TCPFilePort = int.Parse(text[3]);
                UDPSenderPort = int.Parse(text[4]);
                UDPListenerPort = int.Parse(text[5]);
                HupIPAddress = text[6];
            }
            else
            {
                fileName = Path.Combine(path, "portConfig.txt");
                text = File.ReadAllLines(fileName);
                TCPSenderPort = int.Parse(text[0]);
                TCPListenerPort = int.Parse(text[1]);
                TCPFilePort = int.Parse(text[2]);
                UDPSenderPort = int.Parse(text[3]);
                UDPListenerPort = int.Parse(text[4]);
                HupIPAddress = text[5];
            }
        }

        private void CheckStartupPar(string[] startupPar)
        {
            int parIndex = Array.IndexOf(startupPar, "-oen");
            oneExePar = startupPar[parIndex + 1];
            oneExeBool = true;
        }

        private void CreateSockets()
        {
            udpSendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse(HupIPAddress);

            UDPendPoint = new IPEndPoint(serverAddr, UDPSenderPort);
            tcpListenerThread = new Thread(new ThreadStart(TcpListenersStart))
            {
                IsBackground = true
            };
            tcpListenerThread.Start();

        }

        #region tcpListeners
        private void TcpListenersStart()
        {
            try
            {
                myTCPListener = new TcpListener(IPAddress.Any, TCPListenerPort);
                myTCPListener.Start();
                while (iAmRunning)
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
                    string receivedstring = System.Text.Encoding.UTF8.GetString(b);

                    if (receivedstring.Contains(StartRecording))
                    {
                        StartRecordingFunction(receivedstring);

                    }
                    else if (receivedstring.Contains(StopRecording))
                    {
                        StopRecordingFunction();
                    }
                    else if (receivedstring.Contains(SendFile))
                    {
                        HandleSendFile(receivedstring);
                    }
                    else if (receivedstring.Contains(areYouReady))
                    {
                        SendReady();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                int x = 1;
                x++;
            }

        }

        private void HandleSendFile(string receivedstring)
        {
            //"<SEND FILE>myFile.avi</SEND FILE>"
            int startIndex = receivedstring.IndexOf(">") + 1;
            int startIndex2 = receivedstring.IndexOf("</") + 1;

            int length = startIndex2 - startIndex;
            string filename = receivedstring.Substring(startIndex, length - 1);
            SendFileTCP(filename);
        }

        private void StopRecordingFunction()
        {
            int x = frames.Count;
            StopRecordingEvent(this);
            try
            {
                if (myRecordingObject.ApplicationName.Equals("ScreencaptureTest") || amIvideo == true)
                {

                }
                else
                {
                    if (oneExeBool)
                    {
                        string json = JsonConvert.SerializeObject(myRecordingObject, Formatting.Indented);
                        string path = Directory.GetCurrentDirectory();
                        string fileName = path + "\\" + myRecordingObject.RecordingID + myRecordingObject.ApplicationName + ".json";
                        File.WriteAllText(fileName, json);
                        SendFileTCP(fileName);
                        x++;

                    }
                    else
                    {
                        string json = JsonConvert.SerializeObject(myRecordingObject, Formatting.Indented);
                        string path = Directory.GetCurrentDirectory();
                        string fileName = path + "\\" + myRecordingObject.RecordingID + myRecordingObject.ApplicationName + ".json";
                        File.WriteAllText(fileName, json);
                        SendFileTCP(fileName);
                        x++;
                    }


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                int z = 0;
                z++;
            }
        }

        private void StartRecordingFunction(string receivedstring)
        {
            startRecordingTime = DateTime.Now;
            //"<START RECORDING>recordinID,ApplicationID</START RECORDING>"

            if (oneExeBool)
            {
                int startIndex = receivedstring.IndexOf(">") + 1;
                int startIndex2 = receivedstring.IndexOf(",") + 1;
                int startIndex3 = receivedstring.IndexOf("_") + 1;
                int startIndex4 = receivedstring.IndexOf("</");
                int length = startIndex2 - startIndex;
                int length2 = startIndex3 - startIndex2 - 1;
                int length3 = startIndex4 - startIndex3;
                myRecordingObject = new RecordingObject
                {
                    RecordingID = receivedstring.Substring(startIndex, length - 1),
                    ApplicationName = receivedstring.Substring(startIndex2, length2),
                    OenName = receivedstring.Substring(startIndex3, length3)
                };
            }
            else
            {
                int startIndex = receivedstring.IndexOf(">") + 1;
                int startIndex2 = receivedstring.IndexOf(",") + 1;
                int startIndex3 = receivedstring.IndexOf("</");
                int length = startIndex2 - startIndex;
                int length2 = startIndex3 - startIndex2;
                myRecordingObject = new RecordingObject
                {
                    RecordingID = receivedstring.Substring(startIndex, length - 1),
                    ApplicationName = receivedstring.Substring(startIndex2, length2)
                };
            }

            StartRecordingEvent(this);
            startedByHub = true;
        }
        #endregion

        #region sendFile
        private void SendFileTCP(string fileName)
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

                int TotalLength = (int)Fs.Length, CurrentPacketLength;
                for (int i = 0; i < NoOfPackets; i++)
                {
                    if (TotalLength > TCPFileBufferSize)
                    {
                        CurrentPacketLength = TCPFileBufferSize;
                        TotalLength = TotalLength - CurrentPacketLength;
                    }
                    else
                    {
                        CurrentPacketLength = TotalLength;
                    }

                    SendingBuffer = new byte[CurrentPacketLength];
                    Fs.Read(SendingBuffer, 0, CurrentPacketLength);
                    netstream.Write(SendingBuffer, 0, SendingBuffer.Length);

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

        public void SendFeedback(string feedback)
        {
            FeedbackObject f = new FeedbackObject(startRecordingTime, feedback, myRecordingObject.ApplicationName);
            string json = JsonConvert.SerializeObject(f, Formatting.Indented);
            byte[] send_buffer = Encoding.ASCII.GetBytes(json);
            udpSendingSocket.SendTo(send_buffer, UDPendPoint);
        }

        public void SendReady()
        {
            SendTCPAsync(IamReady);
        }

        public void SetValuesName(List<string> valuesNameDefinition)
        {
            this.valuesNameDefinition = valuesNameDefinition;
        }
        public void StoreFrame(List<string> frameValues)
        {
            try
            {
                FrameObject f = new FrameObject(startRecordingTime, valuesNameDefinition, frameValues);
                myRecordingObject.Frames.Add(f);
            }
            catch
            {

            }

        }

        public async void SendTCPAsync(string message)
        {
            try
            {
                tcpClientSocket = new TcpClient(HupIPAddress, TCPSenderPort);
                // Translate the passed message into ASCII and store it as a Byte array.
                byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

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

        public void Close()
        {
            iAmRunning = false;
            myTCPListener.Stop();
            tcpListenerThread.Abort();
        }

        #endregion

    }
}

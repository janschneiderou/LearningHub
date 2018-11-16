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

//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;

namespace ConnectorHubUW
{
    public class ConnectorHub
    {
        public delegate void StartRecordingDelegate(object sender);
        public event StartRecordingDelegate StartRecordingEvent;

        public delegate void StopRecordingDelegate(object sender);
        public event StopRecordingDelegate StopRecordingEvent;

        // Socket udpSendingSocket;
        private IPEndPoint UDPendPoint;

        public bool startedByHub = false;

        public string areYouReady = "<ARE YOU READY?>";
        public string IamReady = "<I AM READY>";
        public string StartRecording = "<START RECORDING>";
        public string StopRecording = "<STOP RECORDING>";
        public string SendFile = "<SEND FILE>";
        public string endSendFile = "</SEND FILE>";

        private string TCPListenerPort { get; set; }
        private int TCPSenderPort { get; set; }
        private int TCPFilePort { get; set; }
        private int UDPListenerPort { get; set; }
        private int UDPSenderPort { get; set; }
        private string HupIPAddress { get; set; }

        private StreamSocket tcpClientSocket;
        private StreamSocketListener myTcpListenerSocket;

        // private TcpListener myTCPListener;
        private Task tcpListenerThread;
        private readonly int TCPFileBufferSize = 1024;
        private System.DateTime startRecordingTime;
        public string recordingID;

        private RecordingObject myRecordingObject;
        private List<string> valuesNameDefinition;
        private List<FrameObject> frames;

        public bool amIVideo = false;

        public bool IamRunning = false;

        public void Init()
        {

            string path = System.IO.Directory.GetCurrentDirectory();
            valuesNameDefinition = new List<string>();
            frames = new List<FrameObject>();
            Path.Combine(path, "portConfig.txt");
            string fileName = Path.Combine(path, "portConfig.txt");



            try
            {
                string[] text = File.ReadAllLines(fileName);
                TCPSenderPort = int.Parse(text[0]);
                TCPListenerPort = text[1];
                TCPFilePort = int.Parse(text[2]);
                UDPSenderPort = int.Parse(text[3]);
                UDPListenerPort = int.Parse(text[4]);
                HupIPAddress = text[5];

                CreateSockets();
            }
            catch (Exception)
            {
                try
                {
                    string[] text = File.ReadAllLines("portConfig");
                    TCPSenderPort = int.Parse(text[0]);
                    TCPListenerPort = text[1];
                    TCPFilePort = int.Parse(text[2]);
                    UDPSenderPort = int.Parse(text[3]);
                    UDPListenerPort = int.Parse(text[4]);
                    HupIPAddress = text[5];

                    CreateSockets();
                }
                catch (Exception)
                {
                    //Console.WriteLine("error opening portConfig.txt file");
                }
                // Console.WriteLine("error opening portConfig.txt file");
            }


        }

        private void CreateSockets()
        {
            //  udpSendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            //   ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse(HupIPAddress);

            UDPendPoint = new IPEndPoint(serverAddr, UDPSenderPort);
            IamRunning = true;
            tcpListenerThread = Task.Factory.StartNew(() => TcpListenersStartAsync());

        }

        #region tcpListeners
        private async Task TcpListenersStartAsync()
        {
            try
            {
                //     myTCPListener = new TcpListener(IPAddress.Any, TCPListenerPort);
                //     myTCPListener.Start();

                myTcpListenerSocket = new StreamSocketListener();

                // The ConnectionReceived event is raised when connections are received.
                myTcpListenerSocket.ConnectionReceived += MyTcpListenerSocket_ConnectionReceived;
                await myTcpListenerSocket.BindServiceNameAsync(TCPListenerPort);


            }
            catch (Exception)
            {

            }

        }

        private async void MyTcpListenerSocket_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {

            string receivedString;
            using (StreamReader streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                receivedString = await streamReader.ReadLineAsync();
            }

            //byte[] b = new byte[100];

            ////      int k = s.Receive(b);
            //// Console.WriteLine("Recieved...");
            //string receivedString = System.Text.Encoding.UTF8.GetString(b);

            if (receivedString.Contains(StartRecording))
            {
                StartRecordingFunction(receivedString);

            }
            else if (receivedString.Contains(StopRecording))
            {

                StopRecordingFunction();
            }
            else if (receivedString.Contains(SendFile))
            {
                HandleSendFile(receivedString);
            }
            else if (receivedString.Contains(areYouReady))
            {
                SendReady();
            }

        }

        private async void StopRecordingFunction()
        {
            int x = frames.Count;
            StopRecordingEvent(this);
            try
            {
                if (myRecordingObject.ApplicationName.Equals("ScreencaptureTest") || amIVideo == true)
                {

                }
                else
                {
                    JsonObject jsonRecordingObject = new JsonObject
                    {
                        { "recordingID", JsonValue.CreateStringValue(myRecordingObject.RecordingID) },
                        { "applicationName", JsonValue.CreateStringValue(myRecordingObject.ApplicationName) }
                    };

                    JsonArray jsonRecordingFrames = new JsonArray();
                    JsonObject[] jsonFrame = new JsonObject[myRecordingObject.Frames.Count];
                    int i = 0;
                    foreach (FrameObject f in myRecordingObject.Frames)
                    {
                        jsonFrame[i] = new JsonObject
                        {
                            { "framestamp", JsonValue.CreateStringValue(f.frameStamp.ToString()) }
                        };
                        JsonObject jsonFrameAttributes = new JsonObject();
                        foreach (KeyValuePair<string, string> attributes in f.frameAttributes)
                        {
                            jsonFrameAttributes.Add(attributes.Key, JsonValue.CreateStringValue(attributes.Value));
                        }
                        jsonFrame[i]["frameAttributes"] = jsonFrameAttributes;
                        i++;
                    }
                    for (int j = 0; j < i; j++)
                    {
                        jsonRecordingFrames.Add(jsonFrame[j]);
                    }

                    jsonRecordingObject["frames"] = jsonRecordingFrames;

                    string json = jsonRecordingObject.Stringify();

                    string fileName = myRecordingObject.RecordingID + "" + myRecordingObject.ApplicationName + ".json";
                    Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                    Windows.Storage.StorageFile recordingFile = await storageFolder.CreateFileAsync(fileName,
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);

                    await Windows.Storage.FileIO.WriteTextAsync(recordingFile, json);

                    SendFileTCP(recordingFile.Path);
                    x++;

                }
            }
            catch (Exception)
            {
                int z = 0;
                z++;
            }
        }

        private void StartRecordingFunction(string receivedString)
        {
            startRecordingTime = DateTime.Now;
            //"<START RECORDING>recordinID,ApplicationID</START RECORDING>"
            int startIndex = receivedString.IndexOf(">") + 1;
            int startIndex2 = receivedString.IndexOf(",") + 1;
            int startIndex3 = receivedString.IndexOf("</");
            int length = startIndex2 - startIndex;
            int length2 = startIndex3 - startIndex2;
            myRecordingObject = new RecordingObject
            {
                RecordingID = receivedString.Substring(startIndex, length - 1),
                ApplicationName = receivedString.Substring(startIndex2, length2)
            };
            StartRecordingEvent(this);
            startedByHub = true;
        }

        private void HandleSendFile(string receivedString)
        {
            //"<SEND FILE>myFile.avi</SEND FILE>"
            int startIndex = receivedString.IndexOf(">") + 1;
            int startIndex2 = receivedString.IndexOf("</") + 1;

            int length = startIndex2 - startIndex;
            string filename = receivedString.Substring(startIndex, length - 1);
            SendFileTCP(filename);
        }

        #endregion

        #region sendFile
        private async void SendFileTCP(string fileName)
        {
            byte[] SendingBuffer = null;
            StreamSocket client = null;

            //    NetworkStream netstream = null;
            try
            {
                client = new StreamSocket();

                Windows.Networking.HostName serverHost = new Windows.Networking.HostName(HupIPAddress);


                await client.ConnectAsync(serverHost, TCPFilePort.ToString());

                //   netstream = (NetworkStream)client.OutputStream;

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



                    using (Stream outputStream = client.OutputStream.AsStreamForWrite())
                    {
                        await outputStream.WriteAsync(SendingBuffer, 0, SendingBuffer.Length);
                        await outputStream.FlushAsync();

                    }
                    //   netstream.Write(SendingBuffer, 0, (int)SendingBuffer.Length);

                }

                Fs.Dispose();
                //Fs.Close();
            }
            catch (Exception)
            {
                // Console.WriteLine(ex.Message);
            }
            finally
            {
                //    netstream.Dispose();
                client.Dispose();

            }
        }

        #endregion

        #region interfaces

        public void SendFeedback(string feedback)
        {
            FeedbackObject f = new FeedbackObject(startRecordingTime, feedback, myRecordingObject.ApplicationName);
            //  string json = JsonConvert.SerializeObject(f, Formatting.Indented);
            //  byte[] send_buffer = Encoding.ASCII.GetBytes(json);
            //    udpSendingSocket.SendTo(send_buffer, UDPendPoint);
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
                tcpClientSocket = new StreamSocket();

                Windows.Networking.HostName serverHost = new Windows.Networking.HostName(HupIPAddress);


                await tcpClientSocket.ConnectAsync(serverHost, TCPSenderPort.ToString());


                using (Stream outputStream = tcpClientSocket.OutputStream.AsStreamForWrite())
                {
                    using (StreamWriter streamWriter = new StreamWriter(outputStream))
                    {
                        await streamWriter.WriteLineAsync(message);
                        await streamWriter.FlushAsync();
                    }
                }

            }
            catch
            {
                // Console.WriteLine("error sending TCP message");
            }
        }

        public void Close()
        {
            IamRunning = false;

            //  myTCPListener.Stop();
        }

        #endregion
    }
}

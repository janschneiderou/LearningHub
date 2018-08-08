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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Data.Json;

namespace ConnectorHubUW
{
    public class ConnectorHub
    {
        public delegate void StartRecordingDelegate(object sender);
        public event StartRecordingDelegate startRecordingEvent;

        public delegate void StopRecordingDelegate(object sender);
        public event StopRecordingDelegate stopRecordingEvent;

       // Socket udpSendingSocket;
        IPEndPoint UDPendPoint;

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
        int TCPFileBufferSize = 1024;
        System.DateTime startRecordingTime;
        public string recordingID;

        private RecordingObject myRecordingObject;

        List<string> valuesNameDefinition;
        List<FrameObject> frames;

        public bool amIVideo = false;

        public bool IamRunning = false;

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
                TCPListenerPort = text[1];
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
                    TCPListenerPort = text[1];
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
          //  udpSendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
         //   ProtocolType.Udp);

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
                //     myTCPListener = new TcpListener(IPAddress.Any, TCPListenerPort);
                //     myTCPListener.Start();

                myTcpListenerSocket = new StreamSocketListener();

                // The ConnectionReceived event is raised when connections are received.
                myTcpListenerSocket.ConnectionReceived += this.myTcpListenerSocket_ConnectionReceived;
                await myTcpListenerSocket.BindServiceNameAsync(TCPListenerPort);

               
            }
            catch (Exception e)
            {

            }

        }

        private async void myTcpListenerSocket_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {

            string receivedString;
            using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                receivedString = await streamReader.ReadLineAsync();
            }

            //byte[] b = new byte[100];

            ////      int k = s.Receive(b);
            //// Console.WriteLine("Recieved...");
            //string receivedString = System.Text.Encoding.UTF8.GetString(b);

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

        private async void doStopStuff()
        {
            int x = frames.Count;
            stopRecordingEvent(this);
            try
            {
                if (myRecordingObject.applicationName.Equals("ScreencaptureTest")||amIVideo==true)
                {

                }
                else
                {
                    JsonObject jsonRecordingObject = new JsonObject();
                    jsonRecordingObject.Add("recordingID", JsonValue.CreateStringValue(myRecordingObject.recordingID));
                    jsonRecordingObject.Add("applicationName", JsonValue.CreateStringValue(myRecordingObject.applicationName));

                    JsonArray jsonRecordingFrames = new JsonArray();
                    JsonObject[] jsonFrame = new JsonObject[myRecordingObject.frames.Count];
                    int i = 0;
                    foreach(FrameObject f in myRecordingObject.frames)
                    {
                        jsonFrame[i] = new JsonObject();
                        jsonFrame[i].Add("framestamp", JsonValue.CreateStringValue(f.frameStamp.ToString()));
                        JsonObject jsonFrameAttributes = new JsonObject();
                        foreach(KeyValuePair<string, string> attributes in f.frameAttributes)
                        {
                            jsonFrameAttributes.Add(attributes.Key, JsonValue.CreateStringValue(attributes.Value));
                        }
                        jsonFrame[i]["frameAttributes"] = jsonFrameAttributes;
                        i++;
                    }
                    for(int j=0;j<i;j++)
                    {
                        jsonRecordingFrames.Add(jsonFrame[j]);
                    }

                    jsonRecordingObject["frames"] = jsonRecordingFrames; 

                    string json = jsonRecordingObject.Stringify();
                    
                    string fileName = myRecordingObject.recordingID +""+ myRecordingObject.applicationName + ".json";
                    Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                    Windows.Storage.StorageFile recordingFile = await storageFolder.CreateFileAsync(fileName,
        Windows.Storage.CreationCollisionOption.ReplaceExisting);

                    await Windows.Storage.FileIO.WriteTextAsync(recordingFile, json);
                    
                    sendFileTCP(recordingFile.Path);
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

                    

                    using (Stream outputStream = client.OutputStream.AsStreamForWrite())
                    {
                        await outputStream.WriteAsync(SendingBuffer, 0, (int)SendingBuffer.Length);
                        await outputStream.FlushAsync();
                        
                    }
                    //   netstream.Write(SendingBuffer, 0, (int)SendingBuffer.Length);

                }

                Fs.Dispose();
                //Fs.Close();
            }
            catch (Exception ex)
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

        public void sendFeedback(string feedback)
        {
            FeedbackObject f = new FeedbackObject(startRecordingTime, feedback, myRecordingObject.applicationName);
          //  string json = JsonConvert.SerializeObject(f, Formatting.Indented);
          //  byte[] send_buffer = Encoding.ASCII.GetBytes(json);
        //    udpSendingSocket.SendTo(send_buffer, UDPendPoint);
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


                await tcpClientSocket.ConnectAsync(serverHost, TCPSenderPort.ToString());

                
                using (Stream outputStream = tcpClientSocket.OutputStream.AsStreamForWrite())
                {
                    using (var streamWriter = new StreamWriter(outputStream))
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

        public void close()
        {
            IamRunning = false;
          //  myTCPListener.Stop();
        }

        #endregion
    }
}

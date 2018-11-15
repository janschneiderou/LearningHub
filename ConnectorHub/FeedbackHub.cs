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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConnectorHub
{
    public class FeedbackHub
    {
        public delegate void feedbackReceivedDelegate(object sender, string feedback);
        public event feedbackReceivedDelegate FeedbackReceivedEvent;

        private int TCPListenerPort { get; set; }
        private int UDPListenerPort { get; set; }

        private UdpClient receivingUdp;
        private Thread udpListenerThread;

        private string currentUDPString;
        private bool isRunning;

        public void Init()
        {
            string path = System.IO.Directory.GetCurrentDirectory();


            string fileName = Path.Combine(path, "feedbackPortConfig.txt");
            try
            {
                string[] text = File.ReadAllLines(fileName);

                TCPListenerPort = int.Parse(text[0]);
                UDPListenerPort = int.Parse(text[1]);

                CreateSockets();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("error opening feedbackPortConfig.txt file");
            }
        }

        private void CreateSockets()
        {
            receivingUdp = new UdpClient(UDPListenerPort);
            Thread thread = new Thread(new ThreadStart(MyUDPThreadFunction))
            {
                IsBackground = true
            };
            udpListenerThread = thread;
            isRunning = true;
            udpListenerThread.Start();
        }

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

                    currentUDPString = returnData.ToString();

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
            FeedbackReceivedEvent(this, currentUDPString);
        }


        public void Close()
        {
            //IamRunning = false;
            receivingUdp.Close();
            udpListenerThread.Abort();
        }
    }
}

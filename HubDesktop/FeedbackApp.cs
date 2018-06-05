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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HubDesktop
{
    public class FeedbackApp
    {
        public string Path { get; set; }
        public int TCPSenderPort { get; set; }
        public int UDPSenderPort { get; set; }
        private TcpClient tcpClientSocket;
        Socket udpSendingSocket;
        IPEndPoint UDPendPoint;


        public FeedbackApp(string Path, int TCPSenderPort, int UDPSenderPort)
        {
            this.Path = Path;
            this.TCPSenderPort = TCPSenderPort;
            this.UDPSenderPort = UDPSenderPort;
            udpSendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
          ProtocolType.Udp);
            IPAddress serverAddr = IPAddress.Parse(Path);
            UDPendPoint = new IPEndPoint(serverAddr, UDPSenderPort);
        }

        public void sendUDP(string message)
        {
            try
            {
                byte[] send_buffer = Encoding.ASCII.GetBytes(message);
                udpSendingSocket.SendTo(send_buffer, UDPendPoint);
            }
            catch
            {

            }
            
        }

        public async void sendTCPAsync(string message)
        {
            try
            {
                string IPSendingAddress= Path;
               

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
    }
}

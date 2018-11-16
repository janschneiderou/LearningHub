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
using System.IO;
using System.Linq;
using System.Net;
//using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ConnectorHubUW
{
    public class FeedbackHub
    {
        public delegate void feedbackReceivedDelegate(object sender, string feedback);
        public event feedbackReceivedDelegate FeedbackReceivedEvent;

        private int TCPListenerPort { get; set; }
        private int UDPListenerPort { get; set; }

        private DatagramSocket receivingUdp;
        private string currentUDPString;

        public FeedbackHub()
        {
            
            
        }

        public void Init(int TCPListenerPort, int UDPListenerPort)
        {
            this.TCPListenerPort = TCPListenerPort;
            this.UDPListenerPort = UDPListenerPort;
            CreateSocketsAsync();
        }

        public void Init()
        {
            

            try
            {
                string path = System.IO.Directory.GetCurrentDirectory();
                string fileName = Path.Combine(path, "feedbackPortConfig.txt");
            
                string[] text = File.ReadAllLines(fileName);

                TCPListenerPort = Int32.Parse(text[0]);
                UDPListenerPort = Int32.Parse(text[1]);

                CreateSocketsAsync();
            }
            catch (Exception)
            {
                TCPListenerPort = 15002;
                UDPListenerPort = 16002;
                CreateSocketsAsync();
                //  Console.WriteLine("error opening feedbackPortConfig.txt file");
            }
        }

        private async void CreateSocketsAsync()
        {
            receivingUdp = new DatagramSocket();
            receivingUdp.MessageReceived += ReceivingUdp_MessageReceived;

            await receivingUdp.BindServiceNameAsync(UDPListenerPort.ToString());

        }

        private void ReceivingUdp_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            string request;
            using (DataReader dataReader = args.GetDataReader())
            {
                request = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                currentUDPString = request;
                HandleUDPPackage();
            }
        }

       
        private void HandleUDPPackage()
        {
            FeedbackReceivedEvent(this, currentUDPString);
        }
    }
}

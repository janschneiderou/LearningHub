using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectorHub
{
    public class FeedbackHub
    {
        public delegate void feedbackReceivedDelegate(object sender, string feedback);
        public event feedbackReceivedDelegate feedbackReceivedEvent;

        private int TCPListenerPort { get; set; }
        private int UDPListenerPort { get; set; }

        private UdpClient receivingUdp;
        private Thread udpListenerThread;

        private TcpListener myTCPListener;
        private Thread tcpListenerThread;

        private string currentUDPString;

        bool isRunning;

        public void init()
        {
            string path = System.IO.Directory.GetCurrentDirectory();

            
            string fileName = Path.Combine(path, "feedbackPortConfig.txt");
            try
            {
                string[] text = File.ReadAllLines(fileName);
            
                TCPListenerPort = Int32.Parse(text[0]);
                UDPListenerPort = Int32.Parse(text[1]);

                createSockets();
            }
            catch (Exception e)
            {
               
                Console.WriteLine("error opening feedbackPortConfig.txt file");
            }
        }

        private void createSockets()
        {
            receivingUdp = new UdpClient(this.UDPListenerPort);
            udpListenerThread = new Thread(new ThreadStart(myUDPThreadFunction));
            isRunning = true;
            udpListenerThread.Start();
        }

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
            feedbackReceivedEvent(this, currentUDPString);
        }
    }
}

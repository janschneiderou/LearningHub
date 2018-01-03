using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;

namespace LearningHub.Classes
{
    class ApplicationClass
    {
        private UdpClient receivingUdp;
        private Thread myRunningThread;
        public bool isRunning = false;
        public bool isEnabled = false;
        bool newPackage = false;
        int listeningPort;
        string filePath;
        public string applicationName;
        string currentString;
        Controller Parent;
          
        public ApplicationClass(string applicationName, string filePath, 
            string remoteBool, string tCPListener, string tCPSender,  string uDPListener, 
            string uDPSender, string usedBool, Controller Parent)
        {
            //this.listeningPort = listeningPort;
            this.filePath = filePath;
            this.applicationName = applicationName;
            this.Parent = Parent;
            //receivingUdp = new UdpClient(this.listeningPort);
        }

        public bool hasNewMessage()
        {
            return newPackage;
        }
        public string getCurrentString()
        {
            newPackage = false;
            return currentString;
        }
        //Starts application and reader for the UDP thread
        public void startApp()
        {
            try
            {
                string path = System.IO.Directory.GetCurrentDirectory();
                try
                {
                    if (filePath.Equals("remoteApp"))
                    {
                        Console.WriteLine("application might be running remotely so thread and listener started");
                    }
                    else
                    {
                        //string filePaths = "explorer.exe";
                        //// create process instance
                        //Process myprocess = new Process();
                        //// set the file path which you want in process
                        //myprocess.StartInfo.FileName = filePaths;
                        //// take the administrator permision to run process
                        //myprocess.StartInfo.Verb = "runas";
                        //// start process
                        //myprocess.Start();

                        //// Define the string value to assign to a new secure string.
                        // char[] chars = { 'v', 'j', '6', '5','q','g','d','p' };
                        // // Instantiate the secure string.
                        // System.Security.SecureString testString = new System.Security.SecureString();
                        // // Assign the character array to the secure string.
                        // foreach (char ch in chars)
                        //     testString.AppendChar(ch);

                        // System.Diagnostics.Process.Start(filePath, "jan", testString, "C:\\Users\\jan");
                        System.Diagnostics.Process.Start(filePath);
                        //System.Diagnostics.Process.Start(filePaths);
                       
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

        //closes application
        public void closeApp()
        {
            try
            {
                myRunningThread.Abort();
                if (filePath.Equals("remoteApp"))
                {

                }
                else
                {
                    System.Diagnostics.Process[] pp1 = System.Diagnostics.Process.GetProcessesByName(applicationName);
                    pp1[0].CloseMainWindow();
                }

            }
            catch (Exception xx)
            {
                Console.WriteLine("I got an exception after closing App" + xx);
            }
            isRunning = false;
        }

        /// <summary>
        /// Thread receiving the UDP packages and forwarding them to the main class
        /// </summary>
        private void myThreadFunction()
        {
            while (isRunning == true)
            {
                //Creates an IPEndPoint to record the IP Address and port number of the sender. 
                // The IPEndPoint will allow you to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, listeningPort);
                try
                {

                    // Blocks until a message returns on this socket from a remote host.
                    Byte[] receiveBytes = receivingUdp.Receive(ref RemoteIpEndPoint);

                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    Console.WriteLine("This is the message you received " +
                                                 returnData);

                    currentString = returnData.ToString();
                    newPackage = true;
                    //if (Parent.directPush == true)
                    //{
                    //    Parent.storeString(currentString);
                    //}
                }

                catch (Exception e)
                {
                    Console.WriteLine("I got an exception in the Pen thread" + e.ToString());
                }
            }
        }
    }
}
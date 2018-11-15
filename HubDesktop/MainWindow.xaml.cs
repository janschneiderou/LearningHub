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


using ConnectorHub;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HubDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ApplicationClass> myApps;
        public List<ApplicationClass> myEnabledApps;

        List<FeedbackApp> myFeedbacks;
        List<LAApplication> myLAApps;

        bool restart = false;
        Recording myRecordingInterface;
        DataSet appsDataSet;
        DataSet LAAppsDataSet;
        DataSet FeedbackAppsDataSet;
        public static string workingDirectory;
        public FeedbackObject lastFeedbackSent;

        public static string currentUser;

        // variables for remote control
        public enum States { menu, recordingReady, isRecording, RecordingStop };
        public static States myState;
        Thread RemoteControlThread;
        private TcpListener myRemoteControlTCPListener;
        bool IamRunning = true;
        string startAPPs = "<START APPLICATIONS>";
        string startRecording = "<START RECORDING>";
        string stopRecording = "<STOP RECORDING>";
        string finish = "<FINISH>";
        int controlPortNumber;

        public MainWindow()
        {
            InitializeComponent();
            workingDirectory = Directory.GetCurrentDirectory();
            lastFeedbackSent = new FeedbackObject(DateTime.Now, "","");
            myApps = new List<ApplicationClass>();
            myEnabledApps = new List<ApplicationClass>();

            myFeedbacks = new List<FeedbackApp>();
            myLAApps = new List<LAApplication>();
            SetAppsTable();
            Loaded += MainWindow_Loaded;

            //System.Diagnostics.Process.Start(@"C:\Users\jan\source\repos\LearningHub\HubDesktop\bin\Debug\restart.bat");

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            myState = States.menu;
            controlPortNumber = Int32.Parse(controlPort.Text);
            RemoteControlThread = new Thread(new ThreadStart(RemoteControlStart));
            RemoteControlThread.Start();
        }

        public void StartAgain()
        {
            MainCanvas.Children.Remove(myRecordingInterface);
            myRecordingInterface = null;

            //TODO
            string pathString = workingDirectory + "\\restart.bat";
            string pathApp = workingDirectory + "\\HubDesktop.exe";



            if (!System.IO.File.Exists(pathString))
            {
                StreamWriter w = new StreamWriter(pathString);
                w.WriteLine("timeout /t  5");
                w.WriteLine("Start "+"\"\" \"" + pathApp +"\"");
                w.Close();


            }

            //restart = true;
            Directory.SetCurrentDirectory(workingDirectory);
            System.Diagnostics.Process.Start(pathString);
            Application.Current.MainWindow.Close();

            //Application.Current.Shutdown();


        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach(ApplicationClass app in myEnabledApps)
            {
                app.CloseApp();
            }
            myEnabledApps = null;
            IamRunning = false;
            try
            {
                myRecordingInterface.isOpen = false;
            }
           catch
            {

            }
            //base.OnExiting(sender, e);
            if (restart )
            {
                
               // System.Diagnostics.Process.Start(workingDirectory + "\\HubDeskTop.exe"); //Very important line for Debug
            }
            Environment.Exit(Environment.ExitCode);
          
        }

        private void SetAppsTable()
        {
 
            appsDataSet = new DataSet();
            appsDataSet.ReadXmlSchema(Directory.GetCurrentDirectory() + "\\DataConfig\\AppsSchema.xsd");
            appsDataSet.ReadXml(Directory.GetCurrentDirectory() + "\\DataConfig\\AppsConfig.xml");
            
            AppsGrid.ItemsSource = appsDataSet.Tables[0].DefaultView;


            LAAppsDataSet = new DataSet();
            LAAppsDataSet.ReadXmlSchema(Directory.GetCurrentDirectory() + "\\DataConfig\\LASchema.xsd");
            LAAppsDataSet.ReadXml(Directory.GetCurrentDirectory() + "\\DataConfig\\LAConfig.xml");

            LAAppsGrid.ItemsSource = LAAppsDataSet.Tables[0].DefaultView;

            FeedbackAppsDataSet = new DataSet();
            FeedbackAppsDataSet.ReadXmlSchema(Directory.GetCurrentDirectory() + "\\DataConfig\\FeedbackSchema.xsd");
            FeedbackAppsDataSet.ReadXml(Directory.GetCurrentDirectory() + "\\DataConfig\\FeedbackConfig.xml");

            FeedbackAppsGrid.ItemsSource = FeedbackAppsDataSet.Tables[0].DefaultView;
        }

        public async Task HandleXAPIAsync()
        {
            if(lastFeedbackSent.verb.Contains("Good"))
            {

            }
            else
            {
                XAPIStuff.XAPIActor actor = new XAPIStuff.XAPIActor("id_" + lastFeedbackSent.applicationName, "application", lastFeedbackSent.applicationName);
                XAPIStuff.XAPIVerb verb = new XAPIStuff.XAPIVerb("id", lastFeedbackSent.verb);
                XAPIStuff.XAPIObject myObject = new XAPIStuff.XAPIObject("student", "id_student");
                TimeSpan ts = new TimeSpan(DateTime.Now.Subtract(lastFeedbackSent.frameStamp).Ticks);
                XAPIStuff.XAPIDurationContext duration = new XAPIStuff.XAPIDurationContext(ts);
                XAPIStuff.XAPIContext context = new XAPIStuff.XAPIContext(duration);

                XAPIStuff.XAPIStatement myStatement = new XAPIStuff.XAPIStatement(actor, verb, myObject, context);

                string xapiString = JsonConvert.SerializeObject(myStatement, Newtonsoft.Json.Formatting.Indented);


                try
                {
                    foreach (LAApplication myLA in myLAApps)
                    {

                        string url = myLA.Path + xapiString;
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        WebResponse response = await request.GetResponseAsync();
                        Stream resStream = response.GetResponseStream();
                        StreamReader sr99 = new StreamReader(resStream);
                        string aa = await sr99.ReadToEndAsync();

                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("error trying to post to server");
                }
            }
        }

        public async void HandleFeedback(string feedback)
        {
            foreach(FeedbackApp fapp in myFeedbacks)
            {
                fapp.SendUDP(feedback);  
            }
            try
            {
                FeedbackObject feed = JsonConvert.DeserializeObject<FeedbackObject>(feedback);

                if (feed.verb.Equals(lastFeedbackSent.verb))
                {

                }
                else if (feed.verb.Contains("Good"))
                {
                    XAPIStuff.XAPIActor actor = new XAPIStuff.XAPIActor("id_" + lastFeedbackSent.applicationName, "application", lastFeedbackSent.applicationName);
                    XAPIStuff.XAPIVerb verb = new XAPIStuff.XAPIVerb("id", lastFeedbackSent.verb);
                    XAPIStuff.XAPIObject myObject = new XAPIStuff.XAPIObject("student", "id_student");
                    XAPIStuff.XAPIDurationContext duration = new XAPIStuff.XAPIDurationContext(feed.frameStamp.Subtract(lastFeedbackSent.frameStamp));
                    XAPIStuff.XAPIContext context = new XAPIStuff.XAPIContext(duration);

                    XAPIStuff.XAPIStatement myStatement = new XAPIStuff.XAPIStatement(actor, verb, myObject, context);

                    string xapiString = JsonConvert.SerializeObject(myStatement, Newtonsoft.Json.Formatting.Indented);

                    try
                    {
                        foreach (LAApplication myLA in myLAApps)
                        {

                            string url = myLA.Path + xapiString;
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                            WebResponse response = await request.GetResponseAsync();
                            Stream resStream = response.GetResponseStream();
                            StreamReader sr99 = new StreamReader(resStream);
                            string aa = await sr99.ReadToEndAsync();

                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("error trying to post to server");
                    }
                    lastFeedbackSent = feed;
                }
                else
                {
                    lastFeedbackSent = feed;
                }
            }
            catch
            {

            }
            
            Dispatcher.Invoke(() =>
            {
                try
                {
                    myRecordingInterface.LabelFeedback.Content = feedback;
                }
                catch
                {

                }
                
            });
        }

        #region startingApps
        private void AddApplicationsToLists()
        {
            myApps = new List<ApplicationClass>();
            myEnabledApps = new List<ApplicationClass>();
            myFeedbacks = new List<FeedbackApp>();
            myLAApps = new List<LAApplication>();

            foreach (DataRow r in appsDataSet.Tables[0].Rows)
            {
                string applicationName = (string)r[0];
                string path = (string)r[1];
                bool oneExecutableBool = (bool)r[2];
                string parameter = (string)r[3];
                bool remoteBool = (bool)r[4];
                int tCPListener = (int)r[5];
                int tCPSender = (int)r[6];
                int tCPFile = (int)r[7];
                int uDPListener = (int)r[8];
                int uDPSender = (int)r[9];
                bool usedBool = (bool)r[10];
                bool isVideo = (bool)r[11];
                ApplicationClass app = new ApplicationClass(applicationName, path, oneExecutableBool, parameter, remoteBool, tCPListener, tCPSender, tCPFile, uDPListener,  uDPSender, usedBool, isVideo, this);
                myApps.Add(app);
                if (app.UsedBool)
                {
                    myEnabledApps.Add(app);
                }
            }
            foreach(DataRow r in FeedbackAppsDataSet.Tables[0].Rows)
            {
                string name = (string)r[0];
                string path = (string)r[1];
                int TCPSenderPort = (int)r[2];
                int UDPSenderPort = (int)r[3];
                FeedbackApp fa = new FeedbackApp(path, TCPSenderPort, UDPSenderPort);
                myFeedbacks.Add(fa);
            }
            foreach (DataRow r in LAAppsDataSet.Tables[0].Rows)
            {
                string name = (string)r[0];
                string path = (string)r[1];
                LAApplication LAApp = new LAApplication(name, path);
                myLAApps.Add(LAApp);
            }
        }

        private void InitializeStartupPar()
        {
            foreach (ApplicationClass app in myEnabledApps)
            {
                app.CheckStartupPar();
            }
        }

        private void InitializeApplications()
        {
            foreach(ApplicationClass app in myEnabledApps)
            {
                app.StartApp();
            }
        }
        #endregion

        #region saveDeleteCopyApps
        private void SaveApplications()
        {
           // appsDataSet.Tables[0].WriteXml(Directory.GetCurrentDirectory() + "\\DataConfig\\AppsConfig.xml");
            appsDataSet.Tables[0].WriteXml(workingDirectory + "\\DataConfig\\AppsConfig.xml");
        }

        private void DeleteApplications()
        {
            try
            {
                if (AppsGrid.SelectedIndex  != -1)
                {
                    appsDataSet.Tables[0].Rows[AppsGrid.SelectedIndex].Delete();
                }
            }
            catch(IndexOutOfRangeException e)
            {
                Console.WriteLine(e);
            }
        }

        private void CopyApplications()
        {
            try
            {
                if (AppsGrid.SelectedIndex != -1)
                {
                    
                    var row = appsDataSet.Tables[0].Rows[AppsGrid.SelectedIndex];
                    appsDataSet.Tables[0].ImportRow(row);
                    for (int i = 5; i < 10; i++)
                    {
                        int x = Convert.ToInt32(appsDataSet.Tables[0].Rows[AppsGrid.SelectedIndex][i]);
                        appsDataSet.Tables[0].Rows[appsDataSet.Tables[0].Rows.Count - 1][i] = x + 1;
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e);
            }
        }


        private void SaveLAApplications()
        {
           // LAAppsDataSet.Tables[0].WriteXml(Directory.GetCurrentDirectory() + "\\DataConfig\\LAConfig.xml");
            LAAppsDataSet.Tables[0].WriteXml(workingDirectory + "\\DataConfig\\LAConfig.xml");
        }

        private void DeleteAApplications()
        {
            LAAppsDataSet.Tables[0].Rows[LAAppsGrid.SelectedIndex].Delete();
        }

        private void SaveFeedbackApplications()
        {
            //FeedbackAppsDataSet.Tables[0].WriteXml(Directory.GetCurrentDirectory() + "\\DataConfig\\FeedbackConfig.xml");
            FeedbackAppsDataSet.Tables[0].WriteXml(workingDirectory + "\\DataConfig\\FeedbackConfig.xml");
        }

        private void DeleteFeedbackApplications()
        {
            FeedbackAppsDataSet.Tables[0].Rows[FeedbackAppsGrid.SelectedIndex].Delete();
        }

        #endregion

        #region interactionsClicks
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveApplications();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteApplications();        
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopyApplications();
        }

        private void StartApplications_Click(object sender, RoutedEventArgs e)
        {
           // System.Diagnostics.Process.Start(@"C:\Users\jan\source\repos\LearningHub\HubDesktop\bin\Debug\restart.bat");

            SaveApplications();
            SaveLAApplications();
            SaveFeedbackApplications();
            AddApplicationsToLists();
            InitializeStartupPar();
            myRecordingInterface = new Recording(this);
            MainCanvas.Children.Add(myRecordingInterface);
            myState = States.recordingReady;
            InitializeApplications();
        }

        private void LAButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveLAApplications();
        }     

        private void LAButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteAApplications();
        }      

        private void FeedbackButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFeedbackApplications();
        }
      
        private void FeedbackButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteFeedbackApplications();        
        }

        #endregion

        #region remoteControlstuff

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }


        private void RemoteControlStart()
        {
            try
            {
                controlPortNumber = Int32.Parse(controlPort.Text);
            }
            catch
            {

            }
            myRemoteControlTCPListener = new TcpListener(IPAddress.Any, controlPortNumber);
            myRemoteControlTCPListener.Start();
            while (IamRunning )
            {
                Console.WriteLine("The server is running at port 12001...");
                Console.WriteLine("The local End point is  :" +
                                  myRemoteControlTCPListener.LocalEndpoint);
                Console.WriteLine("Waiting for a connection.....");

                Socket s = myRemoteControlTCPListener.AcceptSocket();
                Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

                byte[] b = new byte[100];

                int k = s.Receive(b);
                Console.WriteLine("Recieved...");
                string receivedString = System.Text.Encoding.UTF8.GetString(b);

                switch(myState)
                {
                    case States.menu:
                        if(receivedString.Contains(startAPPs))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                StartApplications_Click(null, null);
                            });
                        }
                        break;
                    case States.recordingReady:
                        if(receivedString.Contains(startRecording))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                myRecordingInterface.StartRecording();
                            });
                        }
                        break;
                    case States.isRecording:
                        if(receivedString.Contains(stopRecording))
                        {
                            SendFileName(s);
                            Dispatcher.Invoke(() =>
                            {
                                myRecordingInterface.ButtonStopRecording_Click(null, null);
                            });
                        }
                        break;
                    case States.RecordingStop:
                        if(receivedString.Contains(finish))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                myRecordingInterface.ButtonFinish_Click(null, null);
                            });
                        }
                        break;
                }
            }
            myRemoteControlTCPListener.Stop();
        }

        private void SendFileName(Socket s)
        {
            string stringToSend = myRecordingInterface.recordingID +".zip";
            //TODO send this string to the remoteEndPoint
            byte[] byData = System.Text.Encoding.ASCII.GetBytes("filename="+stringToSend);
            s.Send(byData);
            s.Disconnect(false);
        }
        #endregion


    }
}

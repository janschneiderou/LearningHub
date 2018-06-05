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
            setAppsTable();
            Loaded += MainWindow_Loaded;

           

            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            myState = States.menu;
            controlPortNumber = Int32.Parse(controlPort.Text);
            RemoteControlThread = new Thread(new ThreadStart(remoteControlStart));
            RemoteControlThread.Start();
        }

        public void startAgain()
        {
            MainCanvas.Children.Remove(myRecordingInterface);
            myRecordingInterface = null;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach(ApplicationClass app in myEnabledApps)
            {
                app.closeApp();
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
            Environment.Exit(Environment.ExitCode);
        }

        private void setAppsTable()
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

        public async Task handleXAPIAsync()
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
                catch (Exception E)
                {
                    Console.WriteLine("error trying to post to server");
                }
            }
        }

        public async void handleFeedback(string feedback)
        {
            foreach(FeedbackApp fapp in myFeedbacks)
            {
                fapp.sendUDP(feedback);
                
               
            }

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
                catch (Exception E)
                {
                    Console.WriteLine("error trying to post to server");
                }
                lastFeedbackSent = feed;
            }
            else
            {
                lastFeedbackSent = feed;
            }
            Dispatcher.Invoke(() =>
            {
                myRecordingInterface.LabelFeedback.Content = feedback;
            });
        }

        #region startingApps
        private void addApplicationsToLists()
        {
            myApps = new List<ApplicationClass>();
            myEnabledApps = new List<ApplicationClass>();
            myFeedbacks = new List<FeedbackApp>();
            myLAApps = new List<LAApplication>();

            foreach (DataRow r in appsDataSet.Tables[0].Rows)
            {
                string applicationName = (string)r[0];
                string path = (string)r[1];
                bool remoteBool = (bool)r[2];
                int tCPListener = (int)r[3];
                int tCPSender = (int)r[4];
                int tCPFile = (int)r[5];
                int uDPListener = (int)r[6];
                int uDPSender = (int)r[7];
                bool usedBool = (bool)r[8];
                ApplicationClass app = new ApplicationClass(applicationName, path, remoteBool, tCPListener, tCPSender, uDPListener, tCPFile, uDPSender, usedBool, this);
                myApps.Add(app);
                if (app.usedBool == true)
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

        private void initializeApplications()
        {
            foreach(ApplicationClass app in myEnabledApps)
            {
                app.startApp();
            }
        }
        #endregion

        #region saveDeleteApps
        private void saveApplications()
        {
            appsDataSet.Tables[0].WriteXml(Directory.GetCurrentDirectory() + "\\DataConfig\\AppsConfig.xml");
        }

        private void deleteApplications()
        {
            appsDataSet.Tables[0].Rows[AppsGrid.SelectedIndex].Delete();
        }
        private void saveLAApplications()
        {
            LAAppsDataSet.Tables[0].WriteXml(Directory.GetCurrentDirectory() + "\\DataConfig\\LAConfig.xml");
        }
        private void deleteAApplications()
        {
            LAAppsDataSet.Tables[0].Rows[LAAppsGrid.SelectedIndex].Delete();
        }
        private void SaveFeedbackApplications()
        {
            FeedbackAppsDataSet.Tables[0].WriteXml(Directory.GetCurrentDirectory() + "\\DataConfig\\FeedbackConfig.xml");
        }
        private void DeleteFeedbackApplications()
        {
            FeedbackAppsDataSet.Tables[0].Rows[FeedbackAppsGrid.SelectedIndex].Delete();
        }

        #endregion

        #region interactionsClicks
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            saveApplications();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            deleteApplications();
                
        }
        
        private void StartApplications_Click(object sender, RoutedEventArgs e)
        {
            saveApplications();
            saveLAApplications();
            SaveFeedbackApplications();
            addApplicationsToLists();
            myRecordingInterface = new Recording(this);
            MainCanvas.Children.Add(myRecordingInterface);
            myState = States.recordingReady;
            initializeApplications();

        }

        private void LAButtonSave_Click(object sender, RoutedEventArgs e)
        {
            saveLAApplications();
        }

        

        private void LAButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            deleteAApplications();
            
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


        private void remoteControlStart()
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
            while (IamRunning == true)
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
                                myRecordingInterface.startRecording();
                            });
                        }
                        break;
                    case States.isRecording:
                        if(receivedString.Contains(stopRecording))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                myRecordingInterface.buttonStopRecording_Click(null, null);
                            });
                        }
                        break;
                    case States.RecordingStop:
                        if(receivedString.Contains(finish))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                myRecordingInterface.buttonFinish_Click(null, null);
                            });
                        }
                        break;
                }
            }
            myRemoteControlTCPListener.Stop();
        }
        #endregion

        
    }
}

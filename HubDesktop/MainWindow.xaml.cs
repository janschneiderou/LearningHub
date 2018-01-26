﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();
            workingDirectory = Directory.GetCurrentDirectory();
            myApps = new List<ApplicationClass>();
            myEnabledApps = new List<ApplicationClass>();

            myFeedbacks = new List<FeedbackApp>();
            myLAApps = new List<LAApplication>();
            setAppsTable();
            
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

        public void handleFeedback(string feedback)
        {
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
                string path = (string)r[0];
                int TCPSenderPort = (int)r[1];
                int UDPSenderPort = (int)r[2];
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


    }
}

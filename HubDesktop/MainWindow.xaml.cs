using System;
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
        Recording myRecordingInterface;
        DataSet appsDataSet;
        public static string workingDirectory;

        public MainWindow()
        {
            InitializeComponent();
            workingDirectory = Directory.GetCurrentDirectory();
            myApps = new List<ApplicationClass>();
            myEnabledApps = new List<ApplicationClass>();
            setAppsTable();
            
        }

        private void setAppsTable()
        {
 
            appsDataSet = new DataSet();
            appsDataSet.ReadXmlSchema(Directory.GetCurrentDirectory() + "\\DataConfig\\AppsSchema.xsd");
            appsDataSet.ReadXml(Directory.GetCurrentDirectory() + "\\DataConfig\\AppsConfig.xml");
            
            AppsGrid.ItemsSource = appsDataSet.Tables[0].DefaultView;

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
            addApplicationsToLists();
            myRecordingInterface = new Recording(this);
            MainCanvas.Children.Add(myRecordingInterface);
            initializeApplications();

        }




        #endregion
    }
}

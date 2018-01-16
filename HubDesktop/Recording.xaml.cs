using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace HubDesktop
{
    /// <summary>
    /// Interaction logic for Recording.xaml
    /// </summary>
    public partial class Recording : UserControl
    {
        List<Label> labelApps;
        List<Label> labelReady;
        MainWindow parent;

        bool recordingStarted = false;
        bool everythingReady = false;
        Thread threadWaitingForReady;
        Thread waitingForUpload;

        #region initialization
        public Recording(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            InitializeInterface();
            InitializeControl();

            
        }

        private void InitializeControl()
        {
            threadWaitingForReady = new Thread(new ThreadStart(tcpListenersStart));
            threadWaitingForReady.Start();
        }

        private void tcpListenersStart()
        {
            while(recordingStarted==false && everythingReady == false)
            {
                setLabelReadyContent();
                Thread.Sleep(1000);
            }
        }

        private void InitializeInterface()
        {
            this.Height = parent.Height;
            this.Width = parent.Width;
            
            labelApps = new List<Label>();
            labelReady = new List<Label>();


            ColumnDefinition gridColApps = new ColumnDefinition();
            ColumnDefinition gridColReady = new ColumnDefinition();
            appsGrid.ColumnDefinitions.Add(gridColApps);
            appsGrid.ColumnDefinitions.Add(gridColReady);

            RowDefinition gridRowHeader = new RowDefinition();
            gridRowHeader.Height = new GridLength(45);
            appsGrid.RowDefinitions.Add(gridRowHeader);
            Label headerApp = new Label();
            headerApp.FontStyle = FontStyles.Oblique;
            headerApp.Content = "Application Name";
            Grid.SetRow(headerApp, 0);
            Grid.SetColumn(headerApp, 0);
            appsGrid.Children.Add(headerApp);

            Label headerReady = new Label();
            headerReady.FontStyle = FontStyles.Oblique;
            headerReady.Content = "Is Ready?";
            Grid.SetRow(headerReady, 0);
            Grid.SetColumn(headerReady, 1);
            appsGrid.Children.Add(headerReady);

            int i = 1;
            foreach (ApplicationClass app in parent.myEnabledApps)
            {
                RowDefinition gridRow = new RowDefinition();
                gridRow.Height = new GridLength(45);
                appsGrid.RowDefinitions.Add(gridRow);

                Label lApps = new Label();
                lApps.Content = app.Name;

                Grid.SetRow(lApps, i);
                Grid.SetColumn(lApps, 0);
                appsGrid.Children.Add(lApps);

                Label lReady = new Label();
                lReady.Content = "";

                Grid.SetRow(lReady, i);
                Grid.SetColumn(lReady, 1);
                appsGrid.Children.Add(lReady);
                labelReady.Add(lReady);
                i++;
            }
        }

        #endregion


        private void setLabelReadyContent()
        {
            int i = 0;
            int readyApps = 0;
            foreach(ApplicationClass apps in parent.myEnabledApps )
            {
                if(apps.isREady==true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        labelReady[i].Content = "Yes";
                    });
           
                    readyApps++;
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        labelReady[i].Content = "No";
                    });
                }
                i++;
            }
            if(readyApps==parent.myEnabledApps.Count)
            {
                everythingReady = true;
            }
        }

        private void buttonStartRecording_Click(object sender, RoutedEventArgs e)
        {
            startRecording();
            
        }

        public void startRecording()
        {
            string recordingID = DateTime.Now.Hour.ToString();
            recordingID = recordingID + "H" + DateTime.Now.Minute.ToString() + "M" + DateTime.Now.Second.ToString() + "S";
            
            foreach (ApplicationClass apps in parent.myEnabledApps)
            {
                apps.sendStartRecording(recordingID);
                apps.IamRunning = true;
            }
            buttonStartRecording.IsEnabled = false;
            buttonStopRecording.IsEnabled = true;
        }

        private void buttonStopRecording_Click(object sender, RoutedEventArgs e)
        {
            foreach (ApplicationClass apps in parent.myEnabledApps)
            {
                apps.sendStopRecording();
                apps.IamRunning = false;
            }
            buttonStartRecording.IsEnabled = false;
            buttonStopRecording.IsEnabled = false;

            waitingForUpload = new Thread(new ThreadStart(uploadListener));
            waitingForUpload.Start();
        }

        private void uploadListener()
        {
            bool waitingForUpload = true;
            while(waitingForUpload==true)
            {
                int i = 0;
                foreach (ApplicationClass app in parent.myEnabledApps)
                {
                    if(app.uploadReady ==true)
                    {
                        i++;
                    }
                }
                if(i==parent.myEnabledApps.Count)
                {
                    waitingForUpload = false;
                }
                Thread.Sleep(1000);
            }
            Dispatcher.Invoke(() =>
            {
                buttonFinish.Visibility = Visibility.Visible;
                
            });
        }

        private void buttonFinish_Click(object sender, RoutedEventArgs e)
        {
            foreach (ApplicationClass app in parent.myEnabledApps)
            {
                app.closeApp();
            }

            }
    }
}

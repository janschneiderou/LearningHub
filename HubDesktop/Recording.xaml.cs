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
        public string recordingID;
        public bool isOpen = true;

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
            while(recordingStarted==false && everythingReady == false && isOpen==true)
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
            if(parent.myEnabledApps != null)
            {
                if (readyApps == parent.myEnabledApps.Count)
                {
                    everythingReady = true;
                }
            }
            
        }

        private void buttonStartRecording_Click(object sender, RoutedEventArgs e)
        {
            startRecording();
            
        }

        public void startRecording()
        {
            recordingID = DateTime.Now.Year.ToString() + "-" +DateTime.Now.Month.ToString()+"-"+DateTime.Now.Day+"-";
            recordingID = recordingID + DateTime.Now.Hour.ToString();
            recordingID = recordingID + "H" + DateTime.Now.Minute.ToString() + "M" + DateTime.Now.Second.ToString() + "S" + DateTime.Now.Millisecond.ToString();
            
            foreach (ApplicationClass apps in parent.myEnabledApps)
            {
                apps.sendStartRecording(recordingID);
                apps.IamRunning = true;
            }
            buttonStartRecording.IsEnabled = false;
            buttonStopRecording.IsEnabled = true;
            MainWindow.myState = MainWindow.States.isRecording;
        }

        public void buttonStopRecording_Click(object sender, RoutedEventArgs e)
        {
            foreach (ApplicationClass apps in parent.myEnabledApps)
            {
                apps.sendStopRecording();
                apps.IamRunning = false;
            }


            parent.handleXAPIAsync();
            buttonStartRecording.IsEnabled = false;
            buttonStopRecording.IsEnabled = false;
            MainWindow.myState = MainWindow.States.RecordingStop;
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
                CompressAndUpload ca = new CompressAndUpload(MainWindow.workingDirectory + "\\" + recordingID, recordingID);
            });
        }

        public void buttonFinish_Click(object sender, RoutedEventArgs e)
        {
            foreach (ApplicationClass app in parent.myEnabledApps)
            {
                app.closeApp();
            }
            parent.startAgain();
            MainWindow.myState = MainWindow.States.menu;

            

        }
    }
}

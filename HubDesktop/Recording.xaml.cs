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
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace HubDesktop
{
    /// <summary>
    /// Interaction logic for Recording.xaml
    /// </summary>
    public partial class Recording : UserControl
    {
        private List<Label> labelApps;
        private List<Label> labelReady;
        private MainWindow parent;
        private bool recordingStarted = false;
        private bool everythingReady = false;
        private Thread threadWaitingForReady;
        private Thread waitingForUpload;
        public string recordingID;
        public bool isOpen = true;

        #region initialization
        public Recording(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            InitializeInterface();
            InitializeControl();

            uploadFiles.IsChecked = MainWindow.uploadToServer; 

        }

        private void InitializeControl()
        {
            threadWaitingForReady = new Thread(new ThreadStart(TcpListenersStart));
            threadWaitingForReady.Start();
        }

        private void TcpListenersStart()
        {
            while (recordingStarted == false && everythingReady == false && isOpen )
            {
                SetLabelReadyContent();
                Thread.Sleep(1000);
            }
        }

        private void InitializeInterface()
        {
            Height = parent.Height;
            Width = parent.Width;

            labelApps = new List<Label>();
            labelReady = new List<Label>();


            ColumnDefinition gridColApps = new ColumnDefinition();
            ColumnDefinition gridColReady = new ColumnDefinition();
            appsGrid.ColumnDefinitions.Add(gridColApps);
            appsGrid.ColumnDefinitions.Add(gridColReady);

            RowDefinition gridRowHeader = new RowDefinition
            {
                Height = new GridLength(45)
            };
            appsGrid.RowDefinitions.Add(gridRowHeader);
            Label headerApp = new Label
            {
                FontStyle = FontStyles.Oblique,
                Content = "Application Name "
            };
            Grid.SetRow(headerApp, 0);
            Grid.SetColumn(headerApp, 0);
            appsGrid.Children.Add(headerApp);

            Label headerReady = new Label
            {
                FontStyle = FontStyles.Oblique,
                Content = "Is Ready?"
            };
            Grid.SetRow(headerReady, 0);
            Grid.SetColumn(headerReady, 1);
            appsGrid.Children.Add(headerReady);

            int i = 1;
            foreach (ApplicationClass app in parent.myEnabledApps)
            {
                RowDefinition gridRow = new RowDefinition
                {
                    Height = new GridLength(45)
                };
                appsGrid.RowDefinitions.Add(gridRow);

                if (app.OneExeName == null)
                {
                    Label lApps = new Label
                    {
                        Content = app.Name
                    };
                    Grid.SetRow(lApps, i);
                    Grid.SetColumn(lApps, 0);
                    appsGrid.Children.Add(lApps);
                }
                else
                {
                    Label lApps = new Label
                    {
                        Content = app.Name + " " + app.OneExeName
                    };
                    Grid.SetRow(lApps, i);
                    Grid.SetColumn(lApps, 0);
                    appsGrid.Children.Add(lApps);
                }

                Label lReady = new Label
                {
                    Content = ""
                };

                Grid.SetRow(lReady, i);
                Grid.SetColumn(lReady, 1);
                appsGrid.Children.Add(lReady);
                labelReady.Add(lReady);
                i++;
            }
        }

        #endregion


        private void SetLabelReadyContent()
        {
            int i = 0;
            int readyApps = 0;
            foreach (ApplicationClass apps in parent.myEnabledApps)
            {
                if (apps.isReady )
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
            if (parent.myEnabledApps != null)
            {
                if (readyApps == parent.myEnabledApps.Count)
                {
                    everythingReady = true;
                }
            }

        }

        private void ButtonStartRecording_Click(object sender, RoutedEventArgs e)
        {
            StartRecording();
        }

        public void StartRecording()
        {
            statusLabel.Content = "recording";
            recordingID = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day + "-";
            recordingID = recordingID + DateTime.Now.Hour.ToString();
            recordingID = recordingID + "H" + DateTime.Now.Minute.ToString() + "M" + DateTime.Now.Second.ToString() + "S" + DateTime.Now.Millisecond.ToString();

            foreach (ApplicationClass apps in parent.myEnabledApps)
            {
                apps.SendStartRecording(recordingID);
                apps.iAmRunning = true;
            }
            buttonStartRecording.IsEnabled = false;
            buttonStopRecording.IsEnabled = true;
            MainWindow.myState = MainWindow.States.isRecording;
        }

        public void ButtonStopRecording_Click(object sender, RoutedEventArgs e)
        {
            foreach (ApplicationClass apps in parent.myEnabledApps)
            {
                apps.SendStopRecording();
                apps.iAmRunning = false;
            }


            parent.HandleXAPIAsync();
            buttonStartRecording.IsEnabled = false;
            buttonStopRecording.IsEnabled = false;
            MainWindow.myState = MainWindow.States.RecordingStop;
            waitingForUpload = new Thread(new ThreadStart(UploadListener));
            waitingForUpload.Start();
            statusLabel.Content = "Retrieving recordings";
        }

        private void UploadListener()
        {
            bool waitingForUpload = true;
            while (waitingForUpload)
            {
                int i = 0;
                foreach (ApplicationClass app in parent.myEnabledApps)
                {
                    if (app.uploadReady )
                    {
                        i++;
                    }
                }
                if (i == parent.myEnabledApps.Count)
                {
                    waitingForUpload = false;
                }
                Thread.Sleep(1000);
            }
            Dispatcher.Invoke(() =>
            {
                //buttonFinish.Visibility = Visibility.Visible;
                if(MainWindow.uploadToServer==true)
                {
                    statusLabel.Content = "Uploading files to server";
                    CompressAndUpload ca = new CompressAndUpload(MainWindow.workingDirectory + "\\" + recordingID, recordingID, parent.myEnabledApps);
                    ca.FinishedUploadingEvent += Ca_finishedUploadingEvent;
                }
                else
                {
                    buttonFinish.Visibility = Visibility.Visible;
                    statusLabel.Content = "Recorded Finished";
                }
                
            });
        }

        private void Ca_finishedUploadingEvent(object sender)
        {
            buttonFinish.Visibility = Visibility.Visible;
            statusLabel.Content = "Upload finished";
        }

        public void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {

            foreach (ApplicationClass app in parent.myEnabledApps)
            {
                app.CloseApp();
            }
            parent.StartAgain();
            MainWindow.myState = MainWindow.States.menu;
        }

        private void uploadFiles_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.uploadToServer = true;
            
            
        }

        private void uploadFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.uploadToServer = false;
        }
    }
}

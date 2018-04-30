using ConnectorHub;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Threading;

namespace VisualTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string jsonOne;
        List<RecordingObject> recordingObject;
        double totalTime;
        int totalFrames;
        List<Canvas> valueCanvas;
        List<string> attributeNames;
        List<CheckBox> checkBoxAttributes;
        List<string> selectedValues;
        List<Dictionary<string, string>> selectedDisplayedValues;
        List<SolidColorBrush> colorForValues;
        List<SolidColorBrush> colorForCanvas;
        AttributeList[] AttributeList;
        double topValueSpace = 100;
        Line myTimelineLine;
        double frameLength;
       

        

        public MainWindow()
        {
            InitializeComponent();
            recordingObject = new List<RecordingObject>();
            valuesScroll.Width = this.Width;
            attributeNames = new List<string>();

           
        }

       
        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            buttonShow.IsEnabled = true;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                jsonOne = File.ReadAllText(openFileDialog.FileName);
            RecordingObject r= JsonConvert.DeserializeObject<RecordingObject>(jsonOne);
            recordingObject.Add(r); 

            getAttributeNames();

            totalTime = recordingObject[recordingObject.Count-1].frames[recordingObject[recordingObject.Count - 1].frames.Count - 1].frameStamp.TotalMilliseconds;
            textBoxFile.Text = totalTime+"";

           

        }

        private void getAttributeNames()
        {
            checkBoxAttributes = new List<CheckBox>();
            GridAttributesKeys.Children.Clear();
            GridAttributesKeys.RowDefinitions.Clear();
            foreach (RecordingObject rec in recordingObject)
            {
                attributeNames = new List<string>(rec.frames[0].frameAttributes.Keys);
                
                foreach (string s in attributeNames)
                {
                    CheckBox c = new CheckBox();
                    c.Content = s;
                    checkBoxAttributes.Add(c);
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(30);
                    GridAttributesKeys.RowDefinitions.Add(gridRow);
                    Grid.SetRow(c, GridAttributesKeys.RowDefinitions.Count - 1);
                    GridAttributesKeys.Children.Add(c);
                }
            }
            
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ButtonShow_Click(object sender, RoutedEventArgs e)
        {
            selectedValues = new List<string>();
            colorForValues = new List<SolidColorBrush>();
            colorForCanvas = new List<SolidColorBrush>();
            valueCanvas = new List<Canvas>();
            GridValues.Children.Clear();
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            foreach (CheckBox c in checkBoxAttributes)
            {
                if(c.IsChecked==true)
                {
                    selectedValues.Add(c.Content.ToString());
                    
                    Byte[] b = new Byte[3];
                    rnd.NextBytes(b);
                    SolidColorBrush sc = new SolidColorBrush(Color.FromRgb(b[0],b[1],b[2]));
                    colorForValues.Add(sc);                
                }
            }
            createAttributeLists();
            createGridRows();
            displayValues();
            ButtonLoadVideo.IsEnabled = true;
            
        }

        private void displayValues()
        {
           frameLength = 1000 / double.Parse(textBoxFrameRate.Text);
            double currentTimeTop = frameLength;
            double currentTimeDown = 0;
            totalFrames = (int)(totalTime / frameLength);

            for (int i = 0; i < totalFrames; i++)
            {
                for(int j= 0; j<selectedValues.Count; j++)
                {
                    double displacement = topValueSpace / (AttributeList[j].getMax() - AttributeList[j].getMin()); 
                    for (int k= 0; k< AttributeList[j].myTime.Count;k++)
                    {
                        if(AttributeList[j].myTime[k] > currentTimeDown &&
                        AttributeList[j].myTime[k] <= currentTimeTop)
                        {
                            Ellipse point1 = new Ellipse();
                            point1.Fill = colorForValues[j];
                            point1.Height = 5;
                            point1.Width = 5;
                            valueCanvas[j].Children.Add(point1);

                            double left = i * valuesScroll.Width / totalFrames;
                            Canvas.SetLeft(point1, left);
                            double top = topValueSpace;
                            try
                            {
                                top = -(double)(AttributeList[j].myObjectList[k]) * displacement + topValueSpace;
                                Canvas.SetTop(point1, top);
                            }
                            catch
                            {

                            }
                            
                        }
                        
                    }
                }
                currentTimeDown = currentTimeDown + frameLength;
                currentTimeTop = currentTimeTop + frameLength;
            }
        }

        private void createAttributeLists()
        {
            AttributeList = new VisualTest.AttributeList[selectedValues.Count];
            int i = 0;
            foreach (string s in selectedValues)
            {
                AttributeList[i] = new VisualTest.AttributeList(s);
                foreach (RecordingObject rec in recordingObject)
                {
                    if(rec.frames[0].frameAttributes.ContainsKey(s))
                    {
                        foreach (FrameObject f in rec.frames)
                        {
                            AttributeList[i].myTime.Add(f.frameStamp.TotalMilliseconds);
                            try
                            {
                                AttributeList[i].myObjectList.Add(Double.Parse(f.frameAttributes[s]));

                            }
                            catch
                            {
                                AttributeList[i].myObjectList.Add(f.frameAttributes[s]);
                            }
                        }
                    }
       
                }
                AttributeList[i].setMax();
                AttributeList[i].setMin();
                i++;

            }
        }

        private void createGridRows()
        {
            GridValues.Children.Add(CanvasForValues);
            GridValues.RowDefinitions.Clear();
            
            valueCanvas = new List<Canvas>();
            for(int i = 0; i<selectedValues.Count; i++)
            {
                Canvas c = new Canvas();
                c.Width = GridValues.Width;
                c.Height = 100;
                Label l = new Label();
                l.Content = selectedValues[i];
                l.Foreground = colorForValues[i];
                c.Children.Add(l);
                Canvas.SetTop(l,topValueSpace * .5);
                
                valueCanvas.Add(c);
                RowDefinition gridRow = new RowDefinition();
                gridRow.Height = new GridLength(topValueSpace);
                
                GridValues.RowDefinitions.Add(gridRow);
                Grid.SetRow(c, i);
                Grid.SetColumn(c, 0);
                GridValues.Children.Add(c);
    
            }

            myTimelineLine = new Line();
            CanvasForValues.Width = valuesScroll.ActualWidth;
            CanvasForValues.Height = valuesScroll.ActualHeight;
            CanvasForValues.Children.Add(myTimelineLine);
            myTimelineLine.Fill = Brushes.Black;
            myTimelineLine.Stroke = Brushes.Black;
            myTimelineLine.X1 = 0;
            myTimelineLine.X2 = 0;
            myTimelineLine.Y1 = 0;
            myTimelineLine.Y2 = topValueSpace* selectedValues.Count;
        }

        private void ButtonLoadVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                VideoControl.Source = new System.Uri(openFileDialog.FileName);
            }
            VideoControl.Play();

            // Create a timer that will update the counters and the time slider
            DispatcherTimer timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromMilliseconds(frameLength);
            timerVideoTime.Tick += new EventHandler(timer_Tick);
            timerVideoTime.Start();
            ButtonStop.IsEnabled = true;
            ButtonPlay.IsEnabled = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            double left = (VideoControl.Position.TotalMilliseconds/frameLength) * (valuesScroll.Width / totalFrames);
            
            Canvas.SetLeft(myTimelineLine, left);
            
           // myTimelineLine.X2 = left;
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            
            //if(VideoControl.)
            //{
            //    VideoControl.Play();
            //}
            //else
            //{
            //    VideoControl.Pause();
            //}

            VideoControl.Play();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            
            VideoControl.Stop();
        }

        private void CanvasForValues_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Canvas.SetLeft(myTimelineLine, e.GetPosition(CanvasForValues).X);
            try
            {
                //VideoControl.Pause();
                double left = Canvas.GetLeft(myTimelineLine);
                VideoControl.Position = TimeSpan.FromMilliseconds(left * frameLength * totalFrames / valuesScroll.Width);
                VideoControl.Pause();
                int x = 1;
                    
            }
            catch(Exception ex)
            {
                int z = 1;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LearningHub.Classes
{
    public class Controller
    {
        List<ApplicationClass> myApps;
        List<ApplicationClass> myEnabledApps;

        string appsFile = "";

        public Controller(string appsFile)
        {
            myApps = new List<ApplicationClass>();
            myEnabledApps = new List<ApplicationClass>();
            this.appsFile = appsFile;
            readAppsFile();

            startApps();//just for testing
        }

       

        #region readConfigurationFile
        private void readAppsFile()
        {

            
            string text = System.IO.File.ReadAllText(appsFile);
            //getHololensInfo(text);
            int currentIndex = 0;
            try
            {
                while (text.IndexOf("Application") != -1)
                {
                    currentIndex = text.IndexOf("<Name>");
                    int startText = currentIndex + 6;
                    string applicationName = text.Substring(startText, text.IndexOf("</Name>") - startText);
                    text = text.Substring(text.IndexOf("</Name>"));

                    currentIndex = text.IndexOf("<Path>");
                    startText = currentIndex + 6;
                    string filePath = text.Substring(startText, text.IndexOf("</Path>") - startText);
                    text = text.Substring(text.IndexOf("</Path>"));

                    currentIndex = text.IndexOf("<Remote>");
                    startText = currentIndex + 8;
                    string remoteBool = text.Substring(startText, text.IndexOf("</Remote>") - startText);
                    text = text.Substring(text.IndexOf("</Remote>")+8);

                    currentIndex = text.IndexOf("<TCPListener>");
                    startText = currentIndex + 13;
                    string tCPListener = text.Substring(startText, text.IndexOf("</TCPListener>") - startText);
                    text = text.Substring(text.IndexOf("</TCPListener>")+13);

                    currentIndex = text.IndexOf("<TCPSender>");
                    startText = currentIndex + 11;
                    string tCPSender = text.Substring(startText, text.IndexOf("</TCPSender>") - startText);
                    text = text.Substring(text.IndexOf("</TCPSender>")+11);

                    currentIndex = text.IndexOf("<UDPListener>");
                    startText = currentIndex + 13;
                    string uDPListener = text.Substring(startText, text.IndexOf("</UDPListener>") - startText);
                    text = text.Substring(text.IndexOf("</UDPListener>")+13);

                    currentIndex = text.IndexOf("<UDPSender>");
                    startText = currentIndex + 11;
                    string uDPSender = text.Substring(startText, text.IndexOf("</UDPSender>") - startText);
                    text = text.Substring(text.IndexOf("</UDPSender>")+11);

                    currentIndex = text.IndexOf("<Used>");
                    startText = currentIndex + 6;
                    string usedBool = text.Substring(startText, text.IndexOf("</Used>") - startText);
                    text = text.Substring(text.IndexOf("</Used>")+6);

                   
                    text = text.Substring(text.IndexOf("</Application>") + 3);

                    ApplicationClass app = new ApplicationClass(applicationName, filePath, remoteBool, tCPListener, tCPSender,  uDPListener, uDPSender, usedBool, this);
                    myApps.Add(app);
                    currentIndex++;
                }
            }
            catch
            {
                Console.WriteLine("I got an exception when reading configuration for Applications");
            }

        }

        #endregion

        #region startingApps
        public void startApps()
        {
            foreach (ApplicationClass ac in myApps)
            {
                ac.StartApp();
            }
        }
        #endregion
    }
}
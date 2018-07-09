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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;

namespace HubDesktop
{
    public class CompressAndUpload
    {
        private static readonly HttpClient client = new HttpClient();
        string zipFileName;
        string recordingID;
        List<ApplicationClass> myEnabledApps;

        public CompressAndUpload(string path, string recordingID, List<ApplicationClass> myEnabledApps)
        {
            this.myEnabledApps = myEnabledApps;
            this.recordingID = recordingID + ".zip";
            zipFileName = path + ".zip";
            ZipFile.CreateFromDirectory(path, zipFileName);
            //TODO call the upload method with the parameter zipFileName
            firstPost();
            
        }
        public async void firstPost()
        {
            try
            {
                var values = new Dictionary<string, string>
            {
                { "thing1", "hello" },
                { "thing2", "world" }
            };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("http://wekitproject.appspot.com/storage/requestupload", content);

                var responseString = await response.Content.ReadAsStringAsync();

                secondPost(responseString);
            }
            catch
            {

            }
            
        }
        
        private async void secondPost(string url)
        {
           
                System.Net.Http.MultipartFormDataContent form = new MultipartFormDataContent();

                var fileContent = new ByteArrayContent(FileToByteArray(zipFileName));
                var header = new ContentDispositionHeaderValue("form-data");
                header.Name = "\"myFile\"";
                header.FileName = "\"" + recordingID + "\"";
                //header.FileNameStar = fileName;
                fileContent.Headers.ContentDisposition = header;
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-zip-compressed");
                form.Add(fileContent, "myFile", recordingID);
                //fixed parameters
                string macAddress = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
                string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                form.Add(new StringContent("LearningHub " + macAddress), "\"device\"");
                form.Add(new StringContent(userName), "\"author\"");
            string myapps = "";
            foreach(ApplicationClass ap in myEnabledApps)
            {
                myapps = myapps+ap.Name + "_";
            }
                form.Add(new StringContent(myapps), "\"description\"");
                


                int c = form.Count();
                var response = await client.PostAsync(url, form);
                var responseString = await response.Content.ReadAsStringAsync();
            

            // System.Net.Http.MultipartFormDataContent form = new MultipartFormDataContent();

            // var fileContent = new ByteArrayContent(FileToByteArray(zipFileName));

            // var values = new Dictionary<string, string>
            // {
            //     { "device", "LearningHub" },
            //     { "author", "JanMac" },
            //     { "description", "PTMYOVideo" }
            // };

            // var content = new FormUrlEncodedContent(values);

            // var header = new ContentDispositionHeaderValue("form-data");
            // header.Name = "\"myFile\"";
            // header.FileName = "\"" + recordingID + "\"";
            // //header.FileNameStar = fileName;
            // fileContent.Headers.ContentDisposition = header;

            // fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-zip-compressed");
            // form.Add(fileContent, "myFile", recordingID);
            // form.Add(content);

            // int c = form.Count();
            // var response = await client.PostAsync(url, form);
            // var responseString = await response.Content.ReadAsStringAsync();
            //// int x = 1;
        }

        public byte[] FileToByteArray(string fileName)
        {
            byte[] buff = null;
            FileStream fs = new FileStream(fileName,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(fileName).Length;
            buff = br.ReadBytes((int)numBytes);
            return buff;
        }
        
    }
}

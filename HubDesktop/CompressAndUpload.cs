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

namespace HubDesktop
{
    public class CompressAndUpload
    {
        private static readonly HttpClient client = new HttpClient();
        string zipFileName;
        string recordingID;

        public CompressAndUpload(string path, string recordingID)
        {
            this.recordingID = recordingID + ".zip";
            zipFileName = path + ".zip";
            ZipFile.CreateFromDirectory(path, zipFileName);
            //TODO call the upload method with the parameter zipFileName
            firstPost();
            
        }
        public async void firstPost()
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
        
        private async void secondPost(string url)
        {
            System.Net.Http.MultipartFormDataContent form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(FileToByteArray(zipFileName));

            var values = new Dictionary<string, string>
            {
                { "device", "LearningHub" },
                { "author", "JanMac" },
                { "description", "PTMYOVideo" }
            };

            var content = new FormUrlEncodedContent(values);

            var header = new ContentDispositionHeaderValue("form-data");
            header.Name = "\"myFile\"";
            header.FileName = "\"" + recordingID + "\"";
            //header.FileNameStar = fileName;
            fileContent.Headers.ContentDisposition = header;

            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-zip-compressed");
            form.Add(fileContent, "myFile", recordingID);
            form.Add(content);

            int c = form.Count();
            var response = await client.PostAsync(url, form);
            var responseString = await response.Content.ReadAsStringAsync();
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

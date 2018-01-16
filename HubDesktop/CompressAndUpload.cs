using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace HubDesktop
{
    public class CompressAndUpload
    {
        public CompressAndUpload(string path)
        {
            string zipFileName = path + ".zip";
            ZipFile.CreateFromDirectory(path, zipFileName);
            //TODO call the upload method with the parameter zipFileName
        }
    }
}

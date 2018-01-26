using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubDesktop
{
    public class LAApplication
    {
        public string Path { get; set; }
        public string Name { get; set; }

        public LAApplication(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }
    }
}

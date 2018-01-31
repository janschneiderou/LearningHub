using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubDesktop.XAPIStuff
{
    public class XAPIVerb
    {
        public string id { get; set; }
        public object display { get; set; }
        public XAPIVerb(string id, object display)
        {
            this.id = id;
            this.display = display;
        }

    }
}

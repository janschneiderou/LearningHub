using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubDesktop.XAPIStuff
{
    public class XAPIContext
    {
        public object myObject { get; set; }
        public XAPIContext(object myObject)
        {
            this.myObject = myObject;
        }
    }
}

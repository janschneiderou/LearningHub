using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubDesktop.XAPIStuff
{
    public class XAPIObject
    {

        public string objectType { get; set; }
        public string id { get; set; }


    public XAPIObject( string objectType, string id)
        {
            this.objectType = objectType;
            this.id = id;
        }
    }
}

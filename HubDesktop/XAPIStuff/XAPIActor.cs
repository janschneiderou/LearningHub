using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubDesktop.XAPIStuff
{
    public class XAPIActor
    {
        public string user_id { get; set; }
        public string objectType { get; set; }
        public string userName { get; set; }

        public XAPIActor(string user_id, string objectType, string userName)
        {
            this.user_id = user_id;
            this.userName = userName;
            this.objectType = objectType;
        }
    }
}

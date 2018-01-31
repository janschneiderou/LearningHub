using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubDesktop.XAPIStuff
{
    public class XAPIStatement
    {
        public DateTime timeStamp { get; set; }
        public string id { get; set; }
        public XAPIActor actor { get; set; }
        public XAPIVerb verb { get; set; }
        public XAPIObject myObject { get; set; }
        public XAPIContext context { get; set; }

        public XAPIStatement(XAPIActor actor, XAPIVerb verb, XAPIObject myObject, XAPIContext context)
        {
            timeStamp = DateTime.Now;
            id = "id" + timeStamp.ToString();
            this.actor = actor;
            this.verb = verb;
            this.myObject = myObject;
            this.context = context;
        }
    }
}

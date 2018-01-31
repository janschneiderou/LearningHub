using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubDesktop.XAPIStuff
{
    public class XAPIDurationContext
    {
        public TimeSpan duration { get; set; }

        public XAPIDurationContext(TimeSpan duration)
        {
            this.duration = duration;
        }

    }
}

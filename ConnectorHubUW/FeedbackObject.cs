﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorHubUW
{
    public class FeedbackObject
    {
        public System.TimeSpan frameStamp { get; set; }
        public string applicationName { get; set; }
        public string verb { get; set; }

        public FeedbackObject(System.DateTime start, string feedbackValue, string applicationName)
        {

            this.frameStamp = System.DateTime.Now.Subtract(start);
            this.applicationName = applicationName;
            this.verb = feedbackValue;

        }
    }
}

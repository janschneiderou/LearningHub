using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorHubUW
{
    public class FrameObject
    {
        public System.TimeSpan frameStamp;

        public Dictionary<string, string> frameAttributes;

        public FrameObject(System.DateTime start, List<string> attributesNames, List<string> attributesValues)
        {
            frameAttributes = new Dictionary<string, string>();
            this.frameStamp = System.DateTime.Now.Subtract(start);
            for (int i = 0; i < attributesNames.Count; i++)
            {
                frameAttributes.Add(attributesNames[i], attributesValues[i]);
            }
        }
        public FrameObject()
        {

        }
    }
}

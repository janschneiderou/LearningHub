using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorHub
{
    public class RecordingObject
    {
        public string recordingID { get; set; }
        public string applicationName { get; set; }

        public List<FrameObject> frames { get; set; }
        
        public RecordingObject()
        {
            frames = new List<FrameObject>();
        }
    }
}

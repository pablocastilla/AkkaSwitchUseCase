using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class GeneralMessageData
    {
        public Guid CommandRequestID { get; set; }

        public DateTime SendTimeStamp { get; set; }
        public DateTime ExecutionTimeStamp { get; set; }

        public int ProcessTimeOutInSeconds { get; set; }

        public string User { get; set; }
    }
}

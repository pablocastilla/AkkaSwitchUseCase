using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Constants;

namespace Shared.Commands
{    
    public class Command : GeneralMessageData
    {
        public Guid CommandRequestID { get; set; }
        public DateTime ExecutionTimeStamp { get; set; }
        public Shared.Constants.Commands CommandId { get; set; }
        public int ProcessTimeOutInSeconds { get; set; }
    }
}

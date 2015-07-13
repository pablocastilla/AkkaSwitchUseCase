using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Persistence;

namespace Shared.Actors
{
    public class Device : AtLeastOnceDeliveryActor 
    {

        string serialNumber;

        public Device(string serialNumber)
        {
            this.serialNumber = serialNumber;
        }

        public override string PersistenceId
        {
            get { return serialNumber; }
        }

        protected override bool ReceiveCommand(object message)
        {
            
            throw new NotImplementedException();
        }

        protected override bool ReceiveRecover(object message)
        {
            throw new NotImplementedException();
        }
    }
}

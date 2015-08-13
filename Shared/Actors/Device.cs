using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Persistence;
using Shared.Commands;


namespace Shared.Actors
{
    public class Device : AtLeastOnceDeliveryActor 
    {

        string serialNumber;
        DeviceData data;

        public Device(string serialNumber)
        {
            this.serialNumber = serialNumber;

            data = new DeviceData();
        }

        public override string PersistenceId
        {
            get { return this.GetType().ToString()+" - "+serialNumber; }
        }

        protected override bool ReceiveCommand(object message)
        {
            if (message is ActivateDevice)
            {
                Persist((ActivateDevice)message, m => HandleActivateDevice(m));
                               
            }


            return true;
        }

        protected override bool ReceiveRecover(object message)
        {
            Console.WriteLine("Restoring data");

            if (message is ActivateDevice)
            {
                HandleActivateDevice((ActivateDevice)message);

            }

            return true;
        }

        private void HandleActivateDevice(ActivateDevice message)
        {
            data.GeneralState = DeviceState.Active;
            
        }
    }

    public class DeviceData
    {
        public DeviceState GeneralState{get;set;}
    
    }

    public enum DeviceState { Active, TurnedOn, TurnedOff }
}

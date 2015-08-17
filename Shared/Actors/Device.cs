using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence;
using Shared.Commands;
using Shared.Events;


namespace Shared.Actors
{
    public class Device : AtLeastOnceDeliveryActor 
    {
        public enum DeviceState { Inactive, Active, TurnedOn, TurnedOff }


        string serialNumber;
        DeviceState generalState;

        bool switchOnInProgress;
        Guid? switchOnInProgressGuid;
     
        public Device(string serialNumber)
        {
            this.serialNumber = serialNumber;

            generalState = DeviceState.Inactive;

           
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

            if (message is SwitchOn)
            {
                Persist((SwitchOn)message, m => HandleSwitchOnDevice(m));

            }


            if (message is CommandTimedOut)
            {
                Persist((CommandTimedOut)message, m => {

                    switchOnInProgress = false;
                    switchOnInProgressGuid = null;
                });

               
            }

            return true;
        }

        #region Commands

        private void HandleActivateDevice(ActivateDevice message)
        {
            generalState = DeviceState.Active;

        }

        private object HandleSwitchOnDevice(SwitchOn m)
        {
            var child = Context.Child(m.CommandID.ToString());

            if (child.Equals(ActorRefs.Nobody)) //child doesn't exist
            {
                var switchOnActor = Context.ActorOf(Props.Create(() => new SwitchOnProcess(m.CommandID, Context.Self)), m.CommandID.ToString());

                switchOnActor.Tell(m);

            }
            return child;         

            
        }

        #endregion

        #region Recover

        protected override bool ReceiveRecover(object message)
        {
            Console.WriteLine("Restoring data");

            if (message is ActivateDevice)
            {
                HandleActivateDevice((ActivateDevice)message);

            }

            if (message is SwitchOn)
            {
                switchOnInProgress = true;
                switchOnInProgressGuid = ((SwitchOn)message).CommandID;
            }

            if (message is CommandTimedOut)
            {
                switchOnInProgress = false;
                switchOnInProgressGuid = null;
            }

            return true;
        }

        #endregion

    }

 

    
}

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
    public class Device : ReceivePersistentActor
    {
        public enum DeviceState { Inactive, Active, TurnedOn, TurnedOff }


        string serialNumber;
        DeviceState generalState;

        bool switchInProgress;
        Guid? switchOnInProgressGuid;
        IActorRef currentCommandActor;
     
        public Device(string serialNumber)
        {
            this.serialNumber = serialNumber;

            generalState = DeviceState.Inactive;
            switchInProgress = false;

            Command<ActivateDevice>(
                    m => Persist(m, m2=>HandleActivateDevice(m2))
                    );

            Recover<ActivateDevice>(message =>
                    {
                    HandleActivateDevice(message);
                    });

            Command<SwitchOn>(
                    m => Persist(m, m2 => HandleSwitchOnDevice(m2))
                    );

            Recover<SwitchOn>(message =>
                    {
                        var m = message as SwitchOn;

                        switchInProgress = true;
                        switchOnInProgressGuid = message.CommandRequestID;

                        var timeout = (m.ExecutionTimeStamp - DateTime.Now).TotalSeconds + m.ProcessTimeOutInSeconds;
                        if (timeout > 0)
                        {
                            SetReceiveTimeout(TimeSpan.FromSeconds(timeout));
                        }
                        else
                        {
                            switchInProgress = false;
                            switchOnInProgressGuid = null;
                            SetReceiveTimeout(null);
                        }
                    }
                    );

            Command<CommandTimedOut>(
                    m => Persist(m, m2=>{ 
                        switchInProgress = false;
                        switchOnInProgressGuid = null;
                        SetReceiveTimeout(null);            
                    }));

            Recover<CommandTimedOut>(
                    m =>  {
                        switchInProgress = false;
                        switchOnInProgressGuid = null;
                        SetReceiveTimeout(null);
                    });
                     

            Command<ReceiveTimeout>(
                    m => Persist(m, m2 => {
                        switchInProgress = false;
                        switchOnInProgressGuid = null;
                        SetReceiveTimeout(null);
                    }));

            Recover<ReceiveTimeout>(
                    m => {
                        switchInProgress = false;
                        switchOnInProgressGuid = null;
                        SetReceiveTimeout(null);
                    });


        }

        public override string PersistenceId
        {
            get { return this.GetType().ToString()+" - "+serialNumber; }
        }

     

        #region Commands

        private void HandleActivateDevice(ActivateDevice message)
        {
            generalState = DeviceState.Active;

        }

        private void HandleSwitchOnDevice(SwitchOn m)
        {
            var switchActor = Context.Child("DeviceSwitch/"+serialNumber);           

            if (switchActor.Equals(ActorRefs.Nobody)) //child doesn't exist
            {
                switchActor = Context.ActorOf(Props.Create(() => new DeviceSwitch(serialNumber)));

                if(!IsRecovering)
                    switchActor.Tell(m);

            }
                   
        }

        #endregion

       

    }

 

    
}

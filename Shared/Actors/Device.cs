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

        bool switchOnInProgress;
        Guid? switchOnInProgressGuid;
        IActorRef currentCommandActor;
     
        public Device(string serialNumber)
        {
            this.serialNumber = serialNumber;

            generalState = DeviceState.Inactive;
            switchOnInProgress = false;

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

                        switchOnInProgress = true;
                        switchOnInProgressGuid = message.CommandRequestID;

                        var timeout = (m.ExecutionTimeStamp - DateTime.Now).TotalSeconds + m.ProcessTimeOutInSeconds;
                        if (timeout > 0)
                        {
                            SetReceiveTimeout(TimeSpan.FromSeconds(timeout));
                        }
                        else
                        {
                            switchOnInProgress = false;
                            switchOnInProgressGuid = null;
                            SetReceiveTimeout(null);
                        }
                    }
                    );

            Command<CommandTimedOut>(
                    m => Persist(m, m2=>{ 
                        switchOnInProgress = false;
                        switchOnInProgressGuid = null;
                        SetReceiveTimeout(null);            
                    }));

            Recover<CommandTimedOut>(
                    m =>  {
                        switchOnInProgress = false;
                        switchOnInProgressGuid = null;
                        SetReceiveTimeout(null);
                    });
                     

            Command<ReceiveTimeout>(
                    m => Persist(m, m2 => {
                        switchOnInProgress = false;
                        switchOnInProgressGuid = null;
                        SetReceiveTimeout(null);
                    }));

            Recover<ReceiveTimeout>(
                    m => {
                        switchOnInProgress = false;
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
            if (switchOnInProgress)
                return;

            var child = Context.Child(m.CommandRequestID.ToString());

            if (child.Equals(ActorRefs.Nobody)) //child doesn't exist
            {
                currentCommandActor = Context.ActorOf(Props.Create(() => new SwitchOnProcess(m.CommandRequestID, Context.Self)), m.CommandRequestID.ToString());

                if(!IsRecovering)
                    currentCommandActor.Tell(m);

            }

            var timeout = (m.ExecutionTimeStamp - DateTime.Now).TotalSeconds + m.ProcessTimeOutInSeconds;
            if (timeout > 0)
            {
                SetReceiveTimeout(TimeSpan.FromSeconds(timeout));
            }


        }

        #endregion

       

    }

 

    
}

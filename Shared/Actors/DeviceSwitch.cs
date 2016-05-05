using Akka.Actor;
using Akka.Persistence;
using Shared.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Actors
{
    public class DeviceSwitch : ReceivePersistentActor
    {
        string serialNumber;
        bool switchInProgress;

        public DeviceSwitch(string serialNumber)
        {
            this.serialNumber = serialNumber;

            Command<SwitchOn>(
                   m => Persist(m, m2 => HandleSwitchOnDevice(m2))
                   );

            Recover<SwitchOn>(message =>
            {
               
            }
                    );
        }

        private void HandleSwitchOnDevice(SwitchOn m)
        {
            var configurationSetterActor = Context.Child(m.CommandRequestID.ToString());

            if (configurationSetterActor.Equals(ActorRefs.Nobody)) //child doesn't exist
            {
                configurationSetterActor = Context.ActorOf(Props.Create(() => new ConfigurationSetter(m.CommandRequestID,Self)));

                if (!IsRecovering)
                    configurationSetterActor.Tell(m);

            }

            var timeout = (m.ExecutionTimeStamp - DateTime.Now).TotalSeconds + m.ProcessTimeOutInSeconds;
            if (timeout > 0)
            {
                SetReceiveTimeout(TimeSpan.FromSeconds(timeout));
            }
        }

        public override string PersistenceId
        {
            get { return this.GetType().ToString() + " - " + serialNumber; }
            
        }


    }
}

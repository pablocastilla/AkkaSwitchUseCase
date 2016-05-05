using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Persistence.SqlServer;
using Shared.Actors;
using Shared.Commands;
using Topshelf;

namespace BackEndProcess
{
    public class BackEndActorSystem : ServiceControl
    {

        private ActorSystem actorSystem;

        private IActorRef d1;
        private IActorRef d2;

        public bool Start(HostControl hostControl)
        {
            actorSystem = ActorSystem.Create("BackEnd");
            
            SqlServerPersistence.Init(actorSystem);

            d1 = actorSystem.ActorOf(
                    Props.Create(() => new Device("PCV1")));

            d1.Tell(new ActivateDevice());

            d1.Tell(new SwitchOn() { CommandRequestID = Guid.NewGuid(),ExecutionTimeStamp=DateTime.Now,ProcessTimeOutInSeconds=30 });

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            actorSystem.Shutdown();
            return true;
        }
    }
}

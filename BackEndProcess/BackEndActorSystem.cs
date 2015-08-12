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

        public bool Start(HostControl hostControl)
        {
            actorSystem = ActorSystem.Create("BackEnd");
            
            SqlServerPersistence.Init(actorSystem);

            d1 = actorSystem.ActorOf(
                    Props.Create(() => new Device("asdf2")));

            d1.Tell(new ActivateDevice());
      
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            actorSystem.Shutdown();
            return true;
        }
    }
}

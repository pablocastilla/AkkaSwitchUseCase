using Akka.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Actors
{
    public class SwitchProcess : ReceivePersistentActor
    {
        public override string PersistenceId
        {
            get { throw new NotImplementedException(); }
        }
    }
}

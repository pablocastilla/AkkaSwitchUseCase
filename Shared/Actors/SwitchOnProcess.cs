﻿using Akka.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Shared.Commands;
using Shared.Events;

namespace Shared.Actors
{
    public class SwitchOnProcess : ReceivePersistentActor
    {
        public enum SwitchOnState { Initiated, CommandLaunched, CommandInDevice, CommandExecutedInDevice, CommandTimedOut, CommandOk, CommandKo }
        
        Guid commandID;
        IActorRef device;
        SwitchOnState state;
        int retries;
        SwitchOn mainCommand;

        public SwitchOnProcess(Guid commandID, IActorRef device)
        {
            this.commandID = commandID;
            this.device = device;
            this.state = SwitchOnState.Initiated;

            retries = 0;

            ReadyCommands();
            ReadyRecovers();
        }

       


        public override string PersistenceId
        {
            get { return this.GetType().ToString() + " - " + commandID; }
        }


        private void ReadyCommands()
        {
            Command<SwitchOn>(message => Persist(message, LaunchCommand));


            Command<ReceiveTimeout>(timeout => Persist(timeout, ProcessTimeOut));
        }

        private void ReadyRecovers()
        {
            Recover<SwitchOn>(message => RecoverLaunchCommand(message));

            Recover<ReceiveTimeout>(message => RecoverTimeOutReceived(message));
        }

        


        #region Commands


        private void LaunchCommand(SwitchOn command)
        {                       
            //save main command for retries
            mainCommand = command;

            //Set timeout
            SetReceiveTimeout(TimeSpan.FromSeconds(5));

            //Launch command to frontend actors
            

            //Set state
            state = SwitchOnState.CommandLaunched;
        }

        private void ProcessTimeOut(ReceiveTimeout timeout)
        {
            if (retries < 3)
            {
                LaunchCommand(mainCommand);

                retries++;

            }
            else
            {
                SetReceiveTimeout(null);
                device.Tell(new CommandTimedOut() { CommandRequestID = commandID });
            }
        }


        #endregion


        #region Recover

        private bool RecoverLaunchCommand(SwitchOn message)
        {
            state = SwitchOnState.CommandLaunched;

            return true;
        }


        private bool RecoverTimeOutReceived(ReceiveTimeout message)
        {
            retries++;

            return true;
        }

        #endregion

    }
}

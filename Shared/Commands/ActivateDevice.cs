﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Commands
{
    public class ActivateDevice : Command
    {
        public string SerialNumber { get; set; }
    }
}

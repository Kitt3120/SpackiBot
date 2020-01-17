using System;
using System.Collections.Generic;
using System.Text;

namespace SpackiBot.Modules
{
    internal interface IModuleStatus
    {
        public ModuleStatus ServiceStatus { get; }
    }
}
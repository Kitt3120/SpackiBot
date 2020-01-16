using System;
using System.Collections.Generic;
using System.Text;

namespace SpackiBot.Services
{
    internal interface IServiceStatus
    {
        public ServiceStatus ServiceStatus { get; }
    }
}
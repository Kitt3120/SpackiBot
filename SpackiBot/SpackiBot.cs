using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpackiBot
{
    class SpackiBot
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public SpackiBot()
        {
            ServiceProvider = ContainerBuilder.BuildNewContainer();
        }
    }
}

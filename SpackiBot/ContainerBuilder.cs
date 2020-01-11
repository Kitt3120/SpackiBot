using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpackiBot
{
    class ContainerBuilder
    {
        public static IServiceProvider BuildNewContainer()
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            //TODO: Add interfaces & implementations

            return serviceCollection.BuildServiceProvider();
        }
    }
}

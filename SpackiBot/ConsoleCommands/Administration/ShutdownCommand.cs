using System;
using System.Collections.Generic;
using System.Text;

namespace SpackiBot.ConsoleCommands.Administration
{
    internal class ShutdownCommand : IConsoleCommand
    {
        public string Name => "Shutdown";

        public string[] Aliases => new string[] { "exit", "close", "bye", "byebye", "kill" };

        public string Description => "Shuts down the bot safely";

        private SpackiBot _spackiBot;

        public ShutdownCommand(SpackiBot spackiBot)
        {
            _spackiBot = spackiBot;
        }

        public void PrintUsage() => Console.WriteLine($"{Name} - {Description}");

        public void Run(string[] args)
        {
            _spackiBot.Shutdown();
        }
    }
}
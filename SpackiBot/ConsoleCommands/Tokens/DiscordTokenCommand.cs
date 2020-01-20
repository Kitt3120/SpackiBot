using SpackiBot.Properties;
using System;
using System.Security.Cryptography;

using SpackiBot.Properties;

namespace SpackiBot.ConsoleCommands.Tokens
{
    internal class DiscordTokenCommand : IConsoleCommand
    {
        public string Name => "DiscordToken";

        public string[] Aliases => new string[] { "discordt", "dtoken", "dt" };

        public string Description => "Manage which Token is being used for the Discord connection";

        public void PrintUsage() => Console.WriteLine($"{Name} Show - Shows the current Discord Bot-Token\n" +
                                                      $"{Name} Set <Token> - Sets a new Discord Bot-Token");

        public void Run(string[] args)
        {
            if (args.Length == 0)
                PrintUsage();

            string arg = args[0];
            if (arg.ToLower() == "show")
                Console.WriteLine($"Current Discord Bot-Token: {Properties.Tokens.Default.DiscordBot}");
            else if (arg.ToLower() == "set")
            {
                if (args.Length < 2)
                    PrintUsage();
                else
                {
                    Properties.Tokens.Default.DiscordBot = args[1];
                    Properties.Tokens.Default.Save();
                    Console.WriteLine(
                        "Discord Bot-Token saved\n" +
                        "Please restart in order to take effect.");
                }
            }
            else
                PrintUsage();
        }
    }
}
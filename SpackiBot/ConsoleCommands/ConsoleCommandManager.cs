using SpackiBot.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpackiBot.ConsoleCommands
{
    public class ConsoleCommandManager
    {
        private LoggingSection _loggingSection;
        private List<IConsoleCommand> _consoleCommands;

        public ConsoleCommandManager(IServiceProvider serviceProvider)
        {
            _loggingSection = new LoggingSection("ConsoleCommandManager");
            _loggingSection.Verbose("Reached ConsoleCommandManager");

            _loggingSection.Verbose("Searching IConsoleCommands to instanciate...");
            _consoleCommands = new List<IConsoleCommand>();
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => !p.IsInterface && typeof(IConsoleCommand).IsAssignableFrom(p)))
            {
                using (LoggingSection section = _loggingSection.CreateChild(type.Name))
                {
                    section.Verbose("Resolving services");
                    List<object> resolvedServices = new List<object>();
                    var parameters = type.GetConstructors().FirstOrDefault().GetParameters();
                    foreach (var parameter in parameters)
                    {
                        section.Debug("Resolving parameter " + parameter.ParameterType.Name);
                        object resolved = serviceProvider.GetService(parameter.ParameterType);
                        section.Debug("Resolves to " + resolved.ToString());
                        resolvedServices.Add(resolved);
                    }
                    _consoleCommands.Add((IConsoleCommand)Activator.CreateInstance(type, resolvedServices.ToArray()));
                }
            }
            _loggingSection.Verbose($"Found {_consoleCommands.Count} commands");
        }

        public void Handle(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;

            string command = input.Trim().Split(' ')[0];
            string[] args = input.Replace(command, "").Trim().Split(' ');

            IConsoleCommand foundCommand = _consoleCommands.Where(cmd => cmd.Name.ToLower() == command.ToLower() || cmd.Aliases.Contains(command.ToLower())).FirstOrDefault();
            if (foundCommand == null)
                Console.WriteLine($"Command {command} not found");
            else
                foundCommand.Run(args);
        }
    }
}
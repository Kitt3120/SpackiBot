namespace SpackiBot.ConsoleCommands
{
    internal interface IConsoleCommand
    {
        public string Name { get; }
        public string[] Aliases { get; }
        public string Description { get; }

        public void PrintUsage();

        public void Run(string[] args);
    }
}
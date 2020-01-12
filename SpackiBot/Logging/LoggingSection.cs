using System;
using System.Collections.Generic;

namespace SpackiBot.Logging
{
    class LoggingSection : IDisposable
    {
        public LoggingSection Parent { get; private set; }
        public bool HasParent { get { return Parent != null; } }

        public List<LoggingSection> Children { get; private set; }
        public bool HasChildren { get { return Children != null && Children.Count > 0; } }

        public List<LogMessage> History { get; private set; }

        public string Name { get; private set; }
        public string FullPath { get
            {
                string path = Name;
                LoggingSection currentSection = this;
                while (currentSection.HasParent)
                {
                    path = Parent.Name + "/" + path;
                    currentSection = Parent;
                }
                return path;
            } }

        public bool Disposed { get; private set; }


        public LoggingSection(string name) : this(name, null)
        {}

        public LoggingSection(string name, LoggingSection parent)
        {
            Name = name;

            Parent = parent;
            if (parent != null)
                parent.Children.Add(this);

            Children = new List<LoggingSection>();
            History = new List<LogMessage>();

            Disposed = false;
        }

        public LoggingSection CreateChild(string name)
        {
            if(Disposed)
                return null;
            return new LoggingSection(name, this);
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            Children.ForEach(child => child.Dispose());

            if (HasParent)
                Parent.Children.Remove(this);

            Parent = null;
            Name = null;
            History = null;

            Disposed = true;

            GC.SuppressFinalize(this);
            //Now waiting for garbage collector
        }

        //Wrappers
        public void Debug(string message) => Logger.Debug(this, message);
        public void Verbose(string message) => Logger.Verbose(this, message);
        public void Info(string message) => Logger.Info(this, message);
        public void Warning(string message) => Logger.Warning(this, message);
        public void Error(string message) => Logger.Error(this, message);
        public void Critical(string message) => Logger.Critical(this, message);
        public void Log(LogLevel logLevel, string message) => Logger.Log(this, logLevel, message);
    }
}

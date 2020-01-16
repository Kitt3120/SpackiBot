using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpackiBot.Services
{
    public class AssetsService
    {
        public string ProjectRoot { get; private set; }

        public AssetsService()
        {
            ProjectRoot = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
        }

        public bool ExistsDirectory(string path) => Directory.Exists(Get(path));
        public bool ExistsFile(string path) => File.Exists(Get(path));
        public string Get(string path) => Path.Combine(ProjectRoot, path);
    }
}
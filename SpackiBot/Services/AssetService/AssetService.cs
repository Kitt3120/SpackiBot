using SpackiBot.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SpackiBot.Services.AssetService
{
    public class AssetService : AssetFolder, IServiceStatus
    {
        private LoggingSection _loggingSection;

        private ServiceStatus _serviceStatus;
        ServiceStatus IServiceStatus.ServiceStatus { get => _serviceStatus; }

        public AssetService() : base(null, Path.Combine(AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin")), "Assets"))
        {
            _serviceStatus = ServiceStatus.Starting;

            _loggingSection = new LoggingSection("AssetService");
            _loggingSection.Verbose($"Loaded {SubFolders.Count} AssetFolders");

            _serviceStatus = ServiceStatus.Enabled;
        }

        public AssetFolder FindSub(string path, StringComparison stringComparison) => FindSub(path.Split(Path.DirectorySeparatorChar), stringComparison);

        public AssetFolder FindSub(string[] chain, StringComparison stringComparison)
        {
            if (chain.Length == 0)
                return this;

            AssetFolder assetFolder = FilterSubs(chain[0], stringComparison).FirstOrDefault();
            if (assetFolder == null)
                throw new DirectoryNotFoundException($"Directory {Path.Combine(Information.FullName, chain[0])} not found.");

            for (int i = 1; i < chain.Length; i++)
            {
                AssetFolder next = assetFolder.FilterSubs(chain[i], stringComparison).FirstOrDefault();
                if (next == null)
                    throw new DirectoryNotFoundException($"Directory {Path.Combine(assetFolder.Information.FullName, chain[i])} not found.");
                assetFolder = next;
            }

            return assetFolder;
        }

        public async Task WriteToAsync(AssetFolder assetFolder, string fileName, byte[] data, FileMode fileMode)
        {
            //await File.WriteAllBytesAsync(Path.Combine(assetFolder.FolderPath, fileName), data); Replaced by version below so file content can also be appended with FileMode.

            using (FileStream fileStream = File.Open(Path.Combine(assetFolder.Information.FullName, fileName), fileMode))
                await fileStream.WriteAsync(data);
        }

        public async Task<byte[]> ReadFromAsync(AssetFolder assetFolder, string fileName) => await File.ReadAllBytesAsync(Path.Combine(assetFolder.Information.FullName, fileName));
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpackiBot.Services.AssetService
{
    public class AssetFolder
    {
        public AssetFolder ParentFolder { get; private set; }
        public bool HasParentFolder { get => ParentFolder != null; }
        public List<AssetFolder> SubFolders { get; private set; }
        public List<string> Files { get; private set; }

        public DirectoryInfo Information { get; private set; }

        public AssetFolder(AssetFolder parentFolder, string directory)
        {
            ParentFolder = parentFolder;
            Information = new DirectoryInfo(parentFolder == null ? directory : Path.Combine(parentFolder.Information.FullName, directory));

            RefreshSubs();
            RefreshFiles();
        }

        public void RefreshSubs()
        {
            SubFolders = new List<AssetFolder>();
            foreach (var directory in Directory.EnumerateDirectories(Information.FullName))
                SubFolders.Add(new AssetFolder(this, directory));
        }

        public void RefreshFiles()
        {
            Files = new List<string>();
            foreach (var file in Directory.EnumerateFiles(Information.FullName))
                Files.Add(Path.Combine(Information.FullName, file));
        }

        public List<AssetFolder> FilterSubs(string filter, StringComparison stringComparison) => SubFolders.Where(assetFolder => assetFolder.Information.Name.Contains(filter, stringComparison)).ToList();

        public List<string> FilterFiles(string filter, StringComparison stringComparison) => Files.Where(file => file.Contains(filter, stringComparison)).ToList();
    }
}
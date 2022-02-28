using Microsoft.Extensions.Logging;
using ScanServer_NetCore.Services.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace ScanServer_NetCore.Services.Implementations
{
    public class UpdateService : IUpdateService
    {
        private string appDataFullPath;

        public UpdateService(string appDataFullPath)
        {
            this.appDataFullPath = appDataFullPath;
        }

        public FileInfo GetNewestFile()
        {
            var newestFile = GetNewestFileVersion();
            return new FileInfo(newestFile.FilePath);
        }

        public bool NewVersionAvailable(string requestingVersion)
        {
            var newestFile = GetNewestFileVersion();
            var requestingVersionInfo = new Version(requestingVersion);
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
            return newestFile.VersionInfo.CompareTo(requestingVersionInfo) switch
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
            {
                0 or -1 => false,// earlier than or same
                1 => true// later than
            };
        }

        private FileVersionEntry GetNewestFileVersion()
        {
            var appFiles = Directory.GetFiles(appDataFullPath);
            var fileByVersion = appFiles.Select(a => new FileVersionEntry(a)).ToArray();
            var orderedByVersion = fileByVersion.OrderByDescending(f => f.VersionInfo.Major).ThenByDescending(f => f.VersionInfo.Minor).ThenByDescending(f => f.VersionInfo.Build).ToList();
            var newestVersion = orderedByVersion.First();
            return newestVersion;
        }
    }

    class FileVersionEntry
    {
        public Version VersionInfo { get; }
        public string FilePath { get; }


        public FileVersionEntry(string filePath)
        {
            FilePath = filePath;
            VersionInfo = new Version(Path.GetFileNameWithoutExtension(filePath).Replace("scan_client_", string.Empty));
        }
    }
}

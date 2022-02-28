using ScanServer_NetCore.Exceptions;
using System.IO;

namespace ScanServer_NetCore
{
    public class AppSettings
    {
        public  string? BasePathUser { get; set; }

        public string? BasePathUserPassword { get; set; }

        public string? BasePath { get; set; }

        public string? ScanDataPath { get; set; }

        public string ScanDataFullPath => BasePath != null && ScanDataPath != null ? Path.Combine(BasePath, ScanDataPath) : throw new ScanServerException($"{nameof(BasePath)} as well as {nameof(ScanDataPath)} have to be set!");

        public string? AppDataPath { get; set; }

        public string AppDataFullPath => BasePath != null && AppDataPath != null ? Path.Combine(BasePath, AppDataPath) : throw new ScanServerException($"{nameof(BasePath)} as well as {nameof(AppDataPath)} have to be set!");

        public string? FoldersDateFormat { get; set; }
    }
}

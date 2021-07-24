using System.IO;

namespace ScanServer_NetCore.Models
{
    /// <summary>
    /// Represent location of a file
    /// </summary>
    public class FilePath
    {

        /// <summary>
        /// Name of directory, should be yyyyMMdd or smth like that
        /// </summary>
        public string DirectoryName { get; set; }

        /// <summary>
        /// Name of file
        /// </summary>
        public string FileName { get; set; }

        public override string ToString()
        {
            return Path.Combine(DirectoryName, FileName);
        }

    }
}

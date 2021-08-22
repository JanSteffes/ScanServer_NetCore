using System.IO;

namespace ScanServer_NetCore.Services.Helper
{
    /// <summary>
    /// Helper to provide functions when handling files/filePathes
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Check if file exists, and if so, add/increase count at the end of the fileName till it does not exist anymore.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Full path to valid file name</returns>
        public static string GetValidFileName(string filePath)
        {

            // get if file exists
            if (!File.Exists(filePath))
            {
                // does not, name can be used
                return filePath;
            }
            var directory = Path.GetDirectoryName(filePath);
            do
            {
                filePath = Path.Combine(directory, IncreaseFileName(Path.GetFileName(filePath)));
            } while (File.Exists(filePath));
            // change fileName with count at the end till file does not exists
            // return that name
            return filePath;
        }

        /// <summary>
        /// Add _x to fileName 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string IncreaseFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var pureFileName = Path.GetFileNameWithoutExtension(fileName);
            // get currentCount, end of file with _x where x is a number
            var posOfUnderscore = pureFileName.LastIndexOf("_");
            if (posOfUnderscore == -1 )
            {
                return pureFileName + "_0" + extension;
            }
            // there's an underscore contained, check if afterwords are only numbers
            var afterUnderScore = pureFileName[posOfUnderscore..];
            if (!int.TryParse(afterUnderScore, out var currentIndex))
            {
                return pureFileName + "_0" + extension;
            }
            return pureFileName.Substring(0, posOfUnderscore) + "_" + currentIndex + 1 + extension;
        }
    }
}

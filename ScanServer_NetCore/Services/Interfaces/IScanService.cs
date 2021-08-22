using JetBrains.Annotations;
using ScanServer_NetCore.Services.Enums;
using System.Threading.Tasks;

namespace ScanServer_NetCore.Services.Interfaces
{
    /// <summary>
    /// Service to handle scan releated tasks
    /// </summary>
    public interface IScanService
    {
        /// <summary>
        /// Scan a file with the given parameters and return the filename
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <param name="scanQuality"></param>
        /// <returns>filename on success, null on failure</returns>
        Task<string?> Scan(string folderName, string fileName, ScanQuality scanQuality);
    }
}

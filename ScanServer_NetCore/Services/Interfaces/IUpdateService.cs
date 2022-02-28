using System.IO;

namespace ScanServer_NetCore.Services.Interfaces
{
    /// <summary>
    /// Handle updates of the app
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Determinates if a new version is available by comparing the requesting one with the latest available
        /// </summary>
        /// <param name="requestingVersion"></param>
        /// <returns></returns>
        bool NewVersionAvailable(string requestingVersion);

        /// <summary>
        /// Return data of newest file per steam
        /// </summary>
        /// <returns></returns>
        FileInfo GetNewestFile();
    }
}

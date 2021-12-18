using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ScanServer_NetCore.Services.Interfaces
{
    /// <summary>
    /// Service to handle file operations.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Merges files together.
        /// </summary>
        /// <param name="folderName">folder for all thos efiles</param>
        /// <param name="resultFileName">resulting filename</param>
        /// <param name="filesToMerge">names of files to merge together</param>
        /// <returns>resultFileName or null on error</returns>
        string? MergeFiles(string folderName, string resultFileName, params string[] filesToMerge);

        /// <summary>
        /// Delete the given file
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileToDelete"></param>
        /// <returns>successfull or not</returns>
        bool DeleteFile(string folder, string fileToDelete);

        /// <summary>
        /// Return all files in the given folder ordered by name desc, null if folder doesn't exist.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        List<string>? ReadFilesOfFolder(string folder);

        /// <summary>
        /// Return stream to the file to read
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileToRead"></param>
        /// <returns></returns>
        FileStream? ReadFile(string folder, string fileToRead);

        /// <summary>
        /// Return list of folders
        /// </summary>
        /// <returns></returns>
        List<string> ReadFolders();

        /// <summary>
        /// Rename file in directory
        /// </summary>
        /// <param name="directoryOfFile"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns>new name of file or null on error</returns>
        string? RenameFile(string directoryOfFile, string oldName, string newName);

        /// <summary>
        /// Create thumbnail of a files first page and return it's bytes. Will also save that thumbnail and return that next time instead of calculating anew. Should delete all thumbnails not "needed" anymore (file deleted/renamed).
        /// </summary>
        /// <param name="directoryOfFile"></param>
        /// <param name="fileToRead"></param>
        /// <returns>bytes of generated thumbnail</returns>
        Task<byte[]?> GetThumbnailOfFile(string directoryOfFile, string fileToRead);
    }
}

using ScanServer_NetCore.Models;
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
        /// <param name="filesToMerge">files to merge together</param>
        /// <param name="resultFileName">resulting filename</param>
        /// <returns>successfull or not</returns>
        bool MergeFiles(List<FilePath> filesToMerge, FilePath resultFileName);

        /// <summary>
        /// Delete the given file
        /// </summary>
        /// <param name="fileToDelete"></param>
        /// <returns>successfull or not</returns>
        bool DeleteFile(FilePath fileToDelete);

        /// <summary>
        /// Return all files in the given folder, null if folder doesn't exist.
        /// </summary>
        /// <param name="Folder"></param>
        /// <returns></returns>
        Task<List<FilePath>> ReadFilesOfFolderAsync(string folder);

        /// <summary>
        /// Return stream to the file to read
        /// </summary>
        /// <param name="fileToRead"></param>
        /// <returns></returns>
        Task<FileStream> ReadFileAsync(FilePath fileToRead);

        /// <summary>
        /// Rename file in directory
        /// </summary>
        /// <param name="directoryOfFile"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        Task<bool> RenameFileAsync(string directoryOfFile, string oldName, string newName);


    }
}

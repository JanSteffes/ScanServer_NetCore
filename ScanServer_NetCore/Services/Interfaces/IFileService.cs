using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;

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
        /// <returns>resultFileName</returns>
        [CanBeNull]
        string MergeFiles(string folderName, string resultFileName, params string[] filesToMerge);

        /// <summary>
        /// Delete the given file
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileToDelete"></param>
        /// <returns>successfull or not</returns>
        bool DeleteFile(string folder, string fileToDelete);

        /// <summary>
        /// Return all files in the given folder, null if folder doesn't exist.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        [CanBeNull]
        List<string> ReadFilesOfFolder(string folder);

        /// <summary>
        /// Return stream to the file to read
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileToRead"></param>
        /// <returns></returns>
        [CanBeNull]
        FileStream ReadFile(string folder, string fileToRead);

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
        /// <returns></returns>
        [CanBeNull]
        string RenameFile(string directoryOfFile, string oldName, string newName);
    }
}

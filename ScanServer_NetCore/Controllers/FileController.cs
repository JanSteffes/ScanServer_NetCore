using Microsoft.AspNetCore.Mvc;
using ScanServer_NetCore.Services.Interfaces;
using System.Collections.Generic;

namespace ScanServer_NetCore.Controllers
{
    /// <summary>
    /// Controller to hanlde FileActions like merging, deleting, showing,...
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private IFileService _fileService;

        public FileController(
            IFileService fileService)
        {
            _fileService = fileService;
        }


        /// <summary>
        /// Merge the given files and return result name.
        /// </summary>
        /// <param name="filesToMerge"></param>
        /// <param name="resultFileName"></param>
        /// <returns>name of the result file, can be null</returns>
        [Route("[action]/{folder}/{resultFileName}")]
        [HttpPost]
        public string MergeFiles([FromRoute] string folder, [FromRoute] string resultFileName, [FromBody] List<string> filesToMerge)
        {
            var result = _fileService.MergeFiles(folder, resultFileName, filesToMerge.ToArray());
            return result;
        }


        /// <summary>
        /// Delete given file.
        /// </summary>
        /// <param name="fileToDelete"></param>
        /// <returns></returns>
        [Route("[action]/{folder}/{fileName}")]
        [HttpDelete]
        public bool DeleteFile([FromRoute] string folder, [FromRoute] string fileName)
        {
            var result = _fileService.DeleteFile(folder, fileName);
            return result;
        }


        /// <summary>
        /// Return file
        /// </summary>
        /// <param name="fileToRead"></param>
        /// <returns></returns>
        [HttpGet]
        public FileResult ReadFile([FromQuery] string folder, [FromQuery] string fileToRead)
        {
            var fileStream = _fileService.ReadFile(folder, fileToRead);
            return new FileStreamResult(fileStream, "application/pdf")
            {
                FileDownloadName = fileToRead,
                EnableRangeProcessing = true
            };
        }



        /// <summary>
        /// Return all files in a specific directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        public List<string> ReadFiles([FromQuery] string directory)
        {
            var files = _fileService.ReadFilesOfFolder(directory);
            return files;
        }


        /// <summary>
        /// Rename a file
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="oldFileName"></param>
        /// <param name="newFileName"></param>
        /// <returns></returns>
        [Route("[action]/{folder}/{oldFileName}/{newFileName}")]
        [HttpPatch]
        public string RenameFile(string folder, string oldFileName, string newFileName)
        {
            var renamedName = _fileService.RenameFile(folder, oldFileName, newFileName);
            return renamedName;
        }


        /// <summary>
        /// Return all folders
        /// </summary>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        public List<string> ReadFolders()
        {
            var folders = _fileService.ReadFolders();
            return folders;
        }



       
     }
}

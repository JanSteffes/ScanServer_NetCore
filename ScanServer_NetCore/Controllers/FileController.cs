using Microsoft.AspNetCore.Mvc;
using ScanServer_NetCore.Services.Interfaces;
using System.Collections.Generic;
using System.Net;

namespace ScanServer_NetCore.Controllers
{
    /// <summary>
    /// Controller to hanlde FileActions like merging, deleting, showing,...
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        /// <summary>
        /// Service to handle files
        /// </summary>
        private readonly IFileService _fileService;

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
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public ActionResult MergeFiles([FromRoute] string folder, [FromRoute] string resultFileName, [FromBody] List<string> filesToMerge)
        {
            var result = _fileService.MergeFiles(folder, resultFileName, filesToMerge.ToArray());
            if (string.IsNullOrEmpty(result))
            {
                return BadRequest($"Could not merge files!");
            }
            return Ok(result);
        }


        /// <summary>
        /// Delete given file.
        /// </summary>
        /// <param name="fileToDelete"></param>
        /// <returns></returns>
        [Route("[action]/{folder}/{fileName}")]
        [HttpDelete]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public ActionResult DeleteFile([FromRoute] string folder, [FromRoute] string fileName)
        {
            var result = _fileService.DeleteFile(folder, fileName);
            return Ok(result);
        }


        /// <summary>
        /// Return file
        /// </summary>
        /// <param name="fileToRead"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public ActionResult ReadFile([FromQuery] string folder, [FromQuery] string fileToRead)
        {
            var fileStream = _fileService.ReadFile(folder, fileToRead);
            if (fileStream == null)
            {
                return BadRequest($"Failed to read file!");
            }
            return new FileStreamResult(fileStream, "application/pdf")
            {
                FileDownloadName = fileToRead,
                EnableRangeProcessing = true
            };
        }



        /// <summary>
        /// Return all files in a specific directory ordererd descending by name (newest first)
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public ActionResult ReadFiles([FromQuery] string directory)
        {
            var files = _fileService.ReadFilesOfFolder(directory);
            if (files == null)
            {
                return BadRequest($"Could not read files of folder!");
            }
            return Ok(files);
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
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public ActionResult RenameFile(string folder, string oldFileName, string newFileName)
        {
            var renamedName = _fileService.RenameFile(folder, oldFileName, newFileName);
            if (string.IsNullOrEmpty(renamedName))
            {
                return BadRequest($"Renaming the file failed!");
            }
            return Ok(renamedName);
        }


        /// <summary>
        /// Return all folders
        /// </summary>
        /// <returns></returns>
        [Route("[action]")]
        [HttpGet]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        public ActionResult ReadFolders()
        {
            var folders = _fileService.ReadFolders();
            return Ok(folders);
        }



       
     }
}

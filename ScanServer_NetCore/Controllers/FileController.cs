using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScanServer_NetCore.Models;
using ScanServer_NetCore.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private readonly ILogger<FileController> _logger;

        public FileController(
            IFileService fileService, 
            ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }


        /// <summary>
        /// Merge the given files and return result.
        /// </summary>
        /// <param name="filesToMerge"></param>
        /// <param name="resultFileName"></param>
        /// <returns></returns>
        [Route("[action]/{resultFileName}")]
        [HttpPost]
        public bool MergeFiles([FromBody] List<FilePath> filesToMerge, [FromQuery] FilePath resultFileName)
        {
            var result = _fileService.MergeFiles(filesToMerge, resultFileName);
            return result;
        }


        /// <summary>
        /// Delete given file.
        /// </summary>
        /// <param name="fileToDelete"></param>
        /// <returns></returns>
        [HttpDelete]
        public bool DeleteFile(FilePath fileToDelete)
        {
            var result = _fileService.DeleteFile(fileToDelete);
            return result;
        }


        /// <summary>
        /// Return stream to file
        /// </summary>
        /// <param name="fileToRead"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<FileResult> ReadFile([FromQuery] FilePath fileToRead)
        {
            var fileStream = await _fileService.ReadFileAsync(fileToRead);
            return new FileStreamResult(fileStream, "application/pdf")
            {
                FileDownloadName = fileToRead.FileName,
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
        public async Task<List<FilePath>> ReadFiles([FromQuery] string directory)
        {
            _logger.LogInformation($"Reading files from folder '{directory}'");
            var files = await _fileService.ReadFilesOfFolderAsync(directory);
            return files;
        }


        /// <summary>
        /// Rename a file
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="oldFileName"></param>
        /// <param name="newFileName"></param>
        /// <returns></returns>
        [HttpPatch]
        public async Task<bool> RenameFile(string directory, string oldFileName, string newFileName)
        {
            var renamed = await _fileService.RenameFileAsync(directory, oldFileName, newFileName);
            return renamed;
        }



       
     }
}

using Microsoft.Extensions.Logging;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using ScanServer_NetCore.Models;
using ScanServer_NetCore.Services.Helper;
using ScanServer_NetCore.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ScanServer_NetCore.Services.Implementations
{
    public class FileService : IFileService
    {
        private ILogger _logger;
        private readonly string _baseFolder;

        public FileService(ILoggerFactory loggerFactory, string baseFolder)
        {
            _logger = loggerFactory.CreateLogger<FileService>();
            _baseFolder = baseFolder;
        }

        public bool DeleteFile(FilePath fileToDelete)
        {
            var filePath = Path.Combine(_baseFolder, fileToDelete.DirectoryName, fileToDelete.FileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {fileToDelete} could not be found!");
            }
            File.Delete(filePath);
            var fileGotDeleted = !File.Exists(filePath);
            return fileGotDeleted;
        }

        public bool MergeFiles(List<FilePath> filesToMerge, FilePath resultFile)
        {
            var fullFilePaths = filesToMerge.Select(f => Path.Combine(_baseFolder, f.ToString())).ToList();
            var doAllFilesExist = fullFilePaths.All(filePath => File.Exists(filePath));
            if (!doAllFilesExist)
            {
                var nonExistingFile = fullFilePaths.FirstOrDefault(file => !File.Exists(file));
                throw new FileNotFoundException($"File at '{nonExistingFile}' could not be found!");
            }
            //var commandToExecute = $"pdftk {string.Join(" ", filePaths)} cat output {resultFile}";
            using var outputDocument = new PdfDocument();
            _logger.LogInformation($"Starting to merge {fullFilePaths.Count} files...");
            foreach (var filePath in fullFilePaths)
            {
                _logger.LogInformation($"processing file '{filePath}'..");
                using var currentDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
                foreach(var page in currentDocument.Pages)
                {
                    outputDocument.AddPage(page);
                }
            }
            var resultFilePath = Path.Combine(_baseFolder, resultFile.ToString());
            resultFilePath = FileHelper.GetValidFileName(resultFilePath);
            using var resultFileStream = File.OpenWrite(resultFilePath);
            outputDocument.Save(resultFileStream);
            return File.Exists(resultFilePath);
        }

        public async Task<FileStream> ReadFileAsync(FilePath fileToRead)
        {
            var filePath = Path.Combine(_baseFolder, fileToRead.DirectoryName, fileToRead.FileName);
            if (File.Exists(filePath))
            {
                return File.OpenRead(filePath);
            }
            return null;
        }

        public async Task<List<FilePath>> ReadFilesOfFolderAsync(string folder)
        {
            var folderPath = Path.Combine(_baseFolder, folder + "/");
            _logger.LogInformation($"Reading files from folder '{folderPath}'");
            var files = Directory.GetFiles(folderPath);
            _logger.LogInformation($"Got {files.Count()} files: {string.Join(",",files)}");
            var returnValue = files.Select(f => new FilePath
            {
                DirectoryName = folder,
                FileName = Path.GetFileName(f)
            }).ToList();
            return returnValue;
        }

        public async Task<bool> RenameFileAsync(string directoryOfFile, string oldName, string newName)
        {
            return false;
        }
    }
}

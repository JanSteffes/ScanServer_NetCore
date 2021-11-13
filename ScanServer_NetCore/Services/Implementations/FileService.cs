using Microsoft.Extensions.Logging;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using ScanServer_NetCore.Services.Helper;
using ScanServer_NetCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScanServer_NetCore.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly ILogger _logger;
        private readonly string _baseFolder;

        public FileService(ILoggerFactory loggerFactory, string baseFolder)
        {
            _logger = loggerFactory.CreateLogger<FileService>();
            _baseFolder = baseFolder;
        }

        public bool DeleteFile(string folder, string fileToDelete)
        {
            var filePath = Path.Combine(_baseFolder, folder, fileToDelete);
            if (!File.Exists(filePath))
            {
                return false;
            }
            try
            {
                File.Delete(filePath);
                var fileGotDeleted = !File.Exists(filePath);
                return fileGotDeleted;
            }
            catch(Exception e)
            {
                _logger.LogError($"Exception while trying to delete file at '{filePath}'!", e);
                return false;
            }
        }


        public string? MergeFiles(string folderName, string resultFileName, params string[] filesToMerge)
        {
            var workingFolder = Path.Combine(_baseFolder, folderName);
            var fullFilePaths = filesToMerge.Select(f => Path.Combine(workingFolder, f)).ToList();
            var nonExistingFile = fullFilePaths.FirstOrDefault(file => !File.Exists(file));
            if (!string.IsNullOrEmpty(nonExistingFile))
            {
                _logger.LogError($"File at '{nonExistingFile}' could not be found!");
                return null;
            }
            using var outputDocument = new PdfDocument();
            _logger.LogInformation($"Starting to merge {fullFilePaths.Count} files...");
            foreach (var filePath in fullFilePaths)
            {
                _logger.LogInformation($"Processing file '{filePath}'..");
                using var currentSourceDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
                foreach (var page in currentSourceDocument.Pages)
                {
                    outputDocument.AddPage(page);
                }
            }
            var resultFilePath = Path.Combine(workingFolder, resultFileName);
            resultFilePath = FileHelper.GetValidFileName(resultFilePath);
            using var resultFileStream = File.OpenWrite(resultFilePath);
            outputDocument.Save(resultFileStream);
            // didn't work
            if (!File.Exists(resultFilePath))
            {
                return null;
            }
            return Path.GetFileName(resultFilePath);
        }

        public FileStream? ReadFile(string folder, string fileToRead)
        {
            var filePath = Path.Combine(_baseFolder, folder, fileToRead);
            if (File.Exists(filePath))
            {
                return File.OpenRead(filePath);
            }
            return null;
        }
        
        public List<string>? ReadFilesOfFolder(string folder)
        {
            var folderPath = Path.Combine(_baseFolder, folder);
            _logger.LogInformation($"Reading files from folder '{folderPath}'");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            var files = Directory.GetFiles(folderPath);
            _logger.LogInformation($"Got {files.Count()} files: {string.Join(",", files)}");
            var returnValue = files.Select(f => Path.GetFileName(f)).OrderByDescending(s => s).ToList();
            return returnValue;
        }

        public string? RenameFile(string folder, string oldName, string newName)
        {
            var folderPath = Path.Combine(_baseFolder, folder);
            var oldFilePath = Path.Combine(folderPath, oldName);
            if (!File.Exists(oldFilePath))
            {
                return null;
            }
            var newFilePath = Path.Combine(folderPath, newName);
            var validatedNewFilePath = FileHelper.GetValidFileName(newFilePath);      
            File.Copy(oldFilePath, validatedNewFilePath);
            File.Delete(oldFilePath);
            return Path.GetFileName(validatedNewFilePath);
        }

        public List<string> ReadFolders()
        {
            var folders = Directory.GetDirectories(_baseFolder).Select(p => Path.GetFileName(p)).OrderByDescending(s => s).ToList();
            return folders;
        }
    }
}

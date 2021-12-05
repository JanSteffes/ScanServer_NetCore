using Microsoft.Extensions.Logging;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using ScanServer_NetCore.Services.Helper;
using ScanServer_NetCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static ScanServer_NetCore.Services.Helper.CommandHelper;

namespace ScanServer_NetCore.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Basefolder to work in
        /// </summary>
        private readonly string _baseFolder;

        private double thumbNailWidthToHeightRatio = 1.4142449185284730388039643877037;

        private int thumbNailWidthPoints = 2480; // see https://www.papersizes.org/a-sizes-in-pixels.htm

        private int thumbNailHeightPoints => (int)(thumbNailWidthPoints * thumbNailWidthToHeightRatio);

        /// <summary>
        /// Command to create thumbnails using ghostscript.
        /// <br />{0} is inputfile to create thumnbail from
        /// <br />{1} is tempfile to read data from and delete after doing so
        /// <br /> ratio from width to height should be 1 to 1.4.1 (100 width = 141 height)
        /// </summary>
        private string ghostScriptThumbnailCommand => $"gs -sDEVICE=jpeg -dPDFFitPage=true -dFirstPage=1 -dLastPage=1 -dNOPAUSE -dBATCH -dDEVICEWIDTHPOINTS={thumbNailWidthPoints} -dDEVICEHEIGHTPOINTS={thumbNailHeightPoints} -sOutputFile=";

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

        public async Task<byte[]?> GetThumbnailOfFile(string directoryOfFile, string fileToRead)
        {
            // create thumbnail using ghostscript
            var folderPath = Path.Combine(_baseFolder, directoryOfFile);
            var inputFilePath = Path.Combine(folderPath, fileToRead);
            if (!File.Exists(inputFilePath) || Path.GetExtension(inputFilePath).ToLowerInvariant() != ".pdf")
            {
                return null;
            }
            var tempFileName = Path.GetTempFileName();
            string createThumbnailCommand;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                createThumbnailCommand = ghostScriptThumbnailCommand + $"'{tempFileName}'" + " " + $"'{inputFilePath}'";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                createThumbnailCommand = ghostScriptThumbnailCommand + @$"""{tempFileName}""" + " " + @$"""{inputFilePath}""";
            }
            else
            {
                throw new NotImplementedException($"Function not implemented for '{RuntimeInformation.OSDescription}'!");
            }
            _logger.LogInformation($"Will create thumbnail with command '{createThumbnailCommand}'");
            await ExecuteCommand(createThumbnailCommand);
            if (!File.Exists(tempFileName))
            {
                return null;
            }
            var bytes = await File.ReadAllBytesAsync(tempFileName);
            File.Delete(tempFileName);
            if (File.Exists(tempFileName))
            {
                throw new Exception($"File '{tempFileName}' did not get deleted properly!");
            }
            Debug.WriteLine($"==> Generated thumbnail of '{fileToRead}' is {BytesToHumanReadable((ulong)bytes.Length)}");
            return bytes;
        }

        public const long OneKB = 1024;

        public const long OneMB = OneKB * OneKB;

        public const long OneGB = OneMB * OneKB;

        public const long OneTB = OneGB * OneKB;

        public static string BytesToHumanReadable(ulong bytes)
        {
            return bytes switch
            {
                (< OneKB) => $"{bytes}B",
                (>= OneKB) and (< OneMB) => $"{bytes / OneKB}KB",
                (>= OneMB) and (< OneGB) => $"{bytes / OneMB}MB",
                (>= OneGB) and (< OneTB) => $"{bytes / OneMB}GB",
                (>= OneTB) => $"{bytes / OneTB}"
                //...
            };
        }
    }
}

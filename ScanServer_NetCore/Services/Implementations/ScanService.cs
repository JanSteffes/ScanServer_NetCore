using Microsoft.Extensions.Logging;
using ScanServer_NetCore.Services.Enums;
using ScanServer_NetCore.Services.Helper;
using ScanServer_NetCore.Services.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ScanServer_NetCore.Services.Implementations
{
    public class ScanService : IScanService
    {
        private readonly ILogger _logger;
        private readonly string _baseFolder;

        public ScanService(ILoggerFactory loggerFactory, string baseFolder)
        {
            _logger = loggerFactory.CreateLogger<ScanService>();
            _baseFolder = baseFolder;
        }

        public async Task<string?> Scan(string folderName, string fileName, ScanQuality scanQuality)
        {
            if (Path.GetExtension(fileName) != ".pdf")
            {
                fileName += ".pdf";
            }
            var workingDir = Path.Combine(_baseFolder, folderName);  
            if (!Directory.Exists(workingDir))
            {
                Directory.CreateDirectory(workingDir);
            }
            var filePath = Path.Combine(workingDir, fileName);
            // check if filename is taken and generate new name
            var targetFile = FileHelper.GetValidFileName(filePath);
            if (Path.GetFileName(targetFile) != fileName)
            {
                _logger.LogInformation($"FileName '{fileName}' in folder '{folderName}' was already taken, will use '{Path.GetFileName(targetFile)}' instead");
            }

            // define files that will be used while scanning
            var tempTiffFile = Path.Combine(workingDir, "tempTiff.tiff");
            var tempPdfFile = Path.Combine(workingDir, "tempPdf.pdf");
            var tempPostScriptFile = Path.Combine(workingDir, "tempPostscript.ps");
            // cleanup if existing already
            var tempFiles = new[] { tempTiffFile, tempPdfFile, tempPostScriptFile };
            foreach (var file in tempFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            try
            {
                // scan
                var scanCommand = $"scanimage --format=tiff --resolution { (int)scanQuality } > {tempTiffFile}";
                _logger.LogInformation($"Will scan with command '{scanCommand}'..");
                await ExecuteCommand(scanCommand);
                if (!File.Exists(tempTiffFile))
                {
                    return null;
                }

                // convert fiff to pdf
                var tiffToPdfCommand = $"tiff2pdf -o {tempPdfFile} {tempTiffFile}";
                _logger.LogInformation($"Will convert to temp pdf with command '{tiffToPdfCommand}'");
                await ExecuteCommand(tiffToPdfCommand);
                if (!File.Exists(tempPdfFile))
                {
                    return null;
                }

                // convert to postscript
                var postScriptCommand = $"pdftops {tempPdfFile} {tempPostScriptFile}";
                _logger.LogInformation($"Will convert to ps with command '{postScriptCommand}'");
                await ExecuteCommand(postScriptCommand);
                if (!File.Exists(tempPostScriptFile))
                {
                    return null;
                }

                // convert to final pdf
                var finalPdfConvertCommand = $"ps2pdf {tempPostScriptFile} {targetFile}";
                _logger.LogInformation($"Will convert to final pdf with command '{finalPdfConvertCommand}'");
                await ExecuteCommand(finalPdfConvertCommand);
                if (!File.Exists(targetFile))
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                _logger.LogError($"Exception happend while scanning file: {e.Message}", e);
                return null;
            }
            finally
            {
                // cleanup temp files
                foreach (var file in tempFiles)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
            }

            return targetFile;
        }

        /// <summary>
        /// Execute a command in local shell
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task ExecuteCommand(string command)
        {
            var proc = new Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \" " + command + " \"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            await proc.WaitForExitAsync();            
        }
    }
}

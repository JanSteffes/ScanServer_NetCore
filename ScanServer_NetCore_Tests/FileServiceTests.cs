using Microsoft.Extensions.Logging;
using NUnit.Framework;
using PdfSharpCore.Pdf.IO;
using ScanServer_NetCore.Services.Implementations;
using ScanServer_NetCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using ScanServer_NetCore.Services.Helper;

namespace ScanServer_NetCore_Tests
{

    public class FileServiceTests
    {
        /// <summary>
        /// Service to test
        /// </summary>
        private IFileService _fileService;

        /// <summary>
        /// Temp directory these tests will work on
        /// </summary>
        private string _baseWorkingDir;

        [TearDown]
        public void TearDown()
        {
            // delete all created data
            var baseWorkingDir = Path.Combine(Path.GetTempPath(), "Scans");
            Directory.Delete(baseWorkingDir, true);
        }

        [SetUp]
        public void Setup()
        {
            // setup fileService
            _baseWorkingDir = Path.Combine(Path.GetTempPath(), "Scans");
            if (Directory.Exists(_baseWorkingDir))
            {
                Directory.Delete(_baseWorkingDir, true);
            }
            Directory.CreateDirectory(_baseWorkingDir);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug().AddConsole();
            });
            _fileService = new FileService(loggerFactory, _baseWorkingDir);


            // setup folders and files
            var exampleFilesFolderPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles");
            if (!Directory.Exists(exampleFilesFolderPath))
            {
                throw new Exception("Example files do not exist!");
            }
            // copy all directories of there to temp path
            foreach(var directory in Directory.GetDirectories(exampleFilesFolderPath))
            {
                var lastIndexOfSeperator = directory.LastIndexOf(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar.ToString().Length;
                var dirName = directory[lastIndexOfSeperator..];
                var tempDirPath = Path.Combine(_baseWorkingDir, dirName);
                Directory.CreateDirectory(tempDirPath);
                foreach(var file in Directory.GetFiles(directory))
                {
                    var fileName = Path.GetFileName(file);
                    var newFilePath = Path.Combine(tempDirPath, fileName);
                    File.Copy(file, newFilePath);
                }
            }

            // create empty directory
            var emptyDirFolderName = DateTime.Today.ToString("yyyy-MM-dd");
            var emptyDirPath = Path.Combine(_baseWorkingDir, emptyDirFolderName);
            Directory.CreateDirectory(emptyDirPath);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.DeleteFile(string, string)"/>
        /// </summary>
        [Test]
        public void DeleteFileTest()
        {
            var folder = "2021-01-01";
            var file = "ExamplePdf_1.pdf";
            var deleteResult = _fileService.DeleteFile(folder, file);
            Assert.IsTrue(deleteResult);
            Assert.IsFalse(File.Exists(Path.Combine(_baseWorkingDir, folder, file)));
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.DeleteFile(string, string)"/>, if file does not exist
        /// </summary>
        [Test]
        public void DeleteFileTestNonExisting()
        {
            var folder = "2021-01-01";
            var file = "ExamplePdf_2.pdf";
            var deleteResult = _fileService.DeleteFile(folder, file);
            Assert.IsFalse(deleteResult);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.MergeFiles(string, string, List{string}))"/>
        /// </summary>
        [Test]
        public void MergeFilesTest()
        {
            var folder = "2021-01-01";
            var file1 = "ExamplePdf_1.pdf";
            var file2 = "ExamplePdf_5.pdf";
            var resultFile1 = "ExamplePdf_Merged.pdf";
            var resultFile2 = "ExamplePdf_Merged_0.pdf";
            var merged = _fileService.MergeFiles(folder, resultFile1, file1, file2);
            var resultFile1Path = Path.Combine(_baseWorkingDir, folder, resultFile1);
            Assert.NotNull(merged);
            Assert.AreEqual(resultFile1, merged);
            Assert.IsTrue(File.Exists(resultFile1Path));
            using var resultFile1Document = PdfReader.Open(resultFile1Path);
            Assert.AreEqual(6, resultFile1Document.PageCount);

            var merged2 = _fileService.MergeFiles(folder, resultFile1, file1, file2);
            Assert.NotNull(merged2);
            Assert.AreEqual(merged2, resultFile2);
            var resultFile2Path = Path.Combine(_baseWorkingDir, folder, resultFile2);
            Assert.IsTrue(File.Exists(resultFile2Path));
            using var resultFile2Document = PdfReader.Open(resultFile2Path);
            Assert.AreEqual(6, resultFile2Document.PageCount);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.ReadFile(string, string)"/>, if file exists
        /// </summary>
        [Test]
        public async Task ReadFileTest()
        {
            var folder = "2021-01-01";
            var file = "ExamplePdf_1.pdf";
            var stream = _fileService.ReadFile(folder, file);
            Assert.IsNotNull(stream);
            await stream.DisposeAsync();
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.ReadFile(string, string)"/>, if file does not exist
        /// </summary>
        [Test]
        public void ReadFileTestNotExistingFile()
        {
            var folder = "2021-01-01";
            var file = "ExamplePdf_2.pdf";
            var stream = _fileService.ReadFile(folder, file);
            Assert.IsNull(stream);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.ReadFile(string, string)"/>, if folder does not exist
        /// </summary>
        [Test]
        public void ReadFileTestNotExistingFolder()
        {
            var folder = "2021-01-02";
            var file = "ExamplePdf_1.pdf";
            var stream = _fileService.ReadFile(folder, file);
            Assert.IsNull(stream);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.ReadFilesOfFolder(string)"/>, if folder exists (with expected result count)
        /// </summary>
        [Test]
        public void ReadFilesOfFolderTest()
        {
            var expectedFileCounts = new List<(string directory, string[] expectedFiles)>{
                // usual case
                ("2021-01-01", (new[]{"ExamplePdf_1.pdf", "ExamplePdf_5.pdf"})),
                ("2021-02-25", new[]{"ExamplePdf_1.pdf", "ExamplePdf_3.pdf", "ExamplePdf_4.pdf"}),
                ("2021-03-15", new[]{"ExamplePdf_1.pdf", "ExamplePdf_2.pdf","ExamplePdf_3.pdf","ExamplePdf_4.pdf","ExamplePdf_5.pdf","ExamplePdf_6.pdf","ExamplePdf_7.pdf","ExamplePdf_8.pdf","ExamplePdf_9.pdf","ExamplePdf_10.pdf"})               
            };
            foreach (var (folder, expectedFiles) in expectedFileCounts)
            {
                var filesInFolder = _fileService.ReadFilesOfFolder(folder);
                CollectionAssert.AreEquivalent(expectedFiles, filesInFolder);
                var expectedFirstFile = expectedFiles.OrderByDescending(s => s).First();
                Assert.AreEqual(expectedFirstFile, filesInFolder.First());
                var expectedLastFile = expectedFiles.OrderByDescending(s => s).Last();
                Assert.AreEqual(expectedLastFile, filesInFolder.Last());
            }
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.ReadFilesOfFolder(string)"/>, if folder is empty
        /// </summary>
        [Test]
        public void ReadFilesOfFolderTestEmptyFolder()
        {
            var folder = DateTime.Today.ToString("yyyy-MM-dd");
            var expectedFileCount = 0;
            var filesInFolder = _fileService.ReadFilesOfFolder(folder);
            var filesInFolderCount = filesInFolder?.Count;
            Assert.AreEqual(expectedFileCount, filesInFolderCount);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.ReadFilesOfFolder(string)"/>, if folder does not exist
        /// </summary>
        [Test]
        public void ReadFilesOfFolderTestNonExistingFolder()
        {
            var folder = "2021-01-02";      
            var filesInFolder = _fileService.ReadFilesOfFolder(folder);
            Assert.IsNull(filesInFolder);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.ReadFolders"/>
        /// </summary>
        [Test]
        public void ReadFoldersTest()
        {
            var expectedOutcomes = new List<string> { 
                "2021-01-01",
                "2021-02-25",
                "2021-03-15",
                DateTime.Today.ToString("yyyy-MM-dd")
            };
            var foldersRead = _fileService.ReadFolders();
            CollectionAssert.AreEquivalent(expectedOutcomes, foldersRead);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.RenameFile(string, string, string)"/>
        /// </summary>
        [Test]
        public void RenameFileTest()
        {
            var folder = "2021-01-01";
            var file1 = "ExamplePdf_1.pdf";
            var resultFile = "ExamplePdf_1_renamed.pdf";
            var renamed = _fileService.RenameFile(folder, file1, resultFile);
            Assert.AreEqual(resultFile, renamed);            
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.RenameFile(string, string, string)"/>, if sourceFile does not exist
        /// </summary>
        [Test]
        public void RenameFileTestNonExistingSource()
        {
            var folder = "2021-01-01";
            var file1 = "ExamplePdf_2.pdf";
            var resultFile = "ExamplePdf_2_renamed.pdf";
            var renamed = _fileService.RenameFile(folder, file1, resultFile);
            Assert.IsNull(renamed);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.RenameFile(string, string, string)"/>, if file is renamed if resultFile already exists but renamed correctly
        /// </summary>
        [Test]
        public void RenameFileTestAlreadyExistingResult()
        {
            var folder = "2021-01-01";
            var file1 = "ExamplePdf_5.pdf";
            var resultFile = "ExamplePdf_5.pdf";
            var expectedFileName = "ExamplePdf_5_0.pdf";
            var renamed = _fileService.RenameFile(folder, file1, resultFile);
            Assert.AreEqual(expectedFileName, renamed);
        }

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.GetThumbnailOfFile(string, string)"/>
        /// </summary>
        [Test]
        public async Task CreateThumbnailTest()
        {
            var folder = "2021-01-01";
            var file1 = "ExamplePdf_5.pdf";
            var bytes = await _fileService.GetThumbnailOfFile(folder, file1);
            Assert.IsNotNull(bytes);
            var length = bytes.Length;
            Assert.Greater(length, 0, "Created thumbnailData has no bytes!");
        }       

        /// <summary>
        /// For for <see cref="FileService"/>s implementaion of method <see cref="IFileService.RenameFile(string, string, string)"/>, if file does not exist
        /// </summary>
        [Test]
        public async Task CreateThumbnailTestNonExistingFile()
        {
            var folder = "2021-01-01";
            var file1 = "ExamplePdf_6.pdf";
            var bytes = await _fileService.GetThumbnailOfFile(folder, file1);
            Assert.IsNull(bytes);
        }

        #region Dynamicly create folders and files

        //private void PrepareDynamicFoldersAndFiles()
        //{
        //    //// max number of pages for one document
        //    //const int maxPages = 10;
        //    //const int maxFilesPerFolder = 5;
        //    //// read on example files
        //    //var exampleFilesDataTasks = Directory.GetFiles(exampleFilesFolderPath).Where(f => Path.GetFileName(f).Contains("_")).Select(f => GetExamplePdfData(f));
        //    //var exampleFilesData = (await Task.WhenAll(exampleFilesDataTasks)).ToDictionary(d => d.numberOfPages, d => d.fileBytes);

        //    //var toDate = DateTime.Today;
        //    //var fromDate = toDate.AddDays(-5);
        //    //var random = new Random();
        //    //while (fromDate <= toDate)
        //    //{
        //    //    var currentDateFolderName = fromDate.ToString("yyyy-MM-dd");
        //    //    var currentDateFolderPath = Path.Combine(baseWorkingDir, currentDateFolderName);
        //    //    Directory.CreateDirectory(currentDateFolderPath);
        //    //    var fileCountForFolder = random.Next(0, maxFilesPerFolder);
        //    //    for (int i = 0; i < fileCountForFolder; i++)
        //    //    {
        //    //        var currentNumberOfPages = 1;
        //    //        while (random.NextDouble() > 0.5 && currentNumberOfPages < maxPages)
        //    //        {
        //    //            currentNumberOfPages++;
        //    //        }
        //    //        var currentFileName = $"File{i}_{currentNumberOfPages}.pdf";
        //    //        var currentFilePath = Path.Combine(currentDateFolderPath, currentFileName);
        //    //        var pdfData = exampleFilesData[currentNumberOfPages];
        //    //        await File.WriteAllBytesAsync(currentFilePath, pdfData);
        //    //    }
        //    //    fromDate = fromDate.AddDays(1);
        //    //}
        //}

        ///// <summary>
        ///// Get data of example file
        ///// </summary>
        ///// <param name="pdfFilePath"></param>
        ///// <returns></returns>
        //private async Task<(int numberOfPages, byte[] fileBytes)> GetExamplePdfData(string pdfFilePath)
        //{
        //    var filePages = int.Parse(Path.GetFileNameWithoutExtension(pdfFilePath).Split("_")[1]);
        //    var fileBytes = await File.ReadAllBytesAsync(pdfFilePath);
        //    return (filePages, fileBytes);
        //}

        #endregion Dynamicly create folders and files
    }
}

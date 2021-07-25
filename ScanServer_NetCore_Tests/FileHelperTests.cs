using NUnit.Framework;
using ScanServer_NetCore.Services.Helper;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ScanServer_NetCore_Tests
{    
    public class FileHelperTests
    {

        [SetUp]
        public void Setup()
        {

        }

        /// <summary>
        /// Test to check if file not exists, the filename is still the same
        /// </summary>
        [Test]
        public static void FileDoesNotExists()
        {
            var fileName = Path.GetTempFileName();
            File.Delete(fileName);
            var validFileName = FileHelper.GetValidFileName(fileName);
            Assert.AreEqual(fileName, validFileName);      
        }

        /// <summary>
        /// Test to test basic functionality if file exists, so fileName has to differ and resultFile cannot exist already
        /// </summary>
        /// <returns></returns>
        [Test]
        public static async Task FileExistsAlready()
        {
            var fileName = Path.GetTempFileName();
            await CreateFile(fileName);
            try
            {
                var validFileName = FileHelper.GetValidFileName(fileName);
                Assert.AreNotEqual(fileName, validFileName);
                Assert.IsFalse(File.Exists(validFileName));
            }
            finally
            {
                File.Delete(fileName);
            }
        }


        /// <summary>
        /// Test if increasing filenames work if files exist already
        /// </summary>
        /// <returns></returns>
        [Test]
        public static async Task FilesExistsAlready()
        {
            var createdFiles = new List<string>();
            try
            {
                var fileName = Path.GetTempFileName();
                await CreateFile(fileName);
                for (int count = 0; count < 15; count++)
                {
                    var directory = Path.GetDirectoryName(fileName);
                    var increasedCountFileName = Path.GetFileNameWithoutExtension(fileName) + $"_{count}" + Path.GetExtension(fileName);
                    var resultFile = Path.Combine(directory, increasedCountFileName);
                    createdFiles.Add(resultFile);
                    await CreateFile(resultFile);
                }
                var validName = FileHelper.GetValidFileName(fileName);
                Assert.IsFalse(File.Exists(validName));
            }
            finally
            {
                foreach(var file in createdFiles)
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// Create empty file at given path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static async Task CreateFile(string filePath)
        {
            await File.Create(filePath).DisposeAsync();
        }
    }
}

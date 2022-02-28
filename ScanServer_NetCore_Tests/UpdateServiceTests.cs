using Microsoft.Extensions.Logging;
using NUnit.Framework;
using ScanServer_NetCore.Services.Implementations;
using ScanServer_NetCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanServer_NetCore_Tests
{
    public class UpdateServiceTests
    {
        /// <summary>
        /// Service to test
        /// </summary>
        private IUpdateService _updateService;

        /// <summary>
        /// Temp directory these tests will work on
        /// </summary>
        private string _baseWorkingDir;

        [TearDown]
        public void TearDown()
        {
            // delete all created data
            var baseWorkingDir = Path.Combine(Path.GetTempPath(), "AppData");
            Directory.Delete(baseWorkingDir, true);
        }

        [SetUp]
        public void Setup()
        {
            // setup fileService
            _baseWorkingDir = Path.Combine(Path.GetTempPath(), "AppData");
            if (Directory.Exists(_baseWorkingDir))
            {
                Directory.Delete(_baseWorkingDir, true);
            }
            Directory.CreateDirectory(_baseWorkingDir);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddDebug().AddConsole();
            });
            _updateService = new UpdateService(_baseWorkingDir);


            // setup folders and files
            var exampleFilesFolderPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "AppData");
            if (!Directory.Exists(exampleFilesFolderPath))
            {
                throw new Exception("Example files do not exist!");
            }
            // copy all files of there to temp path          
            foreach (var file in Directory.GetFiles(exampleFilesFolderPath))
            {
                var fileName = Path.GetFileName(file);
                var newFilePath = Path.Combine(_baseWorkingDir, fileName);
                File.Copy(file, newFilePath);
            }
            
        }

        /// <summary>
        /// For for <see cref="UpdateService"/>s implementaion of method <see cref="IUpdateService.GetNewestFile"/>, if file exists
        /// </summary>
        [Test]
        public void ReturnLatestFileTest()
        {
            var fileInfo = _updateService.GetNewestFile();
            Assert.IsNotNull(fileInfo);
            Assert.AreEqual("scan_client_1.0.1.apk", fileInfo.Name);
        }

        /// <summary>
        /// For for <see cref="UpdateService"/>s implementaion of method <see cref="IUpdateService.NewVersionAvailable(string)"/>, if requesting with older version
        /// </summary>
        [Test]
        public void NewVersionAvailableOlderVersion()
        {
            var versionAvailable = _updateService.NewVersionAvailable("1.0.0");
            Assert.IsTrue(versionAvailable);
        }

        /// <summary>
        /// For for <see cref="UpdateService"/>s implementaion of method <see cref="IUpdateService.NewVersionAvailable(string)"/>, if requesting with newer version
        /// </summary>
        [Test]
        public void NewVersionAvailableNewerVersion()
        {

            var versionAvailable = _updateService.NewVersionAvailable("1.0.2");
            Assert.IsFalse(versionAvailable);
        }

        /// <summary>
        /// For for <see cref="UpdateService"/>s implementaion of method <see cref="IUpdateService.NewVersionAvailable(string)"/>, if requesting with same version
        /// </summary>
        [Test]
        public void NewVersionAvailableSameVersion()
        {

            var versionAvailable = _updateService.NewVersionAvailable("1.0.1");
            Assert.IsFalse(versionAvailable);
        }
    }


}

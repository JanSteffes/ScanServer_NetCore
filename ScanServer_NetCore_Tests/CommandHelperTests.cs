using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using ScanServer_NetCore.Services.Helper;

namespace ScanServer_NetCore_Tests
{

    public class CommandHelperTests
    {
        private const string testOfCreationFileName = "H:\\TestCreationOfFile.txt";


        private const string pdfFile = @"""H:\\1.pdf""";
        private const string thumbnail = @"""H:\\1.jpeg""";
        private const string ghostScriptThumbnailCommand = "gs -sDEVICE=jpeg -dPDFFitPage=true -dFirstPage=1 -dLastPage=1 -dNOPAUSE -dBATCH -dDEVICEWIDTHPOINTS=250 -dDEVICEHEIGHTPOINTS=250 -sOutputFile=";
        private const string thumbnailPath = "H:\\1.jpeg";

        [SetUp]
        public void Setup()
        {
            if (File.Exists(testOfCreationFileName))
            {
                File.Delete(testOfCreationFileName);
            }
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(testOfCreationFileName))
            {
                File.Delete(testOfCreationFileName);
            }
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
            }
        }


        [Test]
        public async Task TestSimpleExcecution()
        {
            var command = "dir H:\\";
            await CommandHelper.ExecuteCommand(command);
            Assert.Pass();
        }

        [Test]
        public async Task TestCreationOfFile()
        {
            var fileName = testOfCreationFileName;
            var command = $"echo hello >> \"{fileName}\"";
            await CommandHelper.ExecuteCommand(command);
            Assert.IsTrue(File.Exists(fileName));
            var fileBytes = await File.ReadAllBytesAsync(fileName);
            var fileBytesCount = fileBytes.Length;
            Assert.Greater(fileBytesCount, 0);
        }

        [Test]
        public async Task TestPdfCreation()
        {
            var command = ghostScriptThumbnailCommand + thumbnail + " "  + pdfFile;
            await CommandHelper.ExecuteCommand(command);
            Assert.IsTrue(File.Exists(thumbnailPath));
            var fileBytes = await File.ReadAllBytesAsync(thumbnailPath);
            var fileBytesCount = fileBytes.Length;
            Assert.Greater(fileBytesCount, 0);
        }
    }
}

using VideoLibraryManager.Helper;
using VideoLibraryManager.Services;
using NSubstitute;

namespace VideoLibraryManager.Tests
{
    [TestFixture]
    public class LocalVideoFileManagementServiceTest
    {
        private Microsoft.Extensions.Logging.ILogger _logger;
        private IDirectoryHelper _directoryHelper;
        private ILocalVideoFileManagementService _localVideoFileManagementService;

        [SetUp]
        public void Setup()
        {
            _logger = Substitute.For<Microsoft.Extensions.Logging.ILogger>();
            _directoryHelper = Substitute.For<IDirectoryHelper>();
            _localVideoFileManagementService = new LocalVideoFileManagementService(_logger, _directoryHelper);
        }

        [Test]
        public void RenameVideoTest()
        {
            string fileName = "Video1.mp4";
            string newFileName = "Video2.mp4";
            string filePath = "C:\\Videos\\Video1.mp4";
            _directoryHelper.DoesFileExist("C:\\Videos\\Video2.mp4").Returns(false);

            _localVideoFileManagementService.RenameVideo(fileName, filePath, newFileName);

            _directoryHelper.Received().DoesFileExist("C:\\Videos\\Video2.mp4");
            _directoryHelper.Received().MoveFile("C:\\Videos\\Video1.mp4", "C:\\Videos\\Video2.mp4");
        }

        [Test]
        public async Task LoadVideosAsyncTest()
        {
            string[] sampleFiles = new string[]
            {
                "D:\\Videos\\Video2.mp4",
                "D:\\Videos\\Video1.mp4"
            };
            string[] sampleDirectories = new string[]
            {
                "D:\\Videos\\"
            };

            _directoryHelper.DoesDirectoryExist(Arg.Any<string>()).Returns(true);
            _directoryHelper.GetFiles("D:\\Videos\\").Returns(sampleFiles);
            _directoryHelper.GetDirectories("D:\\").Returns(sampleDirectories);
           
            var videoFiles = await _localVideoFileManagementService.LoadVideosAsync();

           _directoryHelper.Received().GetFiles("D:\\Videos\\");
           _directoryHelper.Received().GetDirectories("D:\\");

        }

        [Test]
        public void DeleteVideoTest()
        {
            string filePath = "C:\\Videos\\Video1.mp4";
            _directoryHelper.DoesFileExist(filePath).Returns(true);

            _localVideoFileManagementService.DeleteVideo(filePath);

            _directoryHelper.Received().DeleteFile(filePath);
        }

    }
}

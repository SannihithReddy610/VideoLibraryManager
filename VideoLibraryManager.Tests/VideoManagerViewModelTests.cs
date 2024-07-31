//using NUnit.Framework;
//using Moq;
//using VideoLibraryManager.Model;
//using VideoLibraryManager.ViewModel;
//using Microsoft.Extensions.Logging;

//namespace VideoLibraryManager.Tests
//{
//    public class VideoManagerViewModelTests
//    {
//        private Mock<ILogger<VideoManagerViewModel>> _logger;
//        private VideoManagerViewModel _viewModel;

//        [SetUp]
//        public void Setup()
//        {
//            _logger = new Mock<ILogger<VideoManagerViewModel>>();
//            _viewModel = new VideoManagerViewModel(_logger.Object);
//        }

//        [Test]
//        public async Task LoadVideosAsync_LoadsVideosSuccessfully()
//        {
//            // Arrange
//            var expectedVideos = new List<VideoFile>
//            {
//                new VideoFile("C:\\Videos\\video1.mp4"),
//                new VideoFile ("C:\\Videos\\video2.mp4"),
//                new VideoFile ("D:\\Videos\\video3.mp4")
//            };
//            //_viewModel.GetVideoFilesAsync = async () => expectedVideos; // Override private method for testing

//            // Act
//            await _viewModel.LoadVideosAsync();

//            // Assert
//            Assert.AreEqual("Local Videos loaded successfully.", _viewModel.StatusMessage);
//            Assert.AreEqual(2, _viewModel.Folders.Count); // Assuming two folders: C:\Videos and D:\Videos
//            // Add more assertions to verify video content and folder structure
//        }

//        [Test]
//        public async Task LoadVideosAsync_HandlesException()
//        {
//            // Arrange
//            var expectedException = new Exception("Test exception");
//            _viewModel.GetVideoFilesAsync = async () => throw expectedException;

//            // Act
//            await _viewModel.LoadVideosAsync();

//            // Assert
//            Assert.AreEqual($"An error occurred while loading local videos: {expectedException.Message}", _viewModel.StatusMessage);
//            _logger.Verify(l => l.LogError(expectedException, It.IsAny<string>()), Times.Once);
//        }
//    }
//}
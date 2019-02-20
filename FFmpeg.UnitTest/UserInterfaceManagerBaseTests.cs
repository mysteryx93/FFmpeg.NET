using System;
using EmergenceGuardian.FFmpeg.Services;
using Moq;
using Xunit;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class UserInterfaceManagerBaseTests {

        protected const string TestTitle = "job title";
        protected const string TestFileName = "test";
        protected const int TestJobId = 0;
        protected Mock<IFFmpegConfig> config;

        protected Mock<FakeUserInterfaceManagerBase> SetupUI() {
            return new Mock<FakeUserInterfaceManagerBase>() { CallBase = true };
        }

        public IProcessManager SetupManager() {
            config = new Mock<IFFmpegConfig>();
            var parser = new FFmpegParser();
            var factory = new FakeProcessFactory();
            var fileSystem = new FakeFileSystemService();
            return new ProcessManager(config.Object, parser, factory, fileSystem);
        }

        [Theory]
        [InlineData(0)]
        [InlineData("")]
        public void Start_Valid_CreateUiCalled(object jobId) {
            var UiMock = SetupUI();

            UiMock.Object.Start(jobId, TestTitle);

            UiMock.Verify(x => x.CreateUI(TestTitle, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public void Start_NullJobId_ThrowsNullException() {
            var UiMock = SetupUI();

            Assert.Throws<ArgumentNullException>(() => UiMock.Object.Start(null, TestTitle));
        }

        [Fact]
        public void Stop_Valid_StopCalledOnWindow() {
            var UiMock = SetupUI();

            UiMock.Object.Start(TestJobId, TestTitle);
            UiMock.Object.Stop(TestJobId);

            Assert.Single(UiMock.Object.Instances);
            var WMock = Mock.Get<IUserInterfaceWindow>(UiMock.Object.Instances[0]);
            WMock.Verify(x => x.Stop(), Times.Once);
        }

        [Fact]
        public void Display_ProcessManagerWithJobId_DisplayTaskCalledOnWindow() {
            var UiMock = SetupUI();
            var Manager = SetupManager();
            Manager.Options.JobId = TestJobId;

            UiMock.Object.Start(TestJobId, TestTitle);
            UiMock.Object.Display(Manager);

            var WMock = Mock.Get<IUserInterfaceWindow>(UiMock.Object.Instances[0]);
            WMock.Verify(x => x.DisplayTask(It.IsAny<IProcessManager>()), Times.Once);
        }

        [Fact]
        public void Display_ProcessManagerWithTitle_CreateUiCalled() {
            var UiMock = SetupUI();
            var Manager = SetupManager();
            Manager.Options.Title = TestTitle;

            UiMock.Object.Display(Manager);

            UiMock.Verify(x => x.CreateUI(TestTitle, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public void RunWithOptionDisplayErrorOnly_Timeout_DisplayError() {
            var UiMock = SetupUI();
            var Manager = SetupManager();
            config.Setup(x => x.UserInterfaceManager).Returns(UiMock.Object);
            Manager.Options.DisplayMode = FFmpegDisplayMode.ErrorOnly;
            Manager.Options.Timeout = TimeSpan.FromMilliseconds(10);
            Manager.ProcessStarted += (s, e) => {
                var PMock = Mock.Get<IProcess>(e.ProcessManager.WorkProcess);
                PMock.Setup(x => x.WaitForExit(It.IsAny<int>())).Returns(false);
            };

            Manager.Run(TestFileName, null);

            UiMock.Verify(x => x.DisplayError(Manager), Times.Once);
        }
    }
}

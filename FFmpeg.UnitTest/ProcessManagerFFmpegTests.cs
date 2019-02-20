using System;
using Moq;
using Xunit;
using EmergenceGuardian.FFmpeg.Services;
using System.IO;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class ProcessManagerFFmpegTests {

        #region Utility Functions

        protected const string AppFFmpeg = "ffmpeg.exe";
        protected const string AppCmd = "cmd";
        protected const string MissingFileName = "MissingFile";
        protected Mock<IFFmpegConfig> config;

        protected IProcessManagerFFmpeg SetupManager() {
            config = new Mock<IFFmpegConfig>();
            config.Setup(x => x.FFmpegPath).Returns(AppFFmpeg);
            var parser = new FFmpegParser();
            var factory = new FakeProcessFactory();
            var fileSystem = Mock.Of<FakeFileSystemService>(x =>
                x.Exists(It.IsAny<string>()) == true && x.Exists(MissingFileName) == false);
            return new ProcessManagerFFmpeg(config.Object, parser, factory, fileSystem);
        }

        /// <summary>
        /// Feeds a sample output into a mock process.
        /// </summary>
        /// <param name="p">The process manager to feed data into..</param>
        /// <param name="output">The sample output to feed.</param>
        protected void FeedOutputToProcess(IProcessManager p, string output) {
            p.ProcessStarted += (s, e) => {
                var MockP = Mock.Get<IProcess>(p.WorkProcess);
                using (StringReader sr = new StringReader(output)) {
                    string line;
                    while ((line = sr.ReadLine()) != null) {
                        MockP.Raise(x => x.ErrorDataReceived += null, ProcessManagerTests.CreateMockDataReceivedEventArgs(line));
                    }
                }
                MockP.Raise(x => x.ErrorDataReceived += null, ProcessManagerTests.CreateMockDataReceivedEventArgs(null));
            };
        }

        #endregion

        #region Constructors

        [Fact]
        public void Constructor_Empty_Success() => new ProcessManagerFFmpeg();

        [Fact]
        public void Constructor_NullDependencies_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessManagerFFmpeg(null, null, null, null, null));

        [Fact]
        public void Constructor_Dependencies_Success() => new ProcessManagerFFmpeg(new FFmpegConfig(), new FFmpegParser(), new ProcessFactory(), new FileSystemService(), null);

        [Fact]
        public void Constructor_OptionFFmpeg_Success() => new ProcessManagerFFmpeg(new FFmpegConfig(), new FFmpegParser(), new ProcessFactory(), new FileSystemService(), new ProcessOptionsFFmpeg());

        #endregion

        #region Options

        [Fact]
        public void Options_SetOptionsFFmpeg_ReturnsSame() {
            var Manager = SetupManager();
            var Options = new ProcessOptionsFFmpeg();

            Manager.Options = Options;

            Assert.Equal(Options, Manager.Options);
        }

        [Fact]
        public void Options_SetOptionsBase_ReturnsNullBaseReturnsSame() {
            var Manager = SetupManager();
            var ManagerBase = Manager as ProcessManager;
            var Options = new ProcessOptions();

            ManagerBase.Options = Options;

            Assert.Null(Manager.Options);
            Assert.Equal(ManagerBase.Options, Options);
        }

        #endregion

        #region RunFFmpeg

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("args")]
        public void RunFFmpeg_Valid_CommandContainAppAndArgs(string args) {
            var Manager = SetupManager();

            var Result = Manager.RunFFmpeg(args);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Contains(AppFFmpeg, Manager.CommandWithArgs);
            if (!string.IsNullOrEmpty(args))
                Assert.Contains(args, Manager.CommandWithArgs);
        }

        [Fact]
        public void RunFFmpeg_AppNotFound_ThrowsFileNotFoundException() {
            var Manager = SetupManager();
            config.Setup(x => x.FFmpegPath).Returns(MissingFileName);

            Assert.Throws<FileNotFoundException>(() => Manager.RunFFmpeg(null));
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, 100)]
        public void RunFFmpeg_OptionFrameCount_ReturnsSpecifiedFrameCount(string output, int frameCount) {
            var Manager = SetupManager();
            Manager.Options.FrameCount = frameCount;
            FeedOutputToProcess(Manager, output);

            Manager.RunFFmpeg(null);

            Assert.Equal(frameCount, Manager.FrameCount);
        }

        #endregion

        #region RunAvisynthToEncoder

        [Theory]
        [InlineData("source", null)]
        [InlineData("source", "")]
        [InlineData("source", "args")]
        public void RunAvisynthToEncoder_Valid_CommandContainsCmdAndSourceAndArgs(string source, string args) {
            var Manager = SetupManager();

            var R = Manager.RunAvisynthToEncoder(source, args);

            Assert.Equal(CompletionStatus.Success, R);
            Assert.Contains(AppCmd, Manager.CommandWithArgs);
            Assert.Contains(source, Manager.CommandWithArgs);
            if (!string.IsNullOrEmpty(args))
                Assert.Contains(args, Manager.CommandWithArgs);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RunAvisynthToEncoder_EmptyArg_ThrowsException(string source) {
            var Manager = SetupManager();

            Assert.Throws<ArgumentException>(() => Manager.RunAvisynthToEncoder(source, null));
        }

        #endregion

        #region RunFFmpeg InjectOutput

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1)]
        public void RunFFmpeg_InjectOutput_StatusSuccess(string output) {
            var Manager = SetupManager();
            FeedOutputToProcess(Manager, output);

            var Result = Manager.RunFFmpeg(null);

            Assert.Equal(CompletionStatus.Success, Result);
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, 7163)]
        public void RunFFmpeg_InjectOutput_ExpectedFrameCount(string output, int expectedFrameCount) {
            var Manager = SetupManager();
            FeedOutputToProcess(Manager, output);

            var Result = Manager.RunFFmpeg(null);

            Assert.Equal(expectedFrameCount, Manager.FrameCount);
            Assert.True(Manager.FileDuration > TimeSpan.Zero);
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, 30)]
        public void RunFFmpeg_InjectOutput_EventsTriggered(string output, int statusLines) {
            var Manager = SetupManager();
            int DataReceivedCalled = 0;
            Manager.DataReceived += (s, e) => DataReceivedCalled++;
            int InfoUpdatedCalled = 0;
            Manager.InfoUpdated += (s, e) => InfoUpdatedCalled++;
            int StatusUpdatedCalled = 0;
            Manager.StatusUpdated += (s, e) => StatusUpdatedCalled++;
            FeedOutputToProcess(Manager, output);

            var Result = Manager.RunFFmpeg(null);

            Assert.True(DataReceivedCalled > 0);
            Assert.Equal(1, InfoUpdatedCalled);
            Assert.Equal(statusLines, StatusUpdatedCalled + 1); // Last is NULL.
        }

        [Theory]
        [InlineData(OutputSamples.FFmpegEncode1, "mpeg1video", "mp2")]
        public void RunFFmpeg_InjectOutput_ExpectedStreams(string output, string videoFormat, string audioFormat) {
            var Manager = SetupManager();
            FeedOutputToProcess(Manager, output);

            var Result = Manager.RunFFmpeg(null);

            if (videoFormat != null) {
                Assert.NotNull(Manager.VideoStream);
                Assert.Equal(videoFormat, Manager.VideoStream.Format);
            } else
                Assert.Null(Manager.VideoStream);
            if (audioFormat != null) {
                Assert.NotNull(Manager.AudioStream);
                Assert.Equal(audioFormat, Manager.AudioStream.Format);
            } else
                Assert.Null(Manager.AudioStream);
        }

        #endregion

    }
}
using System;
using Moq;
using Xunit;
using EmergenceGuardian.FFmpeg.Services;
using System.IO;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class ProcessManagerAvsTests {
        protected const string AppAvs2yuv = "avs2yuv.exe";
        protected const string MissingFileName = "MissingFile";
        protected Mock<IFFmpegConfig> config;

        protected IProcessManagerAvs SetupManager() {
            config = new Mock<IFFmpegConfig>();
            config.Setup(x => x.Avs2yuvPath).Returns(AppAvs2yuv);
            var parser = new FFmpegParser();
            var factory = new FakeProcessFactory();
            var fileSystem = Mock.Of<FakeFileSystemService>(x => 
                x.Exists(It.IsAny<string>()) == true && x.Exists(MissingFileName) == false);
            return new ProcessManagerAvs(config.Object, parser, factory, fileSystem);
        }

        [Fact]
        public void Constructor_Empty_Success() => new ProcessManagerAvs();

        [Fact]
        public void Constructor_NullDependencies_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new ProcessManagerAvs(null, null, null, null, null));

        [Fact]
        public void Constructor_Dependencies_Success() => new ProcessManagerAvs(new FFmpegConfig(), new FFmpegParser(), new ProcessFactory(), new FileSystemService(), null);

        [Fact]
        public void Constructor_OptionGeneric_Success() => new ProcessManagerAvs(new FFmpegConfig(), new FFmpegParser(), new ProcessFactory(), new FileSystemService(), new ProcessOptions());

        [Fact]
        public void Constructor_OptionFFmpeg_Success() => new ProcessManagerAvs(new FFmpegConfig(), new FFmpegParser(), new ProcessFactory(), new FileSystemService(), new ProcessOptionsFFmpeg());

        [Theory]
        [InlineData("file")]
        public void RunAvisynth_ValidFile_CommandContainsAvs2yuv(string path) {
            var Manager = SetupManager();

            var Result = Manager.RunAvisynth(path);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Contains(AppAvs2yuv, Manager.CommandWithArgs);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RunAvisynth_NoFile_ThrowsException(string path) {
            var Manager = SetupManager();

            Assert.Throws<ArgumentException>(() => Manager.RunAvisynth(path));
        }

        [Theory]
        [InlineData("file")]
        public void RunAvisynth_AvsNotFound_ThrowsFileNotFoundException(string path) {
            var Manager = SetupManager();
            config.Setup(x => x.Avs2yuvPath).Returns(MissingFileName);

            Assert.Throws<FileNotFoundException>(() => Manager.RunAvisynth(path));
        }
    }
}

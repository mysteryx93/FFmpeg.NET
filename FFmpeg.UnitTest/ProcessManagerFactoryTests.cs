using System;
using Moq;
using Xunit;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class ProcessManagerFactoryTests {
        protected Mock<IFFmpegConfig> config;

        protected IProcessManagerFactory SetupFactory() {
            var moq = new MockRepository(MockBehavior.Strict);
            config = moq.Create<IFFmpegConfig>();
            var Parser = moq.Create<IFFmpegParser>();
            var ProcessFactory = moq.Create<IProcessFactory>();
            var FileSystem = moq.Create<IFileSystemService>();
            return new ProcessManagerFactory(config.Object, Parser.Object, ProcessFactory.Object, FileSystem.Object);
        }

        [Fact]
        public void Constructor_NoParam_Success() => new ProcessManagerFactory().Create();

        [Fact]
        public void Constructor_Empty_Null_Success() => new ProcessManagerFactory("", null).Create();

        [Fact]
        public void Constructor_InjectDependencies_Success() => new ProcessManagerFactory(new FFmpegConfig(), new FFmpegParser(), new ProcessFactory(), new FileSystemService()).Create();

        [Fact]
        public void Constructor_NullDependencies_Success() => new ProcessManagerFactory(null, null, null, null).Create();

        [Fact]
        public void Constructor_InjectOneDependency_Success() => new ProcessManagerFactory(new FFmpegConfig(), null, null, null).Create();

        [Fact]
        public void Create_NoParam_ReturnsProcessManager() {
            var Factory = SetupFactory();

            var Result = Factory.Create();

            Assert.NotNull(Result);
            Assert.IsType<ProcessManager>(Result);
            Assert.Equal(config.Object, Result.Config);
        }

        [Fact]
        public void Create_ParamOptions_ReturnsSameOptions() {
            var Factory = SetupFactory();
            ProcessOptions options = new ProcessOptions();

            var Result = Factory.Create(options);

            Assert.Equal(options, Result.Options);
        }
    }
}

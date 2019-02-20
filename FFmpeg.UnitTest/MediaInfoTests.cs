using System;
using System.Linq;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class MediaInfoTests {

        #region Declarations

        protected FakeProcessManagerFactory factory = new FakeProcessManagerFactory();
        private readonly ITestOutputHelper output;

        public MediaInfoTests(ITestOutputHelper output) {
            this.output = output;
        }

        #endregion

        #region Utility Functions

        protected IMediaInfo SetupInfo() {
            return new MediaInfo(factory);
        }

        protected void AssertSingleInstance() {
            string ResultCommand = factory.Instances.FirstOrDefault()?.CommandWithArgs;
            output.WriteLine(ResultCommand);
            Assert.Single(factory.Instances);
            Assert.NotNull(ResultCommand);
        }

        #endregion

        #region Constructors

        [Fact]
        public void Constructor_Empty_Success() => new MediaInfo();

        [Fact]
        public void Constructor_WithFactory_Success() => new MediaInfo(factory);

        [Fact]
        public void Constructor_NullFactory_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaInfo(null));

        #endregion

        #region GetVersion

        [Fact]
        public void GetVersion_Valid_ReturnsProcessManager() {
            var Info = SetupInfo();

            IProcessManagerFFmpeg Result = Info.GetVersion();

            AssertSingleInstance();
        }

        [Fact]
        public void GetVersion_ParamOptions_ReturnsSame() {
            var Info = SetupInfo();
            var Options = new ProcessOptionsFFmpeg();

            Info.GetVersion(Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Fact]
        public void GetVersion_ParamCallback_CallbackCalled() {
            var Info = SetupInfo();
            int CallbackCalled = 0;

            IProcessManagerFFmpeg Result = Info.GetVersion(null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region GetFileInfo

        [Theory]
        [InlineData("source")]
        public void GetFileInfo_Valid_ReturnsProcessManager(string source) {
            var Info = SetupInfo();

            Info.GetFileInfo(source);

            AssertSingleInstance();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetFileInfo_EmptyArg_ThrowsException(string source) {
            var Info = SetupInfo();

            Assert.Throws<ArgumentException>(() => Info.GetFileInfo(source));
        }

        [Theory]
        [InlineData("source")]
        public void GetFileInfo_ParamOptions_ReturnsSame(string source) {
            var Info = SetupInfo();
            var Options = new ProcessOptionsFFmpeg();

            Info.GetFileInfo(source, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Theory]
        [InlineData("source")]
        public void GetFileInfo_ParamCallback_CallbackCalled(string source) {
            var Info = SetupInfo();
            int CallbackCalled = 0;

            Info.GetFileInfo(source, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region GetFrameCount

        [Theory]
        [InlineData("source", 10)]
        public void GetFrameCount_Valid_ReturnsFrameCount(string source, int expected) {
            var Info = SetupInfo();
            factory.FrameCount = expected;

            var Result = Info.GetFrameCount(source);

            Assert.Equal(expected, Result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetFrameCount_EmptyArg_ThrowsException(string source) {
            var Info = SetupInfo();

            Assert.Throws<ArgumentException>(() => Info.GetFrameCount(source));
        }

        [Theory]
        [InlineData("source")]
        public void GetFrameCount_ParamOptions_ReturnsSame(string source) {
            var Info = SetupInfo();
            var Options = new ProcessOptionsFFmpeg();

            Info.GetFrameCount(source, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Theory]
        [InlineData("source")]
        public void GetFrameCount_ParamCallback_CallbackCalled(string source) {
            var Info = SetupInfo();
            int CallbackCalled = 0;

            Info.GetFrameCount(source, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

    }
}
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.FFmpeg.IntegrationTests {
    public class MediaInfoTests {
        private readonly ITestOutputHelper output;

        public MediaInfoTests(ITestOutputHelper output) {
            this.output = output;
        }

        private IMediaInfo SetupInfo() {
            IProcessManagerFactory Factory = new ProcessManagerFactory(Properties.Settings.Default.FFmpegPath);
            return new MediaInfo(Factory);
        }

        [Fact]
        public void GetVersion_Valid_ReturnsVersionInfo() {
            var Info = SetupInfo();

            var Worker = Info.GetVersion();

            output.WriteLine(Worker.Output);
            Assert.NotEmpty(Worker.Output);
            Assert.Contains("version", Worker.Output);
        }

        [Theory]
        [InlineData(AppPaths.Mpeg4, 1)]
        [InlineData(AppPaths.Mpeg2, 1)]
        [InlineData(AppPaths.Flv, 2)]
        [InlineData(AppPaths.StreamAac, 1)]
        [InlineData(AppPaths.StreamH264, 1)]
        [InlineData(AppPaths.StreamOpus, 1)]
        [InlineData(AppPaths.StreamVp9, 1)]
        public void GetFileInfo_Valid_ReturnsWorkerWithStreams(string source, int streamCount) {
            var Info = SetupInfo();
            var Src = AppPaths.GetInputFile(source);

            var Worker = Info.GetFileInfo(Src);

            output.WriteLine(Worker.Output);
            Assert.NotNull(Worker.FileStreams);
            Assert.Equal(streamCount, Worker.FileStreams.Count());
        }

        [Theory]
        [InlineData("invalidfile")]
        public void GetFileInfo_InvalidFile_ReturnsWorkerWithEmptyStreamList(string source) {
            var Info = SetupInfo();
            var Src = AppPaths.GetInputFile(source);

            var Worker = Info.GetFileInfo(Src);

            output.WriteLine(Worker.Output);
            Assert.NotNull(Worker.FileStreams);
            Assert.Empty(Worker.FileStreams);
        }

        [Theory]
        [InlineData(AppPaths.Mpeg2)]
        [InlineData(AppPaths.Flv)]
        [InlineData(AppPaths.StreamH264)]
        public void GetFrameCount_Valid_ReturnsFrameCount(string source) {
            var Info = SetupInfo();
            var Src = AppPaths.GetInputFile(source);

            var Count = Info.GetFrameCount(Src);

            Assert.True(Count > 0, "Frame count should be a positive number.");
        }

        [Theory]
        [InlineData("invalidfile")]
        public void GetFrameCount_InvalidFile_ReturnsZero(string source) {
            var Info = SetupInfo();
            var Src = AppPaths.GetInputFile(source);

            var Count = Info.GetFrameCount(Src);

            Assert.Equal(0, Count);
        }
    }
}

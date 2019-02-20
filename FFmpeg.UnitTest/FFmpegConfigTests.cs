using System;
using Moq;
using Xunit;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class FFmpegConfigTests {
        protected Mock<IWindowsApiService> api;

        public IFFmpegConfig SetupConfig() {
            api = new Mock<IWindowsApiService>(MockBehavior.Strict);
            api.Setup(x => x.AttachConsole(It.IsAny<uint>())).Returns(false);
            var FileSystem = new FakeFileSystemService();
            return new FFmpegConfig(api.Object, FileSystem);
        }

        //[Theory]
        //[InlineData("")]
        //[InlineData(null)]
        //[InlineData("FFmpeg.exe")]
        //[InlineData(@"dfsdf\\\abc\")]
        //public void FFmpegPathAbsolute(string path) {
        //    config.FFmpegPath = path;
        //    string Result = config.FFmpegPathAbsolute;
        //    if (path != null) {
        //        Assert.StartsWith(@":\", Result.Substring(1));
        //        Assert.EndsWith(path, Result);
        //    } else
        //        Assert.Null(Result);
        //}

        //[Theory]
        //[InlineData("")]
        //[InlineData(null)]
        //[InlineData("Avs2yuv.exe")]
        //[InlineData(@"dfsdf\\\abc\")]
        //public void Avs2yuvPathAbsolute(string path) {
        //    config.Avs2yuvPath = path;
        //    string Result = config.Avs2yuvPathAbsolute;
        //    if (path != null) {
        //        Assert.StartsWith(@":\", Result.Substring(1));
        //        Assert.EndsWith(path, Result);
        //    } else
        //        Assert.Null(Result);
        //}

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SoftKill_Valid_AttachedConsoleCalled(bool handled) {
            var Config = SetupConfig();
            CloseProcessEventArgs CalledArgs = null;
            Config.CloseProcess += (s, e) => {
                CalledArgs = e;
                if (handled)
                    e.Handled = true;
            };
            var Process = Mock.Of<IProcess>(x => x.HasExited == true && x.Id == 1);

            bool Result = Config.SoftKill(Process);

            Assert.True(Result);
            Assert.NotNull(CalledArgs);
            Assert.Equal(Process, CalledArgs.Process);
            // Very AttachConsole was called. The rest of SoftKillWinApp will not be tested.
            api.Verify(x => x.AttachConsole((uint)Process.Id), handled ? Times.Never() : Times.Once());
        }
    }
}

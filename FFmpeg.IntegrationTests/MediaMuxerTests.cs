using DjvuNet.Tests.Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace EmergenceGuardian.FFmpeg.IntegrationTests {
    public class MediaMuxerTests {
        private readonly ITestOutputHelper output;
        private IProcessManagerFactory factory;

        public MediaMuxerTests(ITestOutputHelper output) {
            this.output = output;
        }

        private IMediaMuxer SetupMuxer() {
            factory = new ProcessManagerFactory(Properties.Settings.Default.FFmpegPath);
            return new MediaMuxer(factory);
        }

        private IProcessManagerFFmpeg GetFileInfo(string path) {
            var Info = new MediaInfo(factory);
            return Info.GetFileInfo(path);
        }

        #region Data Sources

        public static IEnumerable<object[]> GenerateMuxeLists_Valid() {
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream(AppPaths.Mpeg4, 2, "h264", FFmpegStreamType.Video),
                },
                ".mp4", 1
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream(AppPaths.Flv, 1, "flv", FFmpegStreamType.Audio)
                },
                ".mkv", 1
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream(AppPaths.StreamAac, 0, "aac", FFmpegStreamType.Audio),
                    new FFmpegStream(AppPaths.StreamH264, 0, "h264", FFmpegStreamType.Video),
                    new FFmpegStream(AppPaths.StreamVp9, 0, "vp9", FFmpegStreamType.Video)
                },
                ".mkv", 3
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream(AppPaths.StreamAac, 0, "aac", FFmpegStreamType.Audio),
                    new FFmpegStream(AppPaths.StreamH264, 0, "h264", FFmpegStreamType.Video),
                    new FFmpegStream(AppPaths.StreamVp9, 0, "vp9", FFmpegStreamType.Video),
                    new FFmpegStream(AppPaths.StreamOpus, 0, "opus", FFmpegStreamType.Audio)
                },
                ".mkv", 4
            };
        }

        public static IEnumerable<object[]> GenerateMuxeLists_Invalid() {
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("invalidfile", 0, "", FFmpegStreamType.Video),
                },
                ".mp4", 1
            };
        }

        public static IEnumerable<object[]> GenerateConcatenate_Valid() {
            yield return new object[] {
                new List<string>() {
                    AppPaths.Part1
                },
                ".mp4"
            };
            yield return new object[] {
                new List<string>() {
                    AppPaths.Part1, AppPaths.Part2, AppPaths.Part3
                },
                ".mp4"
            };
        }

        public static IEnumerable<object[]> GenerateConcatenate_Invalid() {
            yield return new object[] {
                new List<string>() {
                    "invalidfile"
                },
                ".mp4"
            };
        }

        public static IEnumerable<object[]> GenerateTruncate_Valid() {
            yield return new object[] {
                AppPaths.StreamVp9,
                ".webm",
                null,
                TimeSpan.FromSeconds(5)
            };
            yield return new object[] {
                AppPaths.Mpeg4WithAudio,
                ".mp4",
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(3)
            };
            yield return new object[] {
                AppPaths.StreamOpus,
                ".ogg",
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(8)
            };
        }

        public static IEnumerable<object[]> GenerateTruncate_Invalid() {
            yield return new object[] {
                "invalidfile",
                ".webm",
                null,
                TimeSpan.FromSeconds(5)
            };
        }

        #endregion



        [Theory]
        [InlineData(AppPaths.StreamH264, AppPaths.StreamAac, ".mp4", 2)]
        [InlineData(AppPaths.StreamVp9, AppPaths.StreamOpus, ".webm", 2)]
        [InlineData(AppPaths.StreamH264, AppPaths.StreamOpus, ".mkv", 2)]
        [InlineData(AppPaths.Mpeg2, AppPaths.Flv, ".mkv", 2)]
        [InlineData(AppPaths.Flv, AppPaths.StreamOpus, ".mkv", 2)]
        [InlineData(AppPaths.StreamH264, null, ".mp4", 1)]
        [InlineData("", AppPaths.StreamOpus, ".webm", 1)]
        public void Muxe_Simple_Valid_Success(string videoFile, string audioFile, string destExt, int streamCount) {
            string SrcVideo = AppPaths.GetInputFile(videoFile);
            string SrcAudio = AppPaths.GetInputFile(audioFile);
            string Dest = AppPaths.PrepareDestPath("Muxe", videoFile, destExt);
            var Muxer = SetupMuxer();

            var Result = Muxer.Muxe(SrcVideo, SrcAudio, Dest);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(File.Exists(Dest));
            var FileInfo = GetFileInfo(Dest);
            Assert.Equal(streamCount, FileInfo.FileStreams.Count);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateMuxeLists_Valid))]
        public void Muxe_List_Valid_Success(IEnumerable<FFmpegStream> fileStreams, string destExt, int streamCount) {
            string Dest = AppPaths.PrepareDestPath("MuxeList", fileStreams.First().Path, destExt);
            foreach (var item in fileStreams) {
                item.Path = AppPaths.GetInputFile(item.Path);
            }
            var Muxer = SetupMuxer();

            var Result = Muxer.Muxe(fileStreams, Dest);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(File.Exists(Dest));
            var FileInfo = GetFileInfo(Dest);
            Assert.Equal(streamCount, FileInfo.FileStreams.Count);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateMuxeLists_Invalid))]
        public void Muxe_List_Invalid_ReturnsStatusFailed(IEnumerable<FFmpegStream> fileStreams, string destExt, int streamCount) {
            string Dest = AppPaths.PrepareDestPath("MuxeFailed", fileStreams.First().Path, destExt);
            foreach (var item in fileStreams) {
                item.Path = AppPaths.GetInputFile(item.Path);
            }
            var Muxer = SetupMuxer();

            var Result = Muxer.Muxe(fileStreams, Dest);

            Assert.Equal(CompletionStatus.Failed, Result);
        }

        [Theory]
        [InlineData(AppPaths.StreamOpus, ".ogg")]
        [InlineData(AppPaths.Mpeg4WithAudio, ".mkv")]
        [InlineData(AppPaths.Flv, ".mkv")]
        public void ExtractAudio_Valid_Success(string source, string destExt) {
            string Src = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("ExtractAudio", source, destExt);
            var Muxer = SetupMuxer();

            var Result = Muxer.ExtractAudio(Src, Dest);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(File.Exists(Dest));
        }

        [Theory]
        [InlineData(AppPaths.Mpeg2, ".aaa")]
        public void ExtractAudio_WrongExtension_StatusFailed(string source, string destExt) {
            string Src = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("ExtractAudio", source, destExt);
            var Muxer = SetupMuxer();

            var Result = Muxer.ExtractAudio(Src, Dest);

            Assert.Equal(CompletionStatus.Failed, Result);
            Assert.False(File.Exists(Dest));
        }


        [Theory]
        [InlineData(AppPaths.Mpeg2, ".mp4")]
        [InlineData(AppPaths.Mpeg4, ".mp4")]
        [InlineData(AppPaths.Flv, ".mkv")]
        public void ExtractVideo_Valid_Success(string source, string destExt) {
            string Src = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("ExtractVideo", source, destExt);
            var Muxer = SetupMuxer();

            var Result = Muxer.ExtractVideo(Src, Dest);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(File.Exists(Dest));
        }

        [Theory]
        [InlineData(AppPaths.Mpeg4, ".bbb")]
        public void ExtractVideo_WrongExtension_StatusFailed(string source, string destExt) {
            string Src = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("ExtractVideo", source, destExt);
            var Muxer = SetupMuxer();

            var Result = Muxer.ExtractVideo(Src, Dest);

            Assert.Equal(CompletionStatus.Failed, Result);
            Assert.False(File.Exists(Dest));
        }

        [Theory]
        [MemberData(nameof(GenerateConcatenate_Valid))]
        public void Concatenate_Valid_Success(IEnumerable<string> source, string destExt) {
            List<string> Src = source.Select(x => AppPaths.GetInputFile(x)).ToList();
            string Dest = AppPaths.PrepareDestPath("Concatenate", source.First(), destExt);
            var Muxer = SetupMuxer();

            var Result = Muxer.Concatenate(Src, Dest);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(File.Exists(Dest));
        }

        [Theory]
        [MemberData(nameof(GenerateConcatenate_Invalid))]
        public void Concatenate_Invalid_StatusFailed(IEnumerable<string> source, string destExt) {
            List<string> Src = source.Select(x => AppPaths.GetInputFile(x)).ToList();
            string Dest = AppPaths.PrepareDestPath("Concatenate", source.First(), destExt);
            var Muxer = SetupMuxer();

            var Result = Muxer.Concatenate(Src, Dest);

            Assert.Equal(CompletionStatus.Failed, Result);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateTruncate_Valid))]
        public void Truncate_Valid_Success(string source, string destExt, TimeSpan? startPos, TimeSpan? duration) {
            string Src = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("Truncate", source, destExt);
            var Muxer = SetupMuxer();
            IProcessManager Manager = null;
            ProcessStartedEventHandler Started = (s, e) => Manager = e.ProcessManager;

            var Result = Muxer.Truncate(Src, Dest, startPos, duration, null, Started);

            output.WriteLine(Manager.CommandWithArgs);
            output.WriteLine(Manager.Output);
            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(File.Exists(Dest));
            var FileInfo = GetFileInfo(Dest);
            if (duration.HasValue)
                Assert.True(Math.Abs((duration.Value - FileInfo.FileDuration).TotalSeconds) < .1, "Truncate did not produce expected file duration.");
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateTruncate_Invalid))]
        public void Truncate_Invalid_StatusFailed(string source, string destExt, TimeSpan? startPos, TimeSpan? duration) {
            string Src = AppPaths.GetInputFile(source);
            string Dest = AppPaths.PrepareDestPath("Truncate", source, destExt);
            var Muxer = SetupMuxer();

            var Result = Muxer.Truncate(Src, Dest, startPos, duration);

            Assert.Equal(CompletionStatus.Failed, Result);
        }
    }
}

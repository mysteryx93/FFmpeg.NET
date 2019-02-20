using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using DjvuNet.Tests.Xunit;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class MediaMuxerTests {

        #region Declarations

        private const string FFMPEG = "ffmpeg.exe", AUDIOCODEC = "-acodec", VIDEOCODEC = "-vcodec", FIXAAC = "aac_adtstoasc", FIXPCM = "pcm_s16le";
        private FakeProcessManagerFactory factory = new FakeProcessManagerFactory();
        private readonly ITestOutputHelper output;

        public MediaMuxerTests(ITestOutputHelper output) {
            this.output = output;
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Creates and initializes the MediaMuxer class for testing.
        /// </summary>
        protected IMediaMuxer SetupMuxer() {
            FakeFileSystemService FileSystemStub = new FakeFileSystemService();
            return new MediaMuxer(factory, FileSystemStub);
        }

        /// <summary>
        /// Tests an IFFmpegManager to make sure it contains valid arguments.
        /// </summary>
        /// <param name="hasVideo">Whether the command should include a video source.</param>
        /// <param name="hasAudio">Whether the command should include an audio source.</param>
        /// <param name="instanceIndex">The index of the instance created by the IFFmpegProcessManagerFactory to test, or -1 (default) to use the last.</param>
        private IProcessManager AssertFFmpegManager(bool hasVideo, bool hasAudio, int instanceIndex = -1) {
            var Manager = instanceIndex < 0 ? factory.Instances.Last() : factory.Instances[instanceIndex];
            Assert.NotNull(Manager);
            output.WriteLine(Manager.CommandWithArgs);
            Assert.Contains(FFMPEG, Manager.CommandWithArgs);
            Manager.CommandWithArgs.ContainsOrNot(VIDEOCODEC, hasVideo);
            Manager.CommandWithArgs.ContainsOrNot(AUDIOCODEC, hasAudio);
            return Manager;
        }

        #endregion

        #region Data Sources

        public static IEnumerable<object[]> GenerateStreamLists_Valid() {
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video", 0, "mpeg2", FFmpegStreamType.Video),
                },
                "dest", true, false
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("audio", 1, "mp3", FFmpegStreamType.Audio)
                },
                "dest", false, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video", 0, "mpeg2", FFmpegStreamType.Video),
                    new FFmpegStream("audio", 1, "mp3", FFmpegStreamType.Audio)
                },
                "dest", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video.264", 0, null, FFmpegStreamType.Video),
                    new FFmpegStream("audio.m4a", 1, "m4a", FFmpegStreamType.Audio)
                },
                "dest.mp4", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("Audio.AAC", 1, "aac", FFmpegStreamType.Audio),
                    new FFmpegStream("Video.265", 0, "mpeg2", FFmpegStreamType.Video),
                    new FFmpegStream("audio.m4a", 1, "m4a", FFmpegStreamType.Audio)
                },
                "Dest.MP4", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("Video.MKV", 0, "mpeg2", FFmpegStreamType.Video),
                    new FFmpegStream("Audio.AAC", 1, "aac", FFmpegStreamType.Audio),
                    new FFmpegStream("audio.mp4", 1, "", FFmpegStreamType.Video)
                },
                "Dest.MKV", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("\0\"\0\"\0\n", -5, "\0\0\0\n", FFmpegStreamType.Video),
                    new FFmpegStream("读写汉字", -10, "读写汉字", FFmpegStreamType.None),
                    new FFmpegStream("读写汉字", -15, null, FFmpegStreamType.Video),
                },
                "学中文", true, false
            };
        }

        public static IEnumerable<object[]> GenerateStreamLists_EmptyArgs() {
            yield return new object[] {
                null,
                "dest"
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                },
                "dest"
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video", 0, "", FFmpegStreamType.None)
                },
                "dest"
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video", 0, "", FFmpegStreamType.Video)
                },
                null
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video", 0, "", FFmpegStreamType.Video)
                },
                ""
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream(null, 0, "", FFmpegStreamType.Video)
                },
                "dest"
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("", 0, "", FFmpegStreamType.Video)
                },
                "dest"
            };
        }

        public static IEnumerable<object[]> GenerateStreamLists_FixAac() {
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("audio.aac", 0, "aac", FFmpegStreamType.Audio),
                    new FFmpegStream("video.mp4", 0, "mp4", FFmpegStreamType.Video),
                },
                "dest", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video.Mp4", 0, "mp4", FFmpegStreamType.Video),
                    new FFmpegStream(".AAC", 0, "aac", FFmpegStreamType.Audio),
                },
                "dest", true, true
            };
        }

        public static IEnumerable<object[]> GenerateStreamLists_FixPcm() {
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("audio.wav", 0, "pcm_dvd", FFmpegStreamType.Audio),
                    new FFmpegStream("video.WAV", 0, null, FFmpegStreamType.Video),
                },
                "dest", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video.Mp4", 0, "mp4", FFmpegStreamType.Audio),
                    new FFmpegStream(".AAC", 0, "pcm_dvd", FFmpegStreamType.Audio),
                },
                "dest", false, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream(".AAC", 0, "pcm_dvd", FFmpegStreamType.Audio)
                },
                "dest", false, true
            };
        }

        public static IEnumerable<object[]> GenerateStreamLists_DoNotFixAac() {
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("audio.aac", 0, "aac", FFmpegStreamType.Video),
                    new FFmpegStream("video.mp4", 0, "mp4", FFmpegStreamType.Audio),
                },
                "dest", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("Audio.aac", 0, "aac", FFmpegStreamType.Audio),
                    new FFmpegStream(".AAC", 0, "aac", FFmpegStreamType.Audio),
                },
                "dest", false, true
            };
        }

        public static IEnumerable<object[]> GenerateStreamLists_DoNotFixPcm() {
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("audio.wav", 0, "pcm_dvd", FFmpegStreamType.Video),
                    new FFmpegStream("video.WAV", 0, null, FFmpegStreamType.Audio),
                },
                "dest", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video.aac", 0, "aac", FFmpegStreamType.Audio),
                    new FFmpegStream(".WAV", 0, "pcm_dvd", FFmpegStreamType.None),
                },
                "dest", false, true
            };
        }

        public static IEnumerable<object[]> GenerateStreamLists_SingleValid() {
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video", 0, "mpeg2", FFmpegStreamType.Video),
                    new FFmpegStream("audio", 1, "mp3", FFmpegStreamType.Audio)
                },
                "dest"
            };
        }

        public static IEnumerable<object[]> GenerateStreamLists_H264IntoMkv() {
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("video.264", 0, "h264", FFmpegStreamType.Video),
                    new FFmpegStream("audio.aac", 1, "aac", FFmpegStreamType.Audio)
                },
                "dest.mkv", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream("audio.aac", 1, "aac", FFmpegStreamType.Audio),
                    new FFmpegStream("video.265", 0, "h265", FFmpegStreamType.Video)
                },
                "dest.mkv", true, true
            };
            yield return new object[] {
                new List<FFmpegStream>() {
                    new FFmpegStream(".264", 1, "h264", FFmpegStreamType.Video)
                },
                ".mkv", true, false
            };
        }

        public static IEnumerable<object[]> GenerateConcatenate_Valid() {
            yield return new object[] {
                new List<string>() {
                    "file1"
                },
                "dest.mkv"
            };
            yield return new object[] {
                new List<string>() {
                    "file1",
                    "file2"
                },
                "dest.mkv"
            };
            yield return new object[] {
                new List<string>() {
                    "file1",
                    "file2",
                    "file3"
                },
                "dest.mkv"
            };
        }

        public static IEnumerable<object[]> GenerateConcatenate_Empty() {
            yield return new object[] {
                null,
                "dest.mkv"
            };
            yield return new object[] {
                new List<string>() {
                },
                "dest.mkv"
            };
            yield return new object[] {
                new List<string>() {
                    "file1"
                },
                null
            };
            yield return new object[] {
                new List<string>() {
                    "file1"
                },
                ""
            };
        }

        public static IEnumerable<object[]> GenerateConcatenate_Single() {
            yield return new object[] {
                new List<string>() {
                    "file1",
                    "file2"
                },
                "dest.mkv"
            };
        }


        public static IEnumerable<object[]> GenerateTruncate_Valid() {
            yield return new object[] {
                "source",
                "dest.mkv",
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10)
            };
        }

        public static IEnumerable<object[]> GenerateTruncate_Empty() {
            yield return new object[] {
                null,
                "dest.mkv",
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10)
            };
            yield return new object[] {
                "",
                "dest.mkv",
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10)
            };
            yield return new object[] {
                "source",
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10)
            };
            yield return new object[] {
                "source",
                "",
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10)
            };
        }

        public static IEnumerable<object[]> GenerateTruncate_Single() {
            yield return new object[] {
                "source",
                "dest.mkv",
                TimeSpan.Zero,
                TimeSpan.FromSeconds(10)
            };
        }

        #endregion

        #region Constructors

        [Fact]
        public void Constructor_Empty_Success() => new MediaMuxer();

        [Fact]
        public void Constructor_WithFactory_Success() => new MediaMuxer(factory);

        [Fact]
        public void Constructor_NullFactory_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaMuxer(null));

        [Fact]
        public void Constructor_NullDependency_ThrowsException() => Assert.Throws<ArgumentNullException>(() => new MediaMuxer(factory, null));

        #endregion

        #region Muxe_StreamList

        // DjvuTheory is required to serialize FFmpegStream argument, otherwise we cannot see the list of theory cases.
        [DjvuTheory]
        [MemberData(nameof(GenerateStreamLists_Valid))]
        public void Muxe_StreamList_Valid_Success(IEnumerable<FFmpegStream> fileStreams, string destination, bool hasVideo, bool hasAudio) {
            var Muxer = SetupMuxer();

            var Result = Muxer.Muxe(fileStreams, destination);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Single(factory.Instances);
            AssertFFmpegManager(hasVideo, hasAudio);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateStreamLists_EmptyArgs))]
        public void Muxe_StreamList_EmptyArgs_ThrowsNullException(IEnumerable<FFmpegStream> fileStreams, string destination) {
            var Muxer = SetupMuxer();

            Assert.Throws<ArgumentException>(() => Muxer.Muxe(fileStreams, destination));
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateStreamLists_FixAac))]
        public void Muxe_StreamList_WithAudioAac_AddArgumentFixAac(IEnumerable<FFmpegStream> fileStreams, string destination, bool hasVideo, bool hasAudio) {
            Muxe_StreamList_FixAudio(fileStreams, destination, FIXAAC, hasVideo, hasAudio);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateStreamLists_FixPcm))]
        public void Muxe_StreamList_WithAudioPcm_AddArgumentFixPcm(IEnumerable<FFmpegStream> fileStreams, string destination, bool hasVideo, bool hasAudio) {
            Muxe_StreamList_FixAudio(fileStreams, destination, FIXPCM, hasVideo, hasAudio);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateStreamLists_DoNotFixAac))]
        public void Muxe_StreamList_WithNoAudioAac_DoNotAddArgumentFixAac(IEnumerable<FFmpegStream> fileStreams, string destination, bool hasVideo, bool hasAudio) {
            Muxe_StreamList_FixAudio(fileStreams, destination, FIXAAC, hasVideo, hasAudio, false);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateStreamLists_DoNotFixPcm))]
        public void Muxe_StreamList_WithNoAudioPcm_DoNotAddArgumentFixPcm(IEnumerable<FFmpegStream> fileStreams, string destination, bool hasVideo, bool hasAudio) {
            Muxe_StreamList_FixAudio(fileStreams, destination, FIXPCM, hasVideo, hasAudio, false);
        }

        private void Muxe_StreamList_FixAudio(IEnumerable<FFmpegStream> fileStreams, string destination, string fixString, bool hasVideo, bool hasAudio, bool fixStringExpected = true) {
            var Muxer = SetupMuxer();

            var Result = Muxer.Muxe(fileStreams, destination);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Single(factory.Instances);
            var Manager = AssertFFmpegManager(hasVideo, hasAudio);
            Manager.CommandWithArgs.ContainsOrNot(fixString, fixStringExpected);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateStreamLists_H264IntoMkv))]
        public void Muxe_StreamsList_H264IntoMkv_MuxeIntoMp4First(IEnumerable<FFmpegStream> fileStreams, string destination, bool hasVideo, bool hasAudio) {
            var Muxer = SetupMuxer();

            var Result = Muxer.Muxe(fileStreams, destination, null);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.True(factory.Instances.Count > 1);
            foreach (IProcessManager manager in factory.Instances) {
                output.WriteLine(manager.CommandWithArgs);
            }
            AssertFFmpegManager(hasVideo, hasAudio);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateStreamLists_SingleValid))]
        public void Muxe_StreamsList_ParamOptions_ReturnsSame(IEnumerable<FFmpegStream> fileStreams, string destination) {
            var Muxer = SetupMuxer();
            var Options = new ProcessOptionsFFmpeg();

            var Result = Muxer.Muxe(fileStreams, destination, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [DjvuTheory]
        [MemberData(nameof(GenerateStreamLists_SingleValid))]
        public void Muxe_StreamsList_ParamCallback_CallbackCalled(IEnumerable<FFmpegStream> fileStreams, string destination) {
            var Muxer = SetupMuxer();
            int CallbackCalled = 0;

            var Result = Muxer.Muxe(fileStreams, destination, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region Muxe_Simple

        [Theory]
        [InlineData("video", "audio", "dest")]
        [InlineData("video.mp4", "audio.m4a", "dest.mp4")]
        public void Muxe_Simple_AudioVideo_Success(string videoFile, string audioFile, string destination) {
            var Muxer = SetupMuxer();

            var Result = Muxer.Muxe(videoFile, audioFile, destination);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Equal(3, factory.Instances.Count);
            AssertFFmpegManager(true, true);
        }

        [Theory]
        [InlineData("audio", "dest")]
        [InlineData("audio.m4a", "dest.mp4")]
        [InlineData("audio.AAC", "Dest.MKV")]
        [InlineData("\0\"\0\"\0\n", "\0\0\0\n")]
        [InlineData("读写汉字", "学中文")]
        public void Muxe_Simple_AudioOnly_Success(string audioFile, string destination) {
            var Muxer = SetupMuxer();

            var Result = Muxer.Muxe(null, audioFile, destination);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Equal(2, factory.Instances.Count);
            AssertFFmpegManager(false, true);
        }

        [Theory]
        [InlineData("video", "dest")]
        [InlineData("Video.aac", "dest.mp4")]
        [InlineData("Video.MKV", "Dest.MKV")]
        [InlineData("\0\"\0\"\0\n", "\0\0\0\n")]
        [InlineData("读写汉字", "学中文")]
        public void Muxe_Simple_VideoOnly_Success(string videoFile, string destination) {
            var Muxer = SetupMuxer();

            var Result = Muxer.Muxe(videoFile, null, destination);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Equal(2, factory.Instances.Count);
            AssertFFmpegManager(true, false);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData("video.mp4", "", "")]
        [InlineData("", "audio.aac", "")]
        [InlineData("", "", "dest.mp4")]
        [InlineData("video.mp4", "audio.aac", null)]
        public void Muxe_Simple_EmptyArgs_ThrowsException(string videoFile, string audioFile, string destination) {
            var Muxer = SetupMuxer();

            Assert.Throws<ArgumentException>(() => Muxer.Muxe(videoFile, audioFile, destination));
        }

        [Theory]
        [InlineData("video", "audio", "dest")]
        public void Muxe_Simple_ParamOptions_ReturnsSame(string videoFile, string audioFile, string destination) {
            var Muxer = SetupMuxer();
            var Options = new ProcessOptionsFFmpeg();

            var Result = Muxer.Muxe(videoFile, audioFile, destination, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Theory]
        [InlineData("video", "audio", "dest")]
        public void Muxe_Simple_ParamCallback_CallbackCalled(string videoFile, string audioFile, string destination) {
            var Muxer = SetupMuxer();
            int CallbackCalled = 0;

            var Result = Muxer.Muxe(videoFile, audioFile, destination, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region ExtractAudio

        [Theory]
        [InlineData("source", "dest")]
        public void ExtractAudio_Valid_Success(string source, string destination) {
            var Muxer = SetupMuxer();

            var Result = Muxer.ExtractAudio(source, destination);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Single(factory.Instances);
            AssertFFmpegManager(false, true);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("source", null)]
        [InlineData("", "dest")]
        public void ExtractAudio_EmptyArgs_ThrowsException(string source, string destination) {
            var Muxer = SetupMuxer();

            Assert.Throws<ArgumentException>(() => Muxer.ExtractAudio(source, destination));
        }

        [Theory]
        [InlineData("source", "dest")]
        public void ExtractAudio_ParamOptions_ReturnsSame(string source, string destination) {
            var Muxer = SetupMuxer();
            var Options = new ProcessOptionsFFmpeg();

            var Result = Muxer.ExtractAudio(source, destination, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Theory]
        [InlineData("source", "dest")]
        public void ExtractAudio_ParamCallback_CallbackCalled(string source, string destination) {
            var Muxer = SetupMuxer();
            int CallbackCalled = 0;

            var Result = Muxer.ExtractAudio(source, destination, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region ExtractVideo

        [Theory]
        [InlineData("source", "dest")]
        public void ExtractVideo_Valid_Success(string source, string destination) {
            var Muxer = SetupMuxer();

            var Result = Muxer.ExtractVideo(source, destination);

            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Single(factory.Instances);
            AssertFFmpegManager(true, false);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("source", null)]
        [InlineData("", "dest")]
        public void ExtractVideo_EmptyArgs_ThrowsException(string source, string destination) {
            var Muxer = SetupMuxer();

            Assert.Throws<ArgumentException>(() => Muxer.ExtractVideo(source, destination));
        }

        [Theory]
        [InlineData("source", "dest")]
        public void ExtractVideo_ParamOptions_ReturnsSame(string source, string destination) {
            var Muxer = SetupMuxer();
            var Options = new ProcessOptionsFFmpeg();

            var Result = Muxer.ExtractVideo(source, destination, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Theory]
        [InlineData("source", "dest")]
        public void ExtractVideo_ParamCallback_CallbackCalled(string source, string destination) {
            var Muxer = SetupMuxer();
            int CallbackCalled = 0;

            var Result = Muxer.ExtractVideo(source, destination, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region Concatenate

        [Theory]
        [MemberData(nameof(GenerateConcatenate_Valid))]
        public void Concatenate_Valid_Success(IEnumerable<string> files, string destination) {
            var Muxer = SetupMuxer();

            var Result = Muxer.Concatenate(files, destination);

            output.WriteLine(factory.Instances.FirstOrDefault()?.CommandWithArgs);
            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Single(factory.Instances);
        }

        [Theory]
        [MemberData(nameof(GenerateConcatenate_Empty))]
        public void Concatenate_EmptyArgs_ThrowsException(IEnumerable<string> files, string destination) {
            var Muxer = SetupMuxer();

            Assert.Throws<ArgumentException>(() => Muxer.Concatenate(files, destination));
        }

        [Theory]
        [MemberData(nameof(GenerateConcatenate_Single))]
        public void Concatenate_ParamOptions_ReturnsSame(IEnumerable<string> files, string destination) {
            var Muxer = SetupMuxer();
            var Options = new ProcessOptionsFFmpeg();

            var Result = Muxer.Concatenate(files, destination, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Theory]
        [MemberData(nameof(GenerateConcatenate_Single))]
        public void Concatenate_ParamCallback_CallbackCalled(IEnumerable<string> files, string destination) {
            var Muxer = SetupMuxer();
            int CallbackCalled = 0;

            var Result = Muxer.Concatenate(files, destination, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

        #region Truncate

        [Theory]
        [MemberData(nameof(GenerateTruncate_Valid))]
        public void Truncate_Valid_Success(string source, string destination, TimeSpan? startPos, TimeSpan? duration) {
            var Muxer = SetupMuxer();

            var Result = Muxer.Truncate(source, destination, startPos, duration);

            output.WriteLine(factory.Instances.FirstOrDefault()?.CommandWithArgs);
            Assert.Equal(CompletionStatus.Success, Result);
            Assert.Single(factory.Instances);
        }

        [Theory]
        [MemberData(nameof(GenerateTruncate_Empty))]
        public void Truncate_EmptyArgs_ThrowsException(string source, string destination, TimeSpan? startPos, TimeSpan? duration) {
            var Muxer = SetupMuxer();

            Assert.Throws<ArgumentException>(() => Muxer.Truncate(source, destination, startPos, duration));
        }

        [Theory]
        [MemberData(nameof(GenerateTruncate_Single))]
        public void Truncate_ParamOptions_ReturnsSame(string source, string destination, TimeSpan? startPos, TimeSpan? duration) {
            var Muxer = SetupMuxer();
            var Options = new ProcessOptionsFFmpeg();

            var Result = Muxer.Truncate(source, destination, startPos, duration, Options);

            Assert.Same(Options, factory.Instances[0].Options);
        }

        [Theory]
        [MemberData(nameof(GenerateTruncate_Single))]
        public void Truncate_ParamCallback_CallbackCalled(string source, string destination, TimeSpan? startPos, TimeSpan? duration) {
            var Muxer = SetupMuxer();
            int CallbackCalled = 0;

            var Result = Muxer.Truncate(source, destination, startPos, duration, null, (s, e) => CallbackCalled++);

            Assert.Equal(1, CallbackCalled);
        }

        #endregion

    }
}

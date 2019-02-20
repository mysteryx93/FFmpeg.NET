using System;
using Xunit;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public class FFmpegParserTests {
        protected IFFmpegParser SetupParser() => new FFmpegParser();

        [Theory]
        [InlineData("", 0)]
        [InlineData(null, 0)]
        [InlineData("This is some invalid data: Stream #0", 0)]
        [InlineData(OutputSamples.FFmpegInfo1, 1)]
        [InlineData(OutputSamples.FFmpegInfo2, 2)]
        [InlineData(OutputSamples.FFmpegEncode1, 2)]
        [InlineData(@"  Duration: 00:00:44.00, start: 0.373378, bitrate: 1402 kb/s
    Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1150 kb/s, 25 fps, 25 tbr, 90k tbn, 25 tbc
   aStream #0:1[0x1c0]: Audio: mp2, 44100 Hz, stereo, s16p, 224 kb/s
", 1)]
        public void ParseFileInfo_Any_ReturnsExpectedStreamCount(string outputText, int streamCount) {
            var Parser = SetupParser();

            var Result = Parser.ParseFileInfo(outputText, out TimeSpan FileDuration);

            Assert.NotNull(Result);
            Assert.Equal(streamCount, Result.Count);
        }

        [Theory]
        [InlineData("    Stream #0:1[0x1c0]: Audio: mp2, 44100 Hz, stereo, s16p, 224 kb/s", 1, "mp2", 44100, "stereo", "s16p", 224)]
        [InlineData("    Stream #0:0: Audio: mp3, 44100 Hz, stereo, s16p, 192 kb/s", 0, "mp3", 44100, "stereo", "s16p", 192)]
        [InlineData("    Stream #0:1(und): Audio: aac (LC) (mp4a / 0x6134706D), 44100 Hz, stereo, fltp, 132 kb/s (default)", 1, "aac", 44100, "stereo", "fltp", 132)]
        public void ParseAudioStreamInfo_Valid_ReturnsExpectedData(string text, int index, string format, int sampleRate, string channels, string bitDepth, int bitrate) {
            var Parser = SetupParser();

            var Result = Parser.ParseStreamInfo(text);

            var Info = Result as FFmpegAudioStreamInfo;
            Assert.NotNull(Info);
            Assert.Equal(text, Info.RawText);
            Assert.Equal(FFmpegStreamType.Audio, Info.StreamType);
            Assert.Equal(index, Info.Index);
            Assert.Equal(format, Info.Format);
            Assert.Equal(sampleRate, Info.SampleRate);
            Assert.Equal(channels, Info.Channels);
            Assert.Equal(bitDepth, Info.BitDepth);
            Assert.Equal(bitrate, Info.Bitrate);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("This is invalid data")]
        [InlineData("   Stream #0:0: Audio: mp3, 44100 Hz, stereo, s16p, 192 kb/s")]
        public void ParseAudioStreamInfo_Invalid_ReturnsNull(string text) {
            var Parser = SetupParser();

            var Result = Parser.ParseStreamInfo(text);

            Assert.Null(Result);
        }

        [Theory]
        [InlineData("    Stream #0:1: Video: this, , , is; invalid data", 1, "this", "", "", "", 0, 0, 1, 1, 1, 1, 0, 8, 0)]
        [InlineData("    Stream #0:0[0x1e0]: Video: mpeg1video, yuv420p(tv), 352x288 [SAR 178:163 DAR 1958:1467], 1150 kb/s, 25 fps, 25 tbr, 90k tbn, 25 tbc", 0, "mpeg1video", "yuv420p", "tv", "", 352, 288, 178, 163, 1958, 1467, 25, 8, 1150)]
        [InlineData("    Stream #0:1: Video: mjpeg, yuvj420p(pc, bt470bg/unknown/unknown), 1000x1000 [SAR 1:1 DAR 1:1], 90k tbr, 90k tbn, 90k tbc", 1, "mjpeg", "yuvj420p", "pc", "bt470bg/unknown/unknown", 1000, 1000, 1, 1, 1, 1, 0, 8, 0)]
        [InlineData("    Stream #0:0(und): Video: h264 (High) (avc1 / 0x31637661), yuv420p, 352x288 [SAR 178:163 DAR 1958:1467], 228 kb/s, 25 fps, 25 tbr, 12800 tbn, 50 tbc (default)", 0, "h264", "yuv420p", "", "", 352, 288, 178, 163, 1958, 1467, 25, 8, 228)]
        public void ParseVideoStreamInfo_Valid_ReturnsExpectedData(string text, int index, string format, string colorSpace, string colorRange, string colorMatrix, int width, int height, int sar1, int sar2, int dar1, int dar2, double frameRate, int bitDepth, int bitrate) {
            var Parser = SetupParser();

            var Result = Parser.ParseStreamInfo(text);

            var Info = Result as FFmpegVideoStreamInfo;
            Assert.NotNull(Info);
            Assert.Equal(text, Info.RawText);
            Assert.Equal(FFmpegStreamType.Video, Info.StreamType);
            Assert.Equal(index, Info.Index);
            Assert.Equal(format, Info.Format);
            Assert.Equal(colorSpace, Info.ColorSpace);
            Assert.Equal(colorRange, Info.ColorRange);
            Assert.Equal(colorMatrix, Info.ColorMatrix);
            Assert.Equal(width, Info.Width);
            Assert.Equal(height, Info.Height);
            Assert.Equal(sar1, Info.SAR1);
            Assert.Equal(sar2, Info.SAR2);
            Assert.Equal(dar1, Info.DAR1);
            Assert.Equal(dar2, Info.DAR2);
            Assert.Equal(frameRate, Info.FrameRate);
            Assert.Equal(bitDepth, Info.BitDepth);
            Assert.Equal(bitrate, Info.Bitrate);
        }

        [Theory]
        [InlineData("", 0, 0, 0, "", 0, "", 0)]
        [InlineData(null, 0, 0, 0, "", 0, "", 0)]
        [InlineData("This is invalid data.", 0, 0, 0, "", 0, "", 0)]
        [InlineData("frame=  929 fps=0.0 q=-0.0 size=   68483kB time=00:00:37.00 bitrate=15162.6kbits/s speed=  74x    ", 929, 0, 0, "68483kB", 37, "15162.6kbits/s", 74)]
        public void ParseFFmpegProgress_Any_ReturnsExpectedData(string text, long frame, float fps, float quantizer, string size, double seconds, string bitrate, float speed) {
            var Parser = SetupParser();

            var Result = Parser.ParseFFmpegProgress(text);

            Assert.NotNull(Result);
            Assert.Equal(frame, Result.Frame);
            Assert.Equal(fps, Result.Fps);
            Assert.Equal(quantizer, Result.Quantizer);
            Assert.Equal(size, Result.Size);
            Assert.Equal(TimeSpan.FromSeconds(seconds), Result.Time);
            Assert.Equal(bitrate, Result.Bitrate);
            Assert.Equal(speed, Result.Speed);
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", null)]
        [InlineData(" ", "     ", null)]
        [InlineData("   =   ", " ", "")]
        [InlineData("mode=1 key=MyKey key=value2 ", "key", "MyKey")]
        [InlineData("mode=1 key2=MyKey key=value2", "key", "value2")]
        [InlineData("mode=1 key2=MyKey key=value2   ", "key", "value2")]
        [InlineData("mode=1 key2=MyKey key3=value2", "key", null)]
        public void ParseAttribute_Any_ReturnsExpectedValue(string text, string key, string expected) {
            var Parser = SetupParser();

            var Result = Parser.ParseAttribute(text, key);

            Assert.Equal(expected, Result);
        }

        [Theory]
        [InlineData("", 0, 0, "", "")]
        [InlineData(null, 0, 0, "", "")]
        [InlineData("     12345 Invalid Dataaaaaaaaaaaaaaaaaaaa      ", 1, 0, "", "")]
        [InlineData("     1   0.10  10985.28    0:00:10    22.35 KB  ", 1, 0.1, "10985.28", "22.35 KB")]
        public void ParseX264Progress_Any_ReturnsExpectedData(string text, int frame, float fps, string bitrate, string size) {
            var Parser = SetupParser();

            var Result = Parser.ParseX264Progress(text);

            Assert.NotNull(Result);
            Assert.Equal(frame, Result.Frame);
            Assert.Equal(fps, Result.Fps);
            Assert.Equal(bitrate, Result.Bitrate);
            Assert.Equal(size, Result.Size);
        }
    }
}

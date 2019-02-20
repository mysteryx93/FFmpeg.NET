using System;
using System.IO;

namespace EmergenceGuardian.FFmpeg.IntegrationTests {
    public static class AppPaths {
        public static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
        public static string SampleFilesDirectory => Path.Combine(BaseDirectory, "SampleFiles");
        public static string OutputDirectory => Path.Combine(BaseDirectory, "Output");

        public const string Mpeg2 = "mpeg2_field_encoding.ts";
        public const string Mpeg4 = "MPEG4 by philips.mp4";
        public const string Flv = "zelda.flv";
        public const string Mpeg4WithAudio = "Feline Power Muxed.mp4";
        public const string Part1 = "Part1.mp4";
        public const string Part2 = "Part2.mp4";
        public const string Part3 = "Part3.mp4";
        // These are YouTube streams with no containers.
        public const string StreamAac = "Feline Power.aac";
        public const string StreamH264 = "Feline Power.h264";
        public const string StreamOpus = "Feline Power.opus";
        public const string StreamVp9 = "Feline Power.vp9";

        public static string GetInputFile(string path) => !string.IsNullOrEmpty(path) ? Path.Combine(AppPaths.SampleFilesDirectory, path) : path;

        public static string PrepareDestPath(string prefix, string source, string destExt) {
            string Dest = Path.Combine(AppPaths.OutputDirectory, Path.ChangeExtension(prefix + " " + source, destExt));
            Directory.CreateDirectory(Path.GetDirectoryName(Dest));
            File.Delete(Dest);
            return Dest;
        }
    }
}

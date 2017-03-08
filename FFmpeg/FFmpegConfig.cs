using System;

namespace EmergenceGuardian.FFmpeg {
    public static class FFmpegConfig {
        /// <summary>
        /// Gets or sets the path to FFmpeg.exe
        /// </summary>
        public static string FFmpegPath { get; set; } = "ffmpeg.exe";
        /// <summary>
        /// Gets or sets the path to avs2yuv.exe to use AviSynth in a separate process.
        /// </summary>
        public static string Avs2yuvPath { get; set; } = "avs2yuv.exe";
        /// <summary>
        /// Gets or sets a class that will manage graphical interface instances when DisplayMode = Interface
        /// </summary>
        public static IUserInterfaceManager UserInterfaceManager { get; set; }
        /// <summary>
        /// Gets or sets the options for starting new processes.
        /// </summary>
    }
}

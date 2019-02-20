using System;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Helper class providing static access to FFmpeg components. You can use this if not using dependency injection design pattern.
    /// </summary>
    public static class FFmpegInit {
        /// <summary>
        /// Returns a ProcessFactory allowing creating new instances of FFmpegProcess.
        /// </summary>
        public static IProcessManagerFactory ProcessFactory => processFactory ?? (processFactory = new ProcessManagerFactory());
        private static IProcessManagerFactory processFactory;

        /// <summary>
        /// Returns the FFmpeg configuration settings.
        /// </summary>
        public static IFFmpegConfig Config => ProcessFactory.Config;

        /// <summary>
        /// Returns a MediaInfo instance providing functions to get information on media files.
        /// </summary>
        public static IMediaInfo MediaInfo => mediaInfo ?? (mediaInfo = new MediaInfo(ProcessFactory));
        private static IMediaInfo mediaInfo;

        /// <summary>
        /// Returns a MediaEncoder instance providing functions to encode media files.
        /// </summary>
        public static IMediaEncoder MediaEncoder => mediaEncoder ?? (mediaEncoder = new MediaEncoder(ProcessFactory));
        private static IMediaEncoder mediaEncoder;

        /// <summary>
        /// Returns a MediaMuxer instance providing functions to manage audio and video streams.
        /// </summary>
        public static IMediaMuxer MediaMuxer => mediaMuxer ?? (mediaMuxer = new MediaMuxer(ProcessFactory));
        private static IMediaMuxer mediaMuxer;

        /// <summary>
        /// Releases all static instances.
        /// </summary>
        public static void Clear() {
            mediaInfo = null;
            mediaEncoder = null;
            mediaMuxer = null;
            processFactory = null;
        }
    }
}

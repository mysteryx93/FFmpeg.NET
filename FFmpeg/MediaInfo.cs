using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Provides functions to get information on media files.
    /// </summary>
    public static class MediaInfo {
        /// <summary>
        /// Gets file streams information of specified file via FFmpeg.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <returns>A FFmpegProcess object containing the file information.</returns>
        public static FFmpegProcess GetFileInfo(string source) {
            return GetFileInfo(source, null);
        }

        /// <summary>
        /// Gets file streams information of specified file via FFmpeg.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>A FFmpegProcess object containing the file information.</returns>
        public static FFmpegProcess GetFileInfo(string source, ProcessStartOptions options) {
            FFmpegProcess Worker = new FFmpeg.FFmpegProcess(options);
            Worker.RunFFmpeg(string.Format(@"-i ""{0}""", source));
            return Worker;
        }

        /// <summary>
        /// Returns the exact frame count of specified video file.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <returns>The number of frames in the video.</returns>
        public static long GetFrameCount(string source) {
            return GetFrameCount(source, null);
        }
        
        /// <summary>
        /// Returns the exact frame count of specified video file.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The number of frames in the video.</returns>
        public static long GetFrameCount(string source, ProcessStartOptions options) {
            long Result = 0;
            FFmpegProcess Worker = new FFmpeg.FFmpegProcess(options);
            Worker.StatusUpdated += (sender, e) => {
                Result = e.Status.Frame;
            };
            Worker.RunFFmpeg(string.Format(@"-i ""{0}"" -f null /dev/null", source));
            return Result;
        }
    }
}

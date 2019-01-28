using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace EmergenceGuardian.FFmpeg {

    #region Interface

    /// <summary>
    /// Provides functions to get information on media files.
    /// </summary>
    public interface IMediaInfo {
        /// <summary>
        /// Returns the version information from FFmpeg.
        /// </summary>
        /// <returns>A IFFmpegProcess object containing the version information.</returns>
        IFFmpegProcess GetVersion();
        /// <summary>
        /// Returns the version information from FFmpeg.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>A IFFmpegProcess object containing the version information.</returns>
        IFFmpegProcess GetVersion(ProcessStartOptions options);
        /// <summary>
        /// Gets file streams information of specified file via FFmpeg.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <returns>A IFFmpegProcess object containing the file information.</returns>
        IFFmpegProcess GetFileInfo(string source);
        /// <summary>
        /// Gets file streams information of specified file via FFmpeg.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>A IFFmpegProcess object containing the file information.</returns>
        IFFmpegProcess GetFileInfo(string source, ProcessStartOptions options);
        /// <summary>
        /// Returns the exact frame count of specified video file.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <returns>The number of frames in the video.</returns>
        long GetFrameCount(string source);
        /// <summary>
        /// Returns the exact frame count of specified video file.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The number of frames in the video.</returns>
        long GetFrameCount(string source, ProcessStartOptions options);
    }

    #endregion

    /// <summary>
    /// Provides functions to get information on media files.
    /// </summary>
    public class MediaInfo : IMediaInfo {

        #region Declarations / Constructors

        protected IFFmpegProcessFactory factory;

        public MediaInfo() : this(new FFmpegProcessFactory()) { }

        public MediaInfo(IFFmpegProcessFactory processFactory) {
            this.factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
        }

        #endregion

        /// <summary>
        /// Returns the version information from FFmpeg.
        /// </summary>
        /// <returns>A IFFmpegProcess object containing the version information.</returns>
        public IFFmpegProcess GetVersion() {
            return GetVersion(null);
        }

        /// <summary>
        /// Returns the version information from FFmpeg.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>A IFFmpegProcess object containing the version information.</returns>
        public IFFmpegProcess GetVersion(ProcessStartOptions options) {
            IFFmpegProcess Worker = factory.Create(options);
            Worker.RunFFmpeg("-version", ProcessOutput.Standard);
            return Worker;
        }

        /// <summary>
        /// Gets file streams information of specified file via FFmpeg.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <returns>A IFFmpegProcess object containing the file information.</returns>
        public IFFmpegProcess GetFileInfo(string source) {
            return GetFileInfo(source, null);
        }

        /// <summary>
        /// Gets file streams information of specified file via FFmpeg.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>A IFFmpegProcess object containing the file information.</returns>
        public IFFmpegProcess GetFileInfo(string source, ProcessStartOptions options) {
            IFFmpegProcess Worker = factory.Create(options);
            Worker.RunFFmpeg(string.Format(@"-i ""{0}""", source));
            return Worker;
        }

        /// <summary>
        /// Returns the exact frame count of specified video file.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <returns>The number of frames in the video.</returns>
        public long GetFrameCount(string source) {
            return GetFrameCount(source, null);
        }
        
        /// <summary>
        /// Returns the exact frame count of specified video file.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The number of frames in the video.</returns>
        public long GetFrameCount(string source, ProcessStartOptions options) {
            long Result = 0;
            IFFmpegProcess Worker = factory.Create(options);
            Worker.StatusUpdated += (sender, e) => {
                Result = e.Status.Frame;
            };
            Worker.RunFFmpeg(string.Format(@"-i ""{0}"" -f null /dev/null", source));
            return Result;
        }
    }
}

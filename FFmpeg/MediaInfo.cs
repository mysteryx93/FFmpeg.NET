using System;

namespace EmergenceGuardian.FFmpeg {

    #region Interface

    /// <summary>
    /// Provides functions to get information on media files.
    /// </summary>
    public interface IMediaInfo {
        /// <summary>
        /// Returns the version information from FFmpeg.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>A IFFmpegProcess object containing the version information.</returns>
        IProcessManagerFFmpeg GetVersion(ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Gets file streams information of specified file via FFmpeg.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>A IFFmpegProcess object containing the file information.</returns>
        IProcessManagerFFmpeg GetFileInfo(string source, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Returns the exact frame count of specified video file.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The number of frames in the video.</returns>
        long GetFrameCount(string source, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
    }

    #endregion

    /// <summary>
    /// Provides functions to get information on media files.
    /// </summary>
    public class MediaInfo : IMediaInfo {

        #region Declarations / Constructors

        protected readonly IProcessManagerFactory factory;

        public MediaInfo() : this(new ProcessManagerFactory()) { }

        public MediaInfo(IProcessManagerFactory processFactory) {
            this.factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
        }

        #endregion

        /// <summary>
        /// Returns the version information from FFmpeg.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>A IFFmpegProcess object containing the version information.</returns>
        public IProcessManagerFFmpeg GetVersion(ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            IProcessManagerFFmpeg Worker = factory.CreateFFmpeg(options, callback);
            Worker.OutputType = ProcessOutput.Output;
            Worker.RunFFmpeg("-version");
            return Worker;
        }

        /// <summary>
        /// Gets file streams information of specified file via FFmpeg.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>A IFFmpegProcess object containing the file information.</returns>
        public IProcessManagerFFmpeg GetFileInfo(string source, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be null or empty.", nameof(source));
            IProcessManagerFFmpeg Worker = factory.CreateFFmpeg(options, callback);
            Worker.RunFFmpeg(string.Format(@"-i ""{0}""", source));
            return Worker;
        }

        /// <summary>
        /// Returns the exact frame count of specified video file.
        /// </summary>
        /// <param name="source">The file to get information about.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The number of frames in the video.</returns>
        public long GetFrameCount(string source, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be null or empty.", nameof(source));
            long Result = 0;
            IProcessManagerFFmpeg Worker = factory.CreateFFmpeg(options, callback);
            Worker.StatusUpdated += (sender, e) => {
                Result = e.Status.Frame;
            };
            Worker.RunFFmpeg(string.Format(@"-i ""{0}"" -f null /dev/null", source));
            return Result;
        }
    }
}

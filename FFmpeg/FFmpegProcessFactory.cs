using System;

namespace EmergenceGuardian.FFmpeg {
    
    #region Interface

    /// <summary>
    /// Creates new instances of FFmpeg processes.
    /// </summary>
    public interface IFFmpegProcessFactory {
        /// <summary>
        /// Gets or sets the configuration settings for FFmpeg.
        /// </summary>
        IFFmpegConfig Config { get; set; }
        /// <summary>
        /// Creates a new instance of IFFmpegProcess.
        /// </summary>
        /// <returns>The newly created FFmpeg process.</returns>
        IFFmpegProcess Create();
        /// <summary>
        /// Creates a new instances of IFFmpegProcess with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The newly created FFmpeg process.</returns>
        IFFmpegProcess Create(ProcessStartOptions options);
    }

    #endregion

    /// <summary>
    /// Creates new instances of FFmpeg processes.
    /// </summary>
    public class FFmpegProcessFactory : IFFmpegProcessFactory {

        #region Declarations / Constructors

        /// <summary>
        /// Gets or sets the configuration settings for FFmpeg.
        /// </summary>
        public IFFmpegConfig Config { get; set; }

        public FFmpegProcessFactory() : this(new FFmpegConfig()) { }

        public FFmpegProcessFactory(string ffmpegPath) : this(ffmpegPath, null) { }

        public FFmpegProcessFactory(string ffmpegPath, IUserInterfaceManager userInterfaceManager) : this(new FFmpegConfig(ffmpegPath, userInterfaceManager)) { }

        public FFmpegProcessFactory(IFFmpegConfig config) {
            this.Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #endregion

        /// <summary>
        /// Creates a new instance of IFFmpegProcess.
        /// </summary>
        /// <returns>The newly created FFmpeg process.</returns>
        public IFFmpegProcess Create() {
            return new FFmpegProcess(Config);
        }

        /// <summary>
        /// Creates a new instances of IFFmpegProcess with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The newly created FFmpeg process.</returns>
        public IFFmpegProcess Create(ProcessStartOptions options) {
            return new FFmpegProcess(Config, options);
        }
    }
}

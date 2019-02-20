using System;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg {
    
    #region Interface

    /// <summary>
    /// Creates new instances of FFmpeg processes.
    /// </summary>
    public interface IProcessManagerFactory {
        /// <summary>
        /// Gets or sets the configuration settings for FFmpeg.
        /// </summary>
        IFFmpegConfig Config { get; set; }
        /// <summary>
        /// Creates a new process manager with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created process manager.</returns>
        IProcessManager Create(ProcessOptions options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Creates a new process manager to run ffmpeg with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created FFmpeg process manager.</returns>
        IProcessManagerFFmpeg CreateFFmpeg(ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Creates a new process manager to run avs2yuv with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created avs2yuv process manager.</returns>
        IProcessManagerAvs CreateAvs(ProcessOptions options = null, ProcessStartedEventHandler callback = null);
    }

    #endregion

    /// <summary>
    /// Creates new instances of FFmpeg processes.
    /// </summary>
    public class ProcessManagerFactory : IProcessManagerFactory {

        #region Declarations / Constructors

        /// <summary>
        /// Gets or sets the configuration settings for FFmpeg.
        /// </summary>
        public IFFmpegConfig Config { get; set; }
        public IFFmpegParser Parser { get; set; }
        public IProcessFactory ProcessFactory { get; set; }
        public IFileSystemService FileSystemService { get; set; }

        public ProcessManagerFactory() : this(new FFmpegConfig(), null, null, null) { }

        public ProcessManagerFactory(string ffmpegPath) : this(ffmpegPath, null) { }

        public ProcessManagerFactory(string ffmpegPath, IUserInterfaceManager userInterfaceManager) : this(new FFmpegConfig(ffmpegPath, userInterfaceManager), null, null, null) { }

        public ProcessManagerFactory(IFFmpegConfig config, IFFmpegParser ffmpegParser, IProcessFactory processFactory, IFileSystemService fileSystemService) {
            this.Config = config ?? new FFmpegConfig();
            this.Parser = ffmpegParser ?? new FFmpegParser();
            this.ProcessFactory = processFactory ?? new ProcessFactory();
            this.FileSystemService = fileSystemService ?? new FileSystemService();
        }

        #endregion

        /// <summary>
        /// Creates a new process manager with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created process manager.</returns>
        public IProcessManager Create(ProcessOptions options = null, ProcessStartedEventHandler callback = null) {
            var Result = new ProcessManager(Config, Parser, ProcessFactory, FileSystemService, options);
            if (callback != null)
                Result.ProcessStarted += callback;
            return Result;
        }

        /// <summary>
        /// Creates a new process manager to run ffmpeg with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created FFmpeg process manager.</returns>
        public IProcessManagerFFmpeg CreateFFmpeg(ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            var Result = new ProcessManagerFFmpeg(Config, Parser, ProcessFactory, FileSystemService, options);
            if (callback != null)
                Result.ProcessStarted += callback;
            return Result;
        }

        /// <summary>
        /// Creates a new process manager to run avs2yuv with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The newly created avs2yuv process manager.</returns>
        public IProcessManagerAvs CreateAvs(ProcessOptions options = null, ProcessStartedEventHandler callback = null) {
            var Result = new ProcessManagerAvs(Config, Parser, ProcessFactory, FileSystemService, options);
            if (callback != null)
                Result.ProcessStarted += callback;
            return Result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg {

    #region Interface

    /// <summary>
    /// Executes commands through FFmpeg application.
    /// </summary>
    public interface IProcessManagerFFmpeg : IProcessManager {
        /// <summary>
        /// Gets or sets the options to control the behaviors of the process.
        /// </summary>
        new ProcessOptionsFFmpeg Options { get; set; }
        /// <summary>
        /// Returns the duration of input file.
        /// </summary>
        TimeSpan FileDuration { get; }
        /// <summary>
        /// Returns the frame count of input file (estimated).
        /// </summary>
        long FrameCount { get; }
        /// <summary>
        /// Returns information about input streams.
        /// </summary>
        List<FFmpegStreamInfo> FileStreams { get; }
        /// <summary>
        /// Returns the last status data received from DataReceived event.
        /// </summary>
        FFmpegStatus LastStatusReceived { get; }
        /// <summary>
        /// Occurs after stream info is read from FFmpeg's output.
        /// </summary>
        event EventHandler InfoUpdated;
        /// <summary>
        /// Occurs when status update is received through FFmpeg's output stream.
        /// </summary>
        event StatusUpdatedEventHandler StatusUpdated;
        /// <summary>
        /// Runs FFmpeg with specified arguments.
        /// </summary>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunFFmpeg(string arguments);
        /// <summary>
        /// Runs FFmpeg with specified arguments through avs2yuv.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunAvisynthToEncoder(string source, string arguments);
        /// <summary>
        /// Runs an encoder (FFmpeg by default) with specified arguments through avs2yuv.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <param name="encoderPath">The path of the encoder to run.</param>
        /// <param name="encoderApp">The type of encoder to run, which alters parsing.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus RunAvisynthToEncoder(string source, string arguments, string encoderPath);
        /// <summary>
        /// Gets the first video stream from FileStreams.
        /// </summary>
        /// <returns>A FFmpegVideoStreamInfo object.</returns>
        FFmpegVideoStreamInfo VideoStream { get; }
        /// <summary>
        /// Gets the first audio stream from FileStreams.
        /// </summary>
        /// <returns>A FFmpegAudioStreamInfo object.</returns>
        FFmpegAudioStreamInfo AudioStream { get; }
    }

    #endregion

    /// <summary>
    /// Executes commands through FFmpeg application.
    /// </summary>
    public class ProcessManagerFFmpeg : ProcessManager, IProcessManagerFFmpeg {

        #region Declarations / Constructors

        /// <summary>
        /// Returns the duration of input file.
        /// </summary>
        public TimeSpan FileDuration { get; private set; }
        /// <summary>
        /// Returns the frame count of input file (estimated).
        /// </summary>
        public long FrameCount { get; private set; }
        /// <summary>
        /// Returns information about input streams.
        /// </summary>
        public List<FFmpegStreamInfo> FileStreams { get; private set; }
        /// <summary>
        /// Returns the last status data received from DataReceived event.
        /// </summary>
        public FFmpegStatus LastStatusReceived { get; private set; }
        /// <summary>
        /// Occurs after stream info is read from FFmpeg's output.
        /// </summary>
        public event EventHandler InfoUpdated;
        /// <summary>
        /// Occurs when status update is received through FFmpeg's output stream.
        /// </summary>
        public event StatusUpdatedEventHandler StatusUpdated;
        protected bool isStarted;


        public ProcessManagerFFmpeg() : this(new FFmpegConfig(), new FFmpegParser(), new ProcessFactory(), new FileSystemService(), new ProcessOptionsFFmpeg()) { }

        public ProcessManagerFFmpeg(IFFmpegConfig config, IFFmpegParser ffmpegParser, IProcessFactory processFactory, IFileSystemService fileSystemService, ProcessOptionsFFmpeg options = null)
            : base(config, ffmpegParser, processFactory, fileSystemService, options ?? new ProcessOptionsFFmpeg()) {
            OutputType = ProcessOutput.Error;
        }

        #endregion

        /// <summary>
        /// Gets or sets the options to control the behaviors of the FFmpeg process.
        /// </summary>
        public new ProcessOptionsFFmpeg Options {
            get => base.Options as ProcessOptionsFFmpeg;
            set => base.Options = value;
        }

        /// <summary>
        /// Returns Options.FrameCount while validating against null reference.
        /// </summary>
        protected long OptionsFrameCount => Options?.FrameCount ?? 0;

        /// <summary>
        /// Runs FFmpeg with specified arguments.
        /// </summary>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunFFmpeg(string arguments) => Run(Config.FFmpegPath, arguments);

        /// <summary>
        /// Runs FFmpeg with specified arguments through avs2yuv.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAvisynthToEncoder(string source, string arguments) => RunAvisynthToEncoder(source, arguments, null);

        /// <summary>
        /// Runs an encoder (FFmpeg by default) with specified arguments through avs2yuv.
        /// </summary>
        /// <param name="source">The path of the source Avisynth script file.</param>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <param name="encoderPath">The path of the encoder to run.</param>
        /// <param name="encoderApp">The type of encoder to run, which alters parsing.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAvisynthToEncoder(string source, string arguments, string encoderPath) {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be null or empty.", nameof(source));
            if (!fileSystem.Exists(Config.Avs2yuvPath))
                throw new System.IO.FileNotFoundException(string.Format(@"File ""{0}"" specified by Config.Avs2yuvPath is not found.", Config.Avs2yuvPath));
            String Query = string.Format(@"""{0}"" ""{1}"" -o - | ""{2}"" {3}", Config.Avs2yuvPath, source, encoderPath ?? Config.FFmpegPath, arguments);
            return RunAsCommand(Query);
        }

        /// <summary>
        /// Runs specified application with specified arguments.
        /// </summary>
        /// <param name="fileName">The application to start.</param>
        /// <param name="arguments">The set of arguments to use when starting the application.</param>
        /// <returns>The process completion status.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Occurs when the file to run is not found.</exception>
        /// <exception cref="InvalidOperationException">Occurs when this class instance is already running another process.</exception>
        public override CompletionStatus Run(string fileName, string arguments) {
            isStarted = false;
            FileStreams = null;
            FileDuration = TimeSpan.Zero;
            FrameCount = OptionsFrameCount;
            return base.Run(fileName, arguments);
        }

        /// <summary>
        /// Occurs when data is received from the executing application.
        /// </summary>
        protected override void OnDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data == null) {
                if (!isStarted)
                    ParseFileInfo();
                return;
            }

            //if (e.Data == null)
            //    return;

            base.OnDataReceived(sender, e);

            if (FileStreams == null && (e.Data.StartsWith("Output ") || e.Data.StartsWith("Press [q] to stop")))
                ParseFileInfo();
            if (e.Data.StartsWith("Press [q] to stop") || e.Data.StartsWith("frame="))
                isStarted = true;

            if (isStarted && e.Data.StartsWith("frame=")) {
                FFmpegStatus ProgressInfo = parser.ParseFFmpegProgress(e.Data);
                LastStatusReceived = ProgressInfo;
                StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(ProgressInfo));
            }

            //if (encoder == EncoderApp.FFmpeg) {
            //} else if (encoder == EncoderApp.x264) {
            //    if (!isStarted && e.Data.StartsWith("frames "))
            //        isStarted = true;
            //    else if (isStarted && e.Data.Length == 48) {
            //        FFmpegStatus ProgressInfo = parser.ParseX264Progress(e.Data);
            //        LastStatusReceived = ProgressInfo;
            //        StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(ProgressInfo));
            //    }
            //}
        }

        //private bool HasParsed = false;
        private void ParseFileInfo() {
            //HasParsed = true;
            FileStreams = parser.ParseFileInfo(output.ToString(), out TimeSpan fileDuration);
            FileDuration = fileDuration;
            if (OptionsFrameCount > 0)
                FrameCount = OptionsFrameCount;
            else if (VideoStream != null)
                FrameCount = (int)(FileDuration.TotalSeconds * VideoStream.FrameRate);
            InfoUpdated?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Gets the first video stream from FileStreams.
        /// </summary>
        /// <returns>A FFmpegVideoStreamInfo object.</returns>
        public FFmpegVideoStreamInfo VideoStream => GetStream(FFmpegStreamType.Video) as FFmpegVideoStreamInfo;

        /// <summary>
        /// Gets the first audio stream from FileStreams.
        /// </summary>
        /// <returns>A FFmpegAudioStreamInfo object.</returns>
        public FFmpegAudioStreamInfo AudioStream => GetStream(FFmpegStreamType.Audio) as FFmpegAudioStreamInfo;

        /// <summary>
        /// Returns the first stream of specified type.
        /// </summary>
        /// <param name="streamType">The type of stream to search for.</param>
        /// <returns>A FFmpegStreamInfo object.</returns>
        private FFmpegStreamInfo GetStream(FFmpegStreamType streamType) {
            if (FileStreams != null && FileStreams.Count > 0)
                return FileStreams.FirstOrDefault(f => f.StreamType == streamType);
            else
                return null;
        }
    }
}

using System;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg {

    #region Interface

    /// <summary>
    /// Provides methods to execute an Avisynth script through Avs2yuv application.
    /// </summary>
    public interface IProcessManagerAvs : IProcessManager {
        /// <summary>
        /// Runs avs2yuv with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        CompletionStatus RunAvisynth(string path);
    }

    #endregion

    /// <summary>
    /// Provides methods to execute an Avisynth script through Avs2yuv application.
    /// </summary>
    public class ProcessManagerAvs : ProcessManager, IProcessManagerAvs {

        #region Declarations / Constructors

        public ProcessManagerAvs() : this(new FFmpegConfig(), new FFmpegParser(), new ProcessFactory(), new FileSystemService()) { }

        public ProcessManagerAvs(IFFmpegConfig config, IFFmpegParser ffmpegParser, IProcessFactory processFactory, IFileSystemService fileSystemService, ProcessOptions options = null)
            : base(config, ffmpegParser, processFactory, fileSystemService, options) {
            OutputType = ProcessOutput.None;
        }

        #endregion

        /// <summary>
        /// Runs avs2yuv with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        public CompletionStatus RunAvisynth(string path) {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            if (!fileSystem.Exists(Config.Avs2yuvPath))
                throw new System.IO.FileNotFoundException(string.Format(@"File ""{0}"" specified by Config.Avs2yuvPath is not found.", Config.Avs2yuvPath));
            string TempFile = path + ".out";
            string Args = string.Format(@"""{0}"" -o ""{1}""", path, TempFile);
            CompletionStatus Result = Run(Config.Avs2yuvPath, Args);
            fileSystem.Delete(TempFile);
            return Result;
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg {

    #region Interface

    /// <summary>
    /// Contains the configuration settings for FFmpeg.
    /// </summary>
    public interface IFFmpegConfig {
        /// <summary>
        /// Gets or sets the path to FFmpeg.exe
        /// </summary>
        string FFmpegPath { get; set; }
        /// <summary>
        /// Gets or sets the path to avs2yuv.exe to use AviSynth in a separate process.
        /// </summary>
        string Avs2yuvPath { get; set; }
        /// <summary>
        /// Gets or sets a class that will manage graphical interface instances when DisplayMode = Interface
        /// </summary>
        IUserInterfaceManager UserInterfaceManager { get; set; }
        /// <summary>
        /// Occurs when a process needs to be closed. This needs to be managed manually for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        event CloseProcessEventHandler CloseProcess;
        /// <summary>
        /// Gets the path of the executing assembly.
        /// </summary>
        string ApplicationPath { get; }
        /// <summary>
        /// Returns the absolute path of FFmpeg as defined in settings.
        /// </summary>
        //string FFmpegPathAbsolute { get; }
        /// <summary>
        /// Returns the absolute path of Avs2yuv as defined in settings.
        /// </summary>
        //string Avs2yuvPathAbsolute { get; }
        /// <summary>
        /// Returns all FFmpeg running processes.
        /// </summary>
        /// <returns>A list of FFmpeg processes.</returns>
        IProcess[] GetFFmpegProcesses();
        /// <summary>
        /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        /// <param name="process">The process to close.</param>
        /// <returns>Whether the process was closed.</returns>
        bool SoftKill(IProcess process);
    }

    #endregion

    /// <summary>
    /// Contains the configuration settings for FFmpeg.
    /// </summary>
    public class FFmpegConfig : IFFmpegConfig {

        #region Declarations / Constructors

        protected readonly IWindowsApiService api;
        protected readonly IFileSystemService fileSystem;

        public FFmpegConfig() { }

        public FFmpegConfig(string ffmpegPath) : this(ffmpegPath, null) { }

        public FFmpegConfig(string ffmpegPath, IUserInterfaceManager userInterfaceManager) {
            this.FFmpegPath = ffmpegPath;
            this.UserInterfaceManager = userInterfaceManager;
            this.api = new WindowsApiService();
            this.fileSystem = new FileSystemService();
        }

        public FFmpegConfig(IWindowsApiService winApi, IFileSystemService fileSystemService) {
            this.api = winApi ?? throw new ArgumentNullException(nameof(winApi));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        #endregion

        /// <summary>
        /// Gets or sets the path to FFmpeg.exe
        /// </summary>
        public string FFmpegPath { get; set; } = "ffmpeg.exe";
        /// <summary>
        /// Gets or sets the path to avs2yuv.exe to use AviSynth in a separate process.
        /// </summary>
        public string Avs2yuvPath { get; set; } = "avs2yuv.exe";
        /// <summary>
        /// Gets or sets a class that will manage graphical interface instances when DisplayMode = Interface
        /// </summary>
        public IUserInterfaceManager UserInterfaceManager { get; set; }
        /// <summary>
        /// Occurs when a process needs to be closed. This needs to be managed manually for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        public event CloseProcessEventHandler CloseProcess;
        /// <summary>
        /// Gets the path of the executing assembly.
        /// </summary>
        public string ApplicationPath => fileSystem.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        /// <summary>
        /// Returns the absolute path of FFmpeg as defined in settings.
        /// </summary>
        //public string FFmpegPathAbsolute => GetAbsolutePath(FFmpegPath);
        /// <summary>
        /// Returns the absolute path of Avs2yuv as defined in settings.
        /// </summary>
        //public string Avs2yuvPathAbsolute => GetAbsolutePath(Avs2yuvPath);
        /// <summary>
        /// Returns the absolute path for specified absolute or relative path, using the application's path as default folder.
        /// </summary>
        /// <param name="path">The path to convert into absolute.</param>
        //private string GetAbsolutePath(string path) {
        //    if (path != null)
        //        return fileSystem.IsPathRooted(path) ? path : fileSystem.Combine(ApplicationPath, path);
        //    return null;
        //}

        #region Processes

        /// <summary>
        /// Returns all FFmpeg running processes.
        /// </summary>
        /// <returns>A list of FFmpeg processes.</returns>
        public IProcess[] GetFFmpegProcesses() {
            string ProcessName = fileSystem.GetFileNameWithoutExtension(FFmpegPath);
            return Process.GetProcessesByName(ProcessName).Select(p => new ProcessWrapper(p)).ToArray();
        }

        /// <summary>
        /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        /// <param name="process">The process to close.</param>
        /// <returns>Whether the process was closed.</returns>
        public bool SoftKill(IProcess process) {
            CloseProcessEventArgs Args = new CloseProcessEventArgs(process);
            CloseProcess?.Invoke(null, Args);
            if (!Args.Handled)
                SoftKillWinApp(process);
            return process.HasExited;
        }

        /// <summary>
        /// Soft closes from a WinForms or WPF process.
        /// </summary>
        /// <param name="process">The process to close.</param>
        public void SoftKillWinApp(IProcess process) {
            if (api.AttachConsole((uint)process.Id)) {
                api.SetConsoleCtrlHandler(null, true);
                try {
                    if (!api.GenerateConsoleCtrlEvent())
                        return;
                    process.WaitForExit();
                } finally {
                    api.FreeConsole();
                    api.SetConsoleCtrlHandler(null, false);
                }
            }
        }

        #endregion

    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

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
        string FFmpegPathAbsolute { get; }
        /// <summary>
        /// Returns the absolute path of Avs2yuv as defined in settings.
        /// </summary>
        string Avs2yuvPathAbsolute { get; }
        /// <summary>
        /// Returns all FFmpeg running processes.
        /// </summary>
        /// <returns>A list of FFmpeg processes.</returns>
        Process[] GetFFmpegProcesses();
        /// <summary>
        /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        /// <param name="process">The process to close.</param>
        /// <returns>Whether the process was closed.</returns>
        bool SoftKill(Process process);
    }

    #endregion

    /// <summary>
    /// Contains the configuration settings for FFmpeg.
    /// </summary>
    public class FFmpegConfig : IFFmpegConfig {
        public FFmpegConfig() { }

        public FFmpegConfig(string ffmpegPath) : this(ffmpegPath, null) { }

        public FFmpegConfig(string ffmpegPath, IUserInterfaceManager userInterfaceManager) {
            this.FFmpegPath = ffmpegPath;
            this.UserInterfaceManager = userInterfaceManager;
        }

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
        public string ApplicationPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        /// <summary>
        /// Returns the absolute path of FFmpeg as defined in settings.
        /// </summary>
        public string FFmpegPathAbsolute => Path.IsPathRooted(FFmpegPath) ? FFmpegPath : Path.Combine(ApplicationPath, FFmpegPath);
        /// <summary>
        /// Returns the absolute path of Avs2yuv as defined in settings.
        /// </summary>
        public string Avs2yuvPathAbsolute => Path.IsPathRooted(Avs2yuvPath) ? Avs2yuvPath : Path.Combine(ApplicationPath, Avs2yuvPath);

        #region Processes

        /// <summary>
        /// Returns all FFmpeg running processes.
        /// </summary>
        /// <returns>A list of FFmpeg processes.</returns>
        public Process[] GetFFmpegProcesses() {
            string ProcessName = Path.GetFileNameWithoutExtension(FFmpegPath);
            return Process.GetProcessesByName(ProcessName);
        }

        internal const int CTRL_C_EVENT = 0;
        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        // Delegate type to be used as the Handler Routine for SCCH
        private delegate bool ConsoleCtrlDelegate(uint CtrlType);

        /// <summary>
        /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        /// <param name="process">The process to close.</param>
        /// <returns>Whether the process was closed.</returns>
        public bool SoftKill(Process process) {
            ProcessEventArgs Args = new ProcessEventArgs(process);
            CloseProcess?.Invoke(null, Args);
            if (!Args.Handled)
                SoftKillWinApp(process);
            return process.HasExited;
        }

        /// <summary>
        /// Soft closes from a WinForms or WPF process.
        /// </summary>
        /// <param name="process">The process to close.</param>
        public void SoftKillWinApp(Process process) {
            if (AttachConsole((uint)process.Id)) {
                SetConsoleCtrlHandler(null, true);
                try {
                    if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                        return;
                    process.WaitForExit();
                } finally {
                    FreeConsole();
                    SetConsoleCtrlHandler(null, false);
                }
            }
        }

        #endregion

    }
}

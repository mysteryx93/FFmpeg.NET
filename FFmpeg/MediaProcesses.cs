using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Provides functions to manage processes.
    /// </summary>
    public static class MediaProcesses {
        /// <summary>
        /// Returns all FFmpeg running processes.
        /// </summary>
        /// <returns>A list of FFmpeg processes.</returns>
        public static Process[] GetFFmpegProcesses() {
            string ProcessName = Path.GetFileNameWithoutExtension(FFmpegConfig.FFmpegPath);
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
        delegate Boolean ConsoleCtrlDelegate(uint CtrlType);

        /// <summary>
        /// Soft closes a process. This will work on WinForms and WPF, but needs to be managed differently for Console applications.
        /// See http://stackoverflow.com/a/29274238/3960200
        /// </summary>
        /// <param name="process">The process to close.</param>
        /// <returns>Whether the process was closed.</returns>
        public static bool SoftKill(Process process) {
            ProcessEventArgs Args = new ProcessEventArgs(process);
            FFmpegConfig.CloseProcess?.Invoke(null, Args);
            if (!Args.Handled)
                SoftKillWinApp(process);
            return process.HasExited;
        }

        /// <summary>
        /// Soft closes from a WinForms or WPF process.
        /// </summary>
        /// <param name="process">The process to close.</param>
        public static void SoftKillWinApp(Process process) {
            if (AttachConsole((uint)process.Id)) {
                SetConsoleCtrlHandler(null, true);
                try {
                    if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
                        return;
                    process.WaitForExit();
                }
                finally {
                    FreeConsole();
                    SetConsoleCtrlHandler(null, false);
                }
            }
        }
    }
}

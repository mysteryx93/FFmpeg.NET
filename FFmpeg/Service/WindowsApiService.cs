using System;
using System.Runtime.InteropServices;

namespace EmergenceGuardian.FFmpeg.Services {

    #region Interface

    /// <summary>
    /// Provides Windows API functions to manage processes.
    /// </summary>
    public interface IWindowsApiService {
        bool GenerateConsoleCtrlEvent();
        bool AttachConsole(uint processId);
        bool FreeConsole();
        bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);
    }

    #endregion

    /// <summary>
    /// Provides Windows API functions to manage processes.
    /// </summary>
    public class WindowsApiService: IWindowsApiService {

        public bool GenerateConsoleCtrlEvent() => Api.GenerateConsoleCtrlEvent(Api.CTRL_C_EVENT, 0);

        public bool AttachConsole(uint processId) => Api.AttachConsole(processId);

        public bool FreeConsole() => Api.FreeConsole();

        public bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add) => Api.SetConsoleCtrlHandler(handlerRoutine, add);

        private static class Api {
            internal const int CTRL_C_EVENT = 0;
            [DllImport("kernel32.dll")]
            internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern bool AttachConsole(uint dwProcessId);
            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            internal static extern bool FreeConsole();
            [DllImport("kernel32.dll")]
            internal static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
            // Delegate type to be used as the Handler Routine for SCCH
        }
    }
}

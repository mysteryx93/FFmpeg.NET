using System;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Represents the method that will handle the StatusUpdated event.
    /// </summary>
    public delegate void StatusUpdatedEventHandler(object sender, StatusUpdatedEventArgs e);

    /// <summary>
    /// Provides progress information for the StatusUpdated event.
    /// </summary>
    public class StatusUpdatedEventArgs : EventArgs {
        public FFmpegStatus Status { get; set; }

        public StatusUpdatedEventArgs() {
        }

        public StatusUpdatedEventArgs(FFmpegStatus progress) {
            Status = progress;
        }
    }

    /// <summary>
    /// Represents the method that will handle the ProcessStarted event.
    /// </summary>
    public delegate void ProcessStartedEventHandler(object sender, ProcessStartedEventArgs e);

    /// <summary>
    /// Provides job information for the ProcessStarted event.
    /// </summary>
    public class ProcessStartedEventArgs : EventArgs {
        public IProcessManager ProcessManager { get; set; }

        public ProcessStartedEventArgs() { }

        public ProcessStartedEventArgs(IProcessManager processManager) {
            ProcessManager = processManager;
        }
    }

    /// <summary>
    /// Represents a method that will be called when a process needs to be closed.
    /// </summary>
    public delegate void CloseProcessEventHandler(object sender, CloseProcessEventArgs e);

    /// <summary>
    /// Provides process information for CloseProcess event.
    /// </summary>
    public class CloseProcessEventArgs : EventArgs {
        public IProcess Process { get; set; }
        public bool Handled { get; set; } = false;

        public CloseProcessEventArgs() {
        }

        public CloseProcessEventArgs(IProcess process) {
            Process = process;
        }
    }

    /// <summary>
    /// Represents the method that will handle the ProcessCompleted event.
    /// </summary>
    public delegate void ProcessCompletedEventHandler(object sender, ProcessCompletedEventArgs e);

    /// <summary>
    /// Provides progress information for the ProcessCompleted event.
    /// </summary>
    public class ProcessCompletedEventArgs : EventArgs {
        public CompletionStatus Status { get; set; }

        public ProcessCompletedEventArgs() { }

        public ProcessCompletedEventArgs(CompletionStatus status) {
            Status = status;
        }
    }

    /// <summary>
    /// Delegate used for SetConsoleCtrlHandler Win API call.
    /// </summary>
    public delegate bool ConsoleCtrlDelegate(uint CtrlType);
}

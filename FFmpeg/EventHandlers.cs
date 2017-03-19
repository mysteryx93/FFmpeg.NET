using System;
using System.Diagnostics;

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
    /// Represents the method that will handle the Started event.
    /// </summary>
    public delegate void StartedEventHandler(object sender, StartedEventArgs e);

    /// <summary>
    /// Provides job information for the Started event.
    /// </summary>
    public class StartedEventArgs : EventArgs {
        public FFmpegProcess Process { get; set; }

        public StartedEventArgs() {
        }

        public StartedEventArgs(FFmpegProcess process) {
            Process = process;
        }
    }

    /// <summary>
    /// Represents a method that will be called when a process needs to be closed.
    /// </summary>
    public delegate void CloseProcessEventHandler(object sender, ProcessEventArgs e);

    /// <summary>
    /// Provides process information for CloseProcess event.
    /// </summary>
    public class ProcessEventArgs : EventArgs {
        public Process Process { get; set; }

        public ProcessEventArgs() {
        }

        public ProcessEventArgs(Process process) {
            Process = process;
        }
    }
}

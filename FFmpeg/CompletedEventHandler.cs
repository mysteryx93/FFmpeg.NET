using System;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Represents the method that will handle the ProgressUpdated event.
    /// </summary>
    public delegate void CompletedEventHandler(object sender, CompletedEventArgs e);

    /// <summary>
    /// Provides progress information for the ProgressUpdated event.
    /// </summary>
    public class CompletedEventArgs : EventArgs {
        public CompletionStatus Status { get; set; }

        public CompletedEventArgs() {
        }

        public CompletedEventArgs(CompletionStatus status) {
            Status = status;
        }
    }
}

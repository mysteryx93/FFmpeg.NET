using System;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Represents the process completion status.
    /// </summary>
    public enum CompletionStatus {
        /// <summary>
        /// Process is not yet completed.
        /// </summary>
        None,
        /// <summary>
        /// Process completed successfully.
        /// </summary>
        Success,
        /// <summary>
        /// Process terminated with an error.
        /// </summary>
        Error,
        /// <summary>
        /// Process has been cancelled by the user.
        /// </summary>
        Cancelled,
        /// <summary>
        /// Process was stopped after a timeout.
        /// </summary>
        Timeout
    }
}

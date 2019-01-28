using System;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Represents the application to use for encoding.
    /// </summary>
    public enum EncoderApp {
        Other,
        FFmpeg,
        x264
    }

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

    /// <summary>
    /// Represents the various FFmpeg display modes.
    /// </summary>
    public enum FFmpegDisplayMode {
        /// <summary>
        /// No graphical interface will be created but events can still be handled manually.
        /// </summary>
        None,
        /// <summary>
        /// The native FFmpeg console window will be displayed. Events will not be fired.
        /// </summary>
        Native,
        /// <summary>
        /// IUserInterfaceManager will be used to display and manage the graphical interface.
        /// </summary>
        Interface,
        /// <summary>
        /// IUserInterfaceManager will be used to display the process' output if the process returned an error.
        /// </summary>
        ErrorOnly
    }

    /// <summary>
    /// Represents the type of media file stream.
    /// </summary>
    public enum FFmpegStreamType {
        /// <summary>
        /// No stream type specified.
        /// </summary>
        None,
        /// <summary>
        /// Video stream.
        /// </summary>
        Video,
        /// <summary>
        /// Audio stream.
        /// </summary>
        Audio
    }

    /// <summary>
    /// Represents which process output to read.
    /// </summary>
    public enum ProcessOutput {
        None,
        Standard,
        Error
    }
}

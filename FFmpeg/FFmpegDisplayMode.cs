using System;

namespace EmergenceGuardian.FFmpeg {
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
}

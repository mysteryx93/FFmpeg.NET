using System;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Represents the method that will handle the ProgressUpdated event.
    /// </summary>
    public delegate void ProgressUpdatedEventHandler(object sender, ProgressUpdatedEventArgs e);

    /// <summary>
    /// Provides progress information for the ProgressUpdated event.
    /// </summary>
    public class ProgressUpdatedEventArgs : EventArgs {
        public FFmpegProgress Progress { get; set; }

        public ProgressUpdatedEventArgs() {
        }

        public ProgressUpdatedEventArgs(FFmpegProgress progress) {
            Progress = progress;
        }
    }
}

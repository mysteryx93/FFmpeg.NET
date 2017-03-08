using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Contains the options that can be set while starting a new process.
    /// </summary>
    public class ProcessStartOptions {
        /// <summary>
        /// Gets or sets the display mode when running FFmpeg.
        /// </summary>
        public FFmpegDisplayMode DisplayMode = FFmpegDisplayMode.None;
        /// <summary>
        /// Gets or sets the title to display in the user interface.
        /// </summary>
        public string DisplayTitle;
        /// <summary>
        /// Gets or sets the overall priority category for the associated process.
        /// </summary>
        public ProcessPriorityClass Priority = ProcessPriorityClass.Normal;

        /// <summary>
        /// Initializes a new instance of the ProcessStartOptions class.
        /// </summary>
        public ProcessStartOptions() {
        }

        /// <summary>
        /// Initializes a new instance of the ProcessStartOptions class.
        /// </summary>
        /// <param name="displayMode">Gets or sets the display mode when running FFmpeg.</param>
        public ProcessStartOptions(FFmpegDisplayMode displayMode) {
            this.DisplayMode = displayMode;
        }

        /// <summary>
        /// Initializes a new instance of the ProcessStartOptions class.
        /// </summary>
        /// <param name="displayMode">Gets or sets the display mode when running FFmpeg.</param>
        /// <param name="displayTitle">The title to display in the user interface.</param>
        public ProcessStartOptions(FFmpegDisplayMode displayMode, string displayTitle) {
            this.DisplayMode = displayMode;
            this.DisplayTitle = displayTitle;
        }

        /// <summary>
        /// Initializes a new instance of the ProcessStartOptions class.
        /// </summary>
        /// <param name="displayMode">The display mode when running FFmpeg.</param>
        /// <param name="displayTitle">The title to display in the user interface.</param>
        /// <param name="priority">The overall priority category for the associated process.</param>
        public ProcessStartOptions(FFmpegDisplayMode displayMode, string displayTitle, ProcessPriorityClass priority) {
            this.DisplayMode = displayMode;
            this.DisplayTitle = displayTitle;
            this.Priority = priority;
        }
    }
}

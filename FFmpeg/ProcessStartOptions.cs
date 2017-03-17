using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Contains options to control the behaviors of a process.
    /// </summary>
    public class ProcessStartOptions {
        /// <summary>
        /// Gets or sets the display mode when running FFmpeg.
        /// </summary>
        public FFmpegDisplayMode DisplayMode { get; set; } = FFmpegDisplayMode.None;
        /// <summary>
        /// Gets or sets the title to display.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the overall priority category for the associated process.
        /// </summary>
        public ProcessPriorityClass Priority { get; set; } = ProcessPriorityClass.Normal;
        /// <summary>
        /// Gets or sets an identifier for the job.
        /// </summary>
        public object JobId { get; set; }
        /// <summary>
        /// If displaying several tasks in the same UI, gets whether this is the main task being performed.
        /// </summary>
        public bool IsMainTask { get; set; } = true;
        /// <summary>
        /// Gets or sets the frame count to use when it is not automatically provided by the input file.
        /// </summary>
        public long FrameCount { get; set; }
        /// <summary>
        /// If resuming a job, gets or sets the number of frames that were done previously.
        /// </summary>
        public long ResumePos { get; set; }
        /// <summary>
        /// Gets or sets a timeout, in milliseconds, after which the process will be stopped.
        /// </summary>
        public TimeSpan Timeout { get; set; }
        /// <summary>
        /// Occurs when the job is starting.
        /// </summary>
        public event StartedEventHandler Started;

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
        /// <param name="title">The title to display.</param>
        public ProcessStartOptions(FFmpegDisplayMode displayMode, string title) {
            this.DisplayMode = displayMode;
            this.Title = title;
        }

        /// <summary>
        /// Initializes a new instance of the ProcessStartOptions class.
        /// </summary>
        /// <param name="displayMode">The display mode when running FFmpeg.</param>
        /// <param name="title">The title to display..</param>
        /// <param name="priority">The overall priority category for the associated process.</param>
        public ProcessStartOptions(FFmpegDisplayMode displayMode, string title, ProcessPriorityClass priority) {
            this.DisplayMode = displayMode;
            this.Title = title;
            this.Priority = priority;
        }

        /// <summary>
        /// Initializes a new instance of the ProcessStartOptions class to display several jobs in the same UI.
        /// </summary>
        /// <param name="jobId">An identifier for the job. Can be used to link a set of jobs to the same UI.</param>
        /// <param name="title">The title to display.</param>
        /// <param name="isMainTask">When displaying several tasks in the same UI, whether this is the main task.</param>
        public ProcessStartOptions(object jobId, string title, bool isMainTask) {
            this.DisplayMode = FFmpegDisplayMode.Interface;
            this.JobId = jobId;
            this.Title = title;
            this.IsMainTask = isMainTask;
        }

        /// <summary>
        /// Raises the Started event.
        /// </summary>
        /// <param name="process">The FFmpegProcess object to pass to the event.</param>
        internal void RaiseStarted(FFmpegProcess process) {
            Started?.Invoke(this, new StartedEventArgs(process));
        }
    }
}

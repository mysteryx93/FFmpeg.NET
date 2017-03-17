using System;
using System.Linq;
using System.Collections.Generic;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Base class to implement a user interface for FFmpeg processes.
    /// </summary>
    public abstract class UserInterfaceManagerBase {
        private List<UIItem> UIList = new List<UIItem>();

        /// <summary>
        /// Starts a user interface that will receive all tasks with the specified jobId.
        /// </summary>
        /// <param name="jobId">The jobId associated with this interface.</param>
        /// <param name="title">The title to display.</param>
        public void Start(object jobId, string title) {
            if (!UIList.Any(u => u.JobId.Equals(jobId)))
                UIList.Add(new UIItem(jobId, CreateUI(title, false)));
        }

        /// <summary>
        /// Closes the user interface for specified jobId.
        /// </summary>
        /// <param name="jobId">The jobId to close.</param>
        public void Stop(object jobId) {
            foreach (var item in UIList.Where(u => u.JobId.Equals(jobId)).ToArray()) {
                UIList.Remove(item);
                item.Value.Stop();
            }
        }

        /// <summary>
        /// Displays a FFmpeg process to the user.
        /// </summary>
        /// <param name="host">The FFmpegProcess to display.</param>
        public void Display(FFmpegProcess host) {
            UIItem UI = null;
            if (host.Options.JobId != null)
                UI = UIList.FirstOrDefault(u => u.JobId.Equals(host.Options.JobId));
            if (UI != null)
                UI.Value.DisplayTask(host);
            else {
                string Title = !string.IsNullOrEmpty(host.Options.Title) ? host.Options.Title : "FFmpeg Work in Progress";
                CreateUI(Title, true).DisplayTask(host);
            }
        }

        /// <summary>
        /// When implemented in a derived class, creates the graphical interface window.
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="autoClose">Whether to automatically close the window after the main task is completed.</param>
        /// <returns>The newly created user interface window.</returns>
        public abstract IUserInterface CreateUI(string title, bool autoClose);
        /// <summary>
        /// When implemented in a derived class, displays an error window.
        /// </summary>
        /// <param name="host">The task throwing the error.</param>
        public abstract void DisplayError(FFmpegProcess host);

        private class UIItem {
            public object JobId { get; set; }
            public IUserInterface Value { get; set; }

            public UIItem() { }

            public UIItem(object jobId, IUserInterface ui) {
                this.JobId = jobId;
                this.Value = ui;
            }
        }
    }

    /// <summary>
    /// Provides an interface that must be implemented by the FFmpeg graphical interface window.
    /// </summary>
    public interface IUserInterface {
        /// <summary>
        /// Closes the window.
        /// </summary>
        void Stop();
        /// <summary>
        /// Displays specified process.
        /// </summary>
        /// <param name="host">The process to display.</param>
        void DisplayTask(FFmpegProcess host);
    }
}
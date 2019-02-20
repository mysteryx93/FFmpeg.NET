using System;
using System.Diagnostics;

namespace EmergenceGuardian.FFmpeg {

    #region Interface

    /// <summary>
    /// Provides information about the application, environment and operating system.
    /// </summary>
    public interface IEnvironmentService {
        /// <summary>
        /// Gets the current date and time expressed as local time.
        /// </summary>
        DateTime Now { get; }
        /// <summary>
        /// Gets the current date and time expressed as Universal Standard Time.
        /// </summary>
        DateTime UtcNow { get; }
        /// <summary>
        /// Gets the newline string defined for this environment.
        /// </summary>
        string NewLine { get; }
    }

    #endregion

    /// <summary>
    /// Provides information about the application, environment and operating system.
    /// </summary>
    public class EnvironmentService : IEnvironmentService {
        public EnvironmentService() { }

        /// <summary>
        /// Gets the current date and time expressed as local time.
        /// </summary>
        public DateTime Now => DateTime.Now;

        /// <summary>
        /// Gets the current date and time expressed as Universal Standard Time.
        /// </summary>
        public DateTime UtcNow => DateTime.UtcNow;

        /// <summary>
        /// Gets the newline string defined for this environment.
        /// </summary>
        public string NewLine => Environment.NewLine;
    }
}

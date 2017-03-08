using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Provides an interface to create new instances of graphical interfaces.
    /// </summary>
    public interface IUserInterfaceManager {
        /// <summary>
        /// Creates an instance of a graphical interface for specified FFmpeg instance.
        /// </summary>
        /// <param name="host">A FFmpeg instance that will be bound to the graphical interface.</param>
        void CreateInstance(FFmpegProcess host);
        /// <summary>
        /// Displays specified output after a job threw an error.
        /// </summary>
        /// <param name="displayText">The title to display in the user interface.</param>
        /// <param name="output">The job's output.</param>
        void DisplayError(string displayText, string output);
    }
}

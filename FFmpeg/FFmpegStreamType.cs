using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceGuardian.FFmpeg {
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
}

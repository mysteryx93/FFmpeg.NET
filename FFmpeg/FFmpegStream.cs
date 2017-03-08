using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Represents a file stream.
    /// </summary>
    public class FFmpegStream {
        public string Path { get; set; }
        public int Index { get; set; }
        public string Format { get; set; }
        public FFmpegStreamType Type { get; set; }
    }    
}

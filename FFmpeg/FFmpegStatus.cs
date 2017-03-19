using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Contains progress information returned from FFmpeg's output.
    /// </summary>
    public class FFmpegStatus {
        public long Frame { get; set; }
        public float Fps { get; set; }
        public float Quantizer { get; set; }
        public string Size { get; set; }
        public TimeSpan Time { get; set; }
        public string Bitrate { get; set; }
        public float Speed { get; set; }
    }
}

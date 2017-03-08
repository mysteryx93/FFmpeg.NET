using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Base class for FFmpegVideoStream and FFmpegAudioStream representing a file stream.
    /// </summary>
    public abstract class FFmpegStreamInfo {
        public int Index { get; set; }
        public string Format { get; set; }

        /// <summary>
        /// Returns the stream type based on the derived class type.
        /// </summary>
        public FFmpegStreamType StreamType {
            get {
                return this.GetType() == typeof(FFmpegVideoStreamInfo) ? FFmpegStreamType.Video : this.GetType() == typeof(FFmpegAudioStreamInfo) ? FFmpegStreamType.Audio : FFmpegStreamType.None;
            }
        }
    }

    /// <summary>
    /// Represents a video stream with its info.
    /// </summary>
    public class FFmpegVideoStreamInfo : FFmpegStreamInfo {
        public string ColorSpace { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int SAR1 { get; set; }
        public int SAR2 { get; set; }
        public int DAR1 { get; set; }
        public int DAR2 { get; set; }
        public double PixelAspectRatio { get; set; }
        public double DisplayAspectRatio { get; set; }
        public double FrameRate { get; set; }
    }

    /// <summary>
    /// Represents an audio stream with its info.
    /// </summary>
    public class FFmpegAudioStreamInfo : FFmpegStreamInfo {
        public int SampleRate { get; set; }
        public string Channels { get; set; }
        public string BitDepth { get; set; }
        public string Bitrate { get; set; }
    }

    /// <summary>
    /// Contains progress information returned from FFmpeg's output.
    /// </summary>
    public class FFmpegProgress {
        public int Frame { get; set; }
        public float Fps { get; set; }
        public float Quantizer { get; set; }
        public string Size { get; set; }
        public TimeSpan Time { get; set; }
        public string Bitrate { get; set; }
        public float Speed { get; set; }
        public TimeSpan EstimatedTimeLeft { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Provides functions to encode media files.
    /// </summary>
    public static class MediaEncoder {
        /// <summary>
        /// Converts specified file into AVI UT Video format.
        /// </summary>
        /// <param name="source">The file to convert.</param>
        /// <param name="destination">The destination file, ending with .AVI</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus ConvertToAvi(string source, string destination, ProcessStartOptions options) {
            // -vcodec huffyuv or utvideo, -acodec pcm_s16le
            return Encode(source, "utvideo", "pcm_s16le", null, destination, options);
        }

        /// <summary>
        /// Encodes a media file with specified arguments. 
        /// </summary>
        /// <param name="source">The file to convert.</param>
        /// <param name="videoCodec">The codec to use to encode the video stream.</param>
        /// <param name="audioCodec">The codec to use to encode the audio stream.</param>
        /// <param name="encodeArgs">Additional arguments to pass to FFmpeg.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus Encode(string source, string videoCodec, string audioCodec, string encodeArgs, string destination, ProcessStartOptions options) {
            string[] VideoCodecList = string.IsNullOrEmpty(videoCodec) ? null : new string[] { videoCodec };
            string[] AudioCodecList = string.IsNullOrEmpty(audioCodec) ? null : new string[] { audioCodec };
            return Encode(source, VideoCodecList, AudioCodecList, encodeArgs, destination, options);
        }

        /// <summary>
        /// Encodes a media file with specified arguments. 
        /// </summary>
        /// <param name="source">The file to convert.</param>
        /// <param name="videoCodec">The codec(s) to use to encode the video stream(s).</param>
        /// <param name="audioCodec">The codec(s) to use to encode the audio stream(s).</param>
        /// <param name="encodeArgs">Additional arguments to pass to FFmpeg.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus Encode(string source, string[] videoCodec, string[] audioCodec, string encodeArgs, string destination, ProcessStartOptions options) {
            File.Delete(destination);
            StringBuilder Query = new StringBuilder();
            Query.Append("-y -i ");
            // AviSynth source will pipe through Avs2Yuv into FFmpeg.
            bool SourceAvisynth = source.ToLower().EndsWith(".avs");
            if (SourceAvisynth)
                Query.Append("-"); // Pipe source
            else {
                Query.Append("\"");
                Query.Append(source);
                Query.Append("\"");
            }

            // Add video codec.
            if (videoCodec == null || videoCodec.Length == 0)
                Query.Append(" -vn");
            else if (videoCodec.Length == 1) {
                Query.Append(" -vcodec ");
                Query.Append(videoCodec[0]);
            } else {
                for (int i = 0; i < videoCodec.Length; i++) {
                    Query.Append(" -vcodec:");
                    Query.Append(i);
                    Query.Append(" ");
                    Query.Append(videoCodec[i]);
                }
            }

            // Add audio codec.
            if (audioCodec == null || audioCodec.Length == 0)
                Query.Append(" -an");
            else if (audioCodec.Length == 1) {
                Query.Append(" -acodec ");
                Query.Append(audioCodec[0]);
            } else {
                for (int i = 0; i < audioCodec.Length; i++) {
                    Query.Append(" -acodec:");
                    Query.Append(i);
                    Query.Append(" ");
                    Query.Append(audioCodec[i]);
                }
            }

            if (!string.IsNullOrEmpty(encodeArgs)) {
                Query.Append(" ");
                Query.Append(encodeArgs);
            }

            Query.Append(" \"");
            Query.Append(destination);
            Query.Append("\"");

            // Run FFmpeg with query.
            FFmpegProcess Worker = new FFmpeg.FFmpegProcess(options);
            CompletionStatus Result = SourceAvisynth ? 
                Worker.RunAvisynthToFFmpeg(source, Query.ToString()) : 
                Worker.RunFFmpeg(Query.ToString());
            return Result;
        }
        
    }
}

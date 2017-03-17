using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Provides functions to manage audio and video streams.
    /// </summary>
    public static class MediaMuxer {
        /// <summary>
        /// Merges specified audio and video files.
        /// </summary>
        /// <param name="videoFile">The file containing the video.</param>
        /// <param name="audioFile">The file containing the audio.</param>
        /// <param name="destination">The destination file.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus Muxe(string videoFile, string audioFile, string destination) {
            return Muxe(videoFile, audioFile, destination, null);
        }

        /// <summary>
        /// Merges specified audio and video files.
        /// </summary>
        /// <param name="videoFile">The file containing the video.</param>
        /// <param name="audioFile">The file containing the audio.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus Muxe(string videoFile, string audioFile, string destination, ProcessStartOptions options) {
            CompletionStatus Result = CompletionStatus.Success;
            File.Delete(destination);

            // FFMPEG fails to muxe H264 into MKV container. Converting to MP4 and then muxing with the audio, however, works.
            string OriginalVideoFile = videoFile;
            if ((videoFile.EndsWith(".264") || videoFile.EndsWith(".265")) && destination.ToLower().EndsWith(".mkv")) {
                videoFile = videoFile.Substring(0, videoFile.Length - 4) + ".mp4";
                Result = Muxe(OriginalVideoFile, null, videoFile, options);
            }

            if (Result == CompletionStatus.Success) {
                // Join audio and video files.
                FFmpegProcess Worker = new FFmpegProcess(options);
                // FFMPEG-encoded AAC streams are invalid and require an extra flag to join.
                bool FixAac = audioFile != null ? audioFile.ToLower().EndsWith(".aac") : false;
                string Query;
                if (string.IsNullOrEmpty(audioFile))
                    Query = string.Format(@"-y -i ""{0}"" -vcodec copy -an ""{1}""", videoFile, destination);
                else if (string.IsNullOrEmpty(videoFile))
                    Query = string.Format(@"-y -i ""{0}"" -acodec copy -vn ""{1}""", videoFile, destination);
                else
                    Query = string.Format(@"-y -i ""{0}"" -i ""{1}"" -acodec copy -vcodec copy -map 0:v -map 1:a{2} ""{3}""", videoFile, audioFile, FixAac ? " -bsf:a aac_adtstoasc" : "", destination);
                Result = Worker.RunFFmpeg(Query);
            }

            // Delete temp file.
            if (OriginalVideoFile != videoFile)
                File.Delete(videoFile);
            return Result;
        }

        /// <summary>
        /// Merges the specified list of file streams.
        /// </summary>
        /// <param name="fileStreams">The list of file streams to include in the output.</param>
        /// <param name="destination">The destination file.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus Muxe(IEnumerable<FFmpegStream> fileStreams, string destination) {
            return Muxe(fileStreams, destination, null);
        }

        /// <summary>
        /// Merges the specified list of file streams.
        /// </summary>
        /// <param name="fileStreams">The list of file streams to include in the output.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus Muxe(IEnumerable<FFmpegStream> fileStreams, string destination, ProcessStartOptions options) {
            CompletionStatus Result = CompletionStatus.Success;
            List<string> TempFiles = new List<string>();
            File.Delete(destination);

            // FFMPEG fails to muxe H264 into MKV container. Converting to MP4 and then muxing with the audio, however, works.
            foreach (FFmpegStream item in fileStreams) {
                if (item.Type == FFmpegStreamType.Video && (item.Path.EndsWith(".264") || item.Path.EndsWith(".265")) && destination.EndsWith(".mkv")) {
                    string NewFile = item.Path.Substring(0, item.Path.Length - 4) + ".mp4";
                    Result = Muxe(item.Path, null, NewFile, options);
                    TempFiles.Add(NewFile);
                    if (Result != CompletionStatus.Success)
                        break;
                }
            }

            if (Result == CompletionStatus.Success) {
                // Join audio and video files.
                StringBuilder Query = new StringBuilder();
                StringBuilder Map = new StringBuilder();
                Query.Append("-y ");
                int StreamIndex = 0;
                bool HasVideo = false, HasAudio = false, HasPcmDvdAudio = false;
                foreach (FFmpegStream item in fileStreams.OrderBy(f => f.Type)) {
                    if (item.Type == FFmpegStreamType.Video)
                        HasVideo = true;
                    if (item.Type == FFmpegStreamType.Audio) {
                        HasAudio = true;
                        if (item.Format == "pcm_dvd")
                            HasPcmDvdAudio = true;
                    }
                    Query.Append("-i \"");
                    Query.Append(item.Path);
                    Query.Append("\" ");
                    Map.Append("-map ");
                    Map.Append(StreamIndex++);
                    Map.Append(":");
                    Map.Append(item.Index);
                    Map.Append(" ");
                }
                if (HasVideo)
                    Query.Append("-vcodec copy ");
                if (HasAudio)
                    Query.Append(HasPcmDvdAudio ? "-acodec pcm_s16le " : "-acodec copy ");
                Query.Append(Map);
                // FFMPEG-encoded AAC streams are invalid and require an extra flag to join.
                if (fileStreams.Any(f => f.Path.ToLower().EndsWith(".aac")))
                    Query.Append(" -bsf:a aac_adtstoasc");
                Query.Append("\"");
                Query.Append(destination);
                Query.Append("\"");
                FFmpegProcess Worker = new FFmpegProcess(options);
                Worker.RunFFmpeg(Query.ToString());
            }

            // Delete temp file.
            foreach (string item in TempFiles) {
                File.Delete(item);
            }
            return Result;
        }

        /// <summary>
        /// Concatenates (merges) all specified files.
        /// </summary>
        /// <param name="files">The files to merge.</param>
        /// <param name="destination">The destination file.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus ConcatenateFiles(IEnumerable<string> files, string destination) {
            return Concatenate(files, destination, null);
        }

        /// <summary>
        /// Concatenates (merges) all specified files.
        /// </summary>
        /// <param name="files">The files to merge.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus Concatenate(IEnumerable<string> files, string destination, ProcessStartOptions options) {
            CompletionStatus Result = CompletionStatus.None;
            
            // Write temp file.
            string TempFile = Path.Combine(Path.GetDirectoryName(destination), "MergeList.txt");
            StringBuilder TempContent = new StringBuilder();
            foreach (string item in files) {
                TempContent.AppendFormat("file '{0}'", item).AppendLine();
            }
            File.WriteAllText(TempFile, TempContent.ToString());

            string Query = string.Format(@"-y -f concat -fflags +genpts -async 1 -safe 0 -i ""{0}"" -c copy ""{1}""", TempFile, destination);

            FFmpegProcess Worker = new FFmpegProcess(options);
            Result = Worker.RunFFmpeg(Query.ToString());

            File.Delete(TempFile);
            return Result;
        }

        /// <summary>
        /// Extracts the video stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus ExtractVideo(string source, string destination) {
            return ExtractVideo(source, destination, null);
        }

        /// <summary>
        /// Extracts the video stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus ExtractVideo(string source, string destination, ProcessStartOptions options) {
            File.Delete(destination);
            FFmpegProcess Worker = new FFmpeg.FFmpegProcess(options);
            return Worker.RunFFmpeg(string.Format(@"-y -i ""{0}"" -vcodec copy -an ""{1}""", source, destination));
        }

        /// <summary>
        /// Extracts the audio stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus ExtractAudio(string source, string destination) {
            return ExtractAudio(source, destination, null);
        }

        /// <summary>
        /// Extracts the audio stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>The process completion status.</returns>
        public static CompletionStatus ExtractAudio(string source, string destination, ProcessStartOptions options) {
            File.Delete(destination);
            FFmpegProcess Worker = new FFmpeg.FFmpegProcess(options);
            return Worker.RunFFmpeg(string.Format(@"-y -i ""{0}"" -vn -acodec copy ""{1}""", source, destination));
        }
    }
}

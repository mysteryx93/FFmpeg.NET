using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmergenceGuardian.FFmpeg.Services;

namespace EmergenceGuardian.FFmpeg {

    #region Interface

    /// <summary>
    /// Provides functions to manage audio and video streams.
    /// </summary>
    public interface IMediaMuxer {
        /// <summary>
        /// Merges specified audio and video files.
        /// </summary>
        /// <param name="videoFile">The file containing the video.</param>
        /// <param name="audioFile">The file containing the audio.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus Muxe(string videoFile, string audioFile, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Merges the specified list of file streams.
        /// </summary>
        /// <param name="fileStreams">The list of file streams to include in the output.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus Muxe(IEnumerable<FFmpegStream> fileStreams, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Extracts the video stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus ExtractVideo(string source, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Extracts the audio stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus ExtractAudio(string source, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Concatenates (merges) all specified files.
        /// </summary>
        /// <param name="files">The files to merge.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus Concatenate(IEnumerable<string> files, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
        /// <summary>
        /// Truncates a media file from specified start position with specified duration.
        /// </summary>
        /// <param name="source">The source file to truncate.</param>
        /// <param name="destination">The output file to write.</param>
        /// <param name="startPos">The position where to start copying. Anything before this position will be ignored. TimeSpan.Zero or null to start from beginning.</param>
        /// <param name="duration">The duration after which to stop copying. Anything after this duration will be ignored. TimeSpan.Zero or null to copy until the end.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        CompletionStatus Truncate(string source, string destination, TimeSpan? startPos, TimeSpan? duration = null, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null);
    }

    #endregion

    /// <summary>
    /// Provides functions to manage audio and video streams.
    /// </summary>
    public class MediaMuxer : IMediaMuxer {

        #region Declarations / Constructors

        protected readonly IProcessManagerFactory factory;
        protected readonly IFileSystemService fileSystem;

        public MediaMuxer() : this(new ProcessManagerFactory(), new FileSystemService()) { }

        public MediaMuxer(IProcessManagerFactory processFactory) : this(processFactory, new FileSystemService()) { }

        public MediaMuxer(IProcessManagerFactory processFactory, IFileSystemService fileSystemService) {
            this.factory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        #endregion

        /// <summary>
        /// Merges specified audio and video files.
        /// </summary>
        /// <param name="videoFile">The file containing the video.</param>
        /// <param name="audioFile">The file containing the audio.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Muxe(string videoFile, string audioFile, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            if (string.IsNullOrEmpty(videoFile) && string.IsNullOrEmpty(audioFile))
                throw new ArgumentException("You must specifiy either videoFile or audioFile.", nameof(videoFile));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination cannot be null or empty.", nameof(destination));

            List<FFmpegStream> InputStreamList = new List<FFmpegStream>();
            FFmpegStream InputStream;
            if (!string.IsNullOrEmpty(videoFile)) {
                InputStream = GetStreamInfo(videoFile, FFmpegStreamType.Video, options);
                if (InputStream != null)
                    InputStreamList.Add(InputStream);
            }
            if (!string.IsNullOrEmpty(audioFile)) {
                InputStream = GetStreamInfo(audioFile, FFmpegStreamType.Audio, options);
                if (InputStream != null)
                    InputStreamList.Add(InputStream);
            }

            if (InputStreamList.Any())
                return Muxe(InputStreamList, destination, options, callback);
            else
                return CompletionStatus.Failed;
        }

        /// <summary>
        /// Merges the specified list of file streams.
        /// </summary>
        /// <param name="fileStreams">The list of file streams to include in the output.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Muxe(IEnumerable<FFmpegStream> fileStreams, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            if (fileStreams == null || !fileStreams.Any())
                throw new ArgumentException("FileStreams cannot be null or empty.", nameof(fileStreams));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination cannot be null or empty.", nameof(destination));

            CompletionStatus Result = CompletionStatus.Success;
            List<string> TempFiles = new List<string>();
            fileSystem.Delete(destination);

            // FFMPEG fails to muxe H264 into MKV container. Converting to MP4 and then muxing with the audio, however, works.
            foreach (FFmpegStream item in fileStreams) {
                if (string.IsNullOrEmpty(item.Path) && item.Type != FFmpegStreamType.None)
                    throw new ArgumentException("FFmpegStream.Path cannot be null or empty.", nameof(item.Path));
                if (item.Type == FFmpegStreamType.Video && (item.Format == "h264" || item.Format == "h265") && destination.EndsWith(".mkv")) {
                    string NewFile = item.Path.Substring(0, item.Path.LastIndexOf('.')) + ".mp4";
                    Result = Muxe(new List<FFmpegStream>() { item }, NewFile, options);
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
                StringBuilder AacFix = new StringBuilder();
                var FileStreamsOrdered = fileStreams.OrderBy(f => f.Type);
                foreach (FFmpegStream item in FileStreamsOrdered) {
                    if (item.Type == FFmpegStreamType.Video)
                        HasVideo = true;
                    if (item.Type == FFmpegStreamType.Audio) {
                        HasAudio = true;
                        if (item.Format == "aac")
                            AacFix.AppendFormat("-bsf:{0} aac_adtstoasc ", StreamIndex);
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
                if (!HasVideo && !HasAudio)
                    throw new ArgumentException("FileStreams cannot be empty.", nameof(fileStreams));
                if (HasVideo)
                    Query.Append("-vcodec copy ");
                if (HasAudio)
                    Query.Append(HasPcmDvdAudio ? "-acodec pcm_s16le " : "-acodec copy ");
                Query.Append(Map);
                // FFMPEG-encoded AAC streams are invalid and require an extra flag to join.
                if (AacFix.Length > 0 && HasVideo)
                    Query.Append(AacFix);
                Query.Append("\"");
                Query.Append(destination);
                Query.Append("\"");
                IProcessManagerFFmpeg Worker = factory.CreateFFmpeg(options, callback);
                Result = Worker.RunFFmpeg(Query.ToString());
            }

            // Delete temp file.
            foreach (string item in TempFiles) {
                fileSystem.Delete(item);
            }
            return Result;
        }

        /// <summary>
        /// Returns stream information as FFmpegStream about specified media file that can be used to call a muxing operation.
        /// </summary>
        /// <param name="path">The path of the media file to query.</param>
        /// <param name="streamType">The type of media stream to query.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <returns>A FFmpegStream object.</returns>
        private FFmpegStream GetStreamInfo(string path, FFmpegStreamType streamType, ProcessOptionsFFmpeg options) {
            IProcessManagerFFmpeg Worker = factory.CreateFFmpeg(options);
            Worker.RunFFmpeg(string.Format(@"-i ""{0}""", path));
            FFmpegStreamInfo StreamInfo = Worker.FileStreams?.FirstOrDefault(x => x.StreamType == streamType);
            if (StreamInfo != null) {
                return new FFmpegStream() {
                    Path = path,
                    Index = StreamInfo.Index,
                    Format = StreamInfo.Format,
                    Type = streamType
                };
            } else
                return null;
        }

        /// <summary>
        /// Extracts the video stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus ExtractVideo(string source, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            return ExtractStream(@"-y -i ""{0}"" -vcodec copy -an ""{1}""", source, destination, options, callback);
        }

        /// <summary>
        /// Extracts the audio stream from specified file.
        /// </summary>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus ExtractAudio(string source, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            return ExtractStream(@"-y -i ""{0}"" -vn -acodec copy ""{1}""", source, destination, options, callback);
        }

        /// <summary>
        /// Extracts an audio or video stream from specified file.
        /// </summary>
        /// <param name="args">The arguments string that will be passed to FFmpeg.</param>
        /// <param name="source">The media file to extract from.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        private CompletionStatus ExtractStream(string args, string source, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be empty.", nameof(source));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination cannot be empty.", nameof(destination));
            fileSystem.Delete(destination);
            IProcessManagerFFmpeg Worker = factory.CreateFFmpeg(options, callback);
            return Worker.RunFFmpeg(string.Format(args, source, destination));
        }

        /// <summary>
        /// Concatenates (merges) all specified files.
        /// </summary>
        /// <param name="files">The files to merge.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Concatenate(IEnumerable<string> files, string destination, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            if (files == null || !files.Any())
                throw new ArgumentException("Files cannot be null or empty.", nameof(files));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination cannot be null or empty.", nameof(destination));
            CompletionStatus Result = CompletionStatus.None;

            // Write temp file.
            string TempFile = fileSystem.GetTempFile();
            StringBuilder TempContent = new StringBuilder();
            foreach (string item in files) {
                TempContent.AppendFormat("file '{0}'", item).AppendLine();
            }
            fileSystem.WriteAllText(TempFile, TempContent.ToString());

            string Query = string.Format(@"-y -f concat -fflags +genpts -async 1 -safe 0 -i ""{0}"" -c copy ""{1}""", TempFile, destination);

            IProcessManagerFFmpeg Worker = factory.CreateFFmpeg(options, callback);
            Result = Worker.RunFFmpeg(Query.ToString());

            fileSystem.Delete(TempFile);
            return Result;
        }

        /// <summary>
        /// Truncates a media file from specified start position with specified duration.
        /// </summary>
        /// <param name="source">The source file to truncate.</param>
        /// <param name="destination">The output file to write.</param>
        /// <param name="startPos">The position where to start copying. Anything before this position will be ignored. TimeSpan.Zero or null to start from beginning.</param>
        /// <param name="duration">The duration after which to stop copying. Anything after this duration will be ignored. TimeSpan.Zero or null to copy until the end.</param>
        /// <param name="options">The options for starting the process.</param>
        /// <param name="callback">A method that will be called after the process has been started.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Truncate(string source, string destination, TimeSpan? startPos, TimeSpan? duration = null, ProcessOptionsFFmpeg options = null, ProcessStartedEventHandler callback = null) {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentException("Source cannot be empty.", nameof(source));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination cannot be empty.", nameof(destination));
            fileSystem.Delete(destination);
            IProcessManagerFFmpeg Worker = factory.CreateFFmpeg(options, callback);
            string Args = string.Format(@"-i ""{0}"" -vcodec copy -acodec copy {1}{2}""{3}""", source,
                startPos.HasValue && startPos > TimeSpan.Zero ? string.Format("-ss {0:c} ", startPos) : "",
                duration.HasValue && duration > TimeSpan.Zero ? string.Format("-t {0:c} ", duration) : "",
                destination);
            return Worker.RunFFmpeg(Args);
        }
    }
}

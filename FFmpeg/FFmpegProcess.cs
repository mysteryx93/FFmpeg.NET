using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Management;
using System.Runtime.InteropServices;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Executes commands through FFmpeg assembly.
    /// </summary>
    public class FFmpegProcess {
        /// <summary>
        /// Gets or sets the options to control the behaviors of the process.
        /// </summary>
        public ProcessStartOptions Options { get; set; } = new ProcessStartOptions();
        /// <summary>
        /// Gets the process currently being executed.
        /// </summary>
        public Process WorkProcess { get; private set; }
        /// <summary>
        /// Occurs when the application writes to its output stream.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;
        /// <summary>
        /// Occurs after stream info is read from FFmpeg's output.
        /// </summary>
        public event EventHandler InfoUpdated;
        /// <summary>
        /// Occurs when status update is received through FFmpeg's output stream.
        /// </summary>
        public event StatusUpdatedEventHandler StatusUpdated;
        /// <summary>
        /// Occurs when the process has terminated its work.
        /// </summary>
        public event CompletedEventHandler Completed;
        /// <summary>
        /// Returns the raw console output from FFmpeg.
        /// </summary>
        public string Output {
            get { return output.ToString(); }
        }
        /// <summary>
        /// Returns the duration of input file.
        /// </summary>
        public TimeSpan FileDuration { get; private set; }
        /// <summary>
        /// Returns the frame count of input file (estimated).
        /// </summary>
        public long FrameCount { get; private set; }
        /// <summary>
        /// Returns information about input streams.
        /// </summary>
        public List<FFmpegStreamInfo> FileStreams { get; private set; }
        /// <summary>
        /// Returns the CompletionStatus of the last operation.
        /// </summary>
        public CompletionStatus LastCompletionStatus { get; private set; }

        private StringBuilder output;
        private bool isFFmpeg;
        private bool isStarted;
        private CancellationTokenSource cancelWork;

        /// <summary>
        /// Initializes a new instances of the FFmpegProcess class.
        /// </summary>
        public FFmpegProcess() {
        }

        /// <summary>
        /// Initializes a new instances of the FFmpegProcess class with specified options.
        /// </summary>
        /// <param name="options">The options for starting the process.</param>
        public FFmpegProcess(ProcessStartOptions options) {
            Options = options ?? new ProcessStartOptions();
        }

        /// <summary>
        /// Runs FFmpeg with specified arguments.
        /// </summary>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunFFmpeg(string arguments) {
            return Run(FFmpegConfig.FFmpegPath, arguments, true, false);
        }

        /// <summary>
        /// Runs FFmpeg with specified arguments through avs2yuv.
        /// </summary>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAvisynthToFFmpeg(string source, string arguments) {
            if (!File.Exists(FFmpegConfig.Avs2yuvPath))
                throw new FileNotFoundException(string.Format(@"File ""{0}"" specified by FFmpegConfig.Avs2yuvPath is not found.", FFmpegConfig.FFmpegPath));
            String Query = string.Format(@"""{0}"" ""{1}"" -o - | ""{2}"" {3}", FFmpegConfig.Avs2yuvPath, source, FFmpegConfig.FFmpegPath, arguments);
            return RunAsCommand(Query, true);
        }

        /// <summary>
        /// Runs avs2yuv with specified source file. The output will be discarded.
        /// </summary>
        /// <param name="path">The path to the script to run.</param>
        public void RunAvisynth(string path) {
            string TempFile = path + ".out";
            string Args = string.Format(@"""{0}"" -o {1}", path, TempFile);
            CompletionStatus Result = Run("Encoder\\avs2yuv.exe", Args, false, false);
            File.Delete(TempFile);
        }

        /// <summary>
        /// Runs the command as 'cmd /c', allowing the use of command line features such as piping.
        /// </summary>
        /// <param name="cmd">The full command to be executed with arguments.</param>
        /// <param name="isFFmpeg">Whether to parse the output from FFmpeg.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus RunAsCommand(string cmd, bool isFFmpeg) {
            return Run("cmd", string.Format(@" /c "" {0} """, cmd), isFFmpeg, true);
        }

        /// <summary>
        /// Runs specified application with specified arguments.
        /// </summary>
        /// <param name="fileName">The application to start.</param>
        /// <param name="arguments">The set of arguments to use when starting the application.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Run(string fileName, string arguments) {
            return Run(fileName, arguments, false, false);
        }

        /// <summary>
        /// Runs specified application with specified arguments.
        /// </summary>
        /// <param name="fileName">The application to start.</param>
        /// <param name="arguments">The set of arguments to use when starting the application.</param>
        /// <param name="isFFmpeg">Whether to parse the output as FFmpeg.</param>
        /// <param name="nestedProcess">If true, killing the process with kill all sub-processes.</param>
        /// <returns>The process completion status.</returns>
        public CompletionStatus Run(string fileName, string arguments, bool isFFmpeg, bool nestedProcess) {
            if (!File.Exists(FFmpegConfig.FFmpegPath))
                throw new FileNotFoundException(string.Format(@"File ""{0}"" specified by FFmpegConfig.FFmpegPath is not found.", FFmpegConfig.FFmpegPath));
            if (WorkProcess != null)
                throw new InvalidOperationException("This instance of FFmpeg is busy. You can run concurrent commands by creating other class instances.");

            Process P = new Process();
            this.isFFmpeg = isFFmpeg;
            WorkProcess = P;
            output = new StringBuilder();
            isStarted = false;
            FileStreams = null;
            FrameCount = 0;
            FileDuration = TimeSpan.Zero;
            cancelWork = new CancellationTokenSource();
            if (Options == null)
                Options = new ProcessStartOptions();

            P.StartInfo.FileName = fileName;
            P.StartInfo.Arguments = arguments;
            P.OutputDataReceived += FFmpeg_DataReceived;
            P.ErrorDataReceived += FFmpeg_DataReceived;

            if (Options.DisplayMode != FFmpegDisplayMode.Native) {
                if (Options.DisplayMode == FFmpegDisplayMode.Interface && FFmpegConfig.UserInterfaceManager != null)
                    FFmpegConfig.UserInterfaceManager.Display(this);
                P.StartInfo.CreateNoWindow = true;
                P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                P.StartInfo.RedirectStandardError = true;
                P.StartInfo.UseShellExecute = false;
            }

            Options.RaiseStarted(this);

            P.Start();
            try {
                if (!P.HasExited)
                    P.PriorityClass = Options.Priority;
            }
            catch { }
            P.BeginErrorReadLine();

            bool Timeout = Wait();

            // ExitCode is 0 for normal exit. Different value when closing the console.
            CompletionStatus Result = Timeout ? CompletionStatus.Timeout : cancelWork.IsCancellationRequested ? CompletionStatus.Cancelled : P.ExitCode == 0 ? CompletionStatus.Success : CompletionStatus.Error;

            isStarted = false;
            cancelWork = null;
            LastCompletionStatus = Result;
            Completed?.Invoke(this, new CompletedEventArgs(Result));
            if ((Result == CompletionStatus.Error || Result == CompletionStatus.Timeout) && Options.DisplayMode == FFmpegDisplayMode.ErrorOnly)
                FFmpegConfig.UserInterfaceManager?.DisplayError(this);
            return Result;
        }

        /// <summary>
        /// Waits for the process to terminate while allowing the cancellation token to kill the process.
        /// </summary>
        /// <returns>Whether a timeout occured.</returns>
        private bool Wait() {
            DateTime StartTime = DateTime.Now;
            while (!WorkProcess.HasExited) {
                if (cancelWork.Token.IsCancellationRequested && !WorkProcess.HasExited)
                    MediaProcesses.SoftKill(WorkProcess);
                if (Options.Timeout > TimeSpan.Zero && DateTime.Now - StartTime > Options.Timeout) {
                    MediaProcesses.SoftKill(WorkProcess);
                    return true;
                }
                WorkProcess.WaitForExit(500);
            }
            WorkProcess.WaitForExit();
            return false;


            //using (var waitHandle = new SafeWaitHandle(WorkProcess.Handle, false)) {
            //    using (var processFinishedEvent = new ManualResetEvent(false)) {
            //        processFinishedEvent.SafeWaitHandle = waitHandle;

            //        int index = WaitHandle.WaitAny(new[] { processFinishedEvent, cancelWork.Token.WaitHandle }, Options.Timeout);
            //        if (index > 0) {
            //            if (nestedProcess)
            //                KillProcessAndChildren(WorkProcess.Id);
            //            else if (!WorkProcess.HasExited)
            //                WorkProcess.Kill();
            //        }
            //        WorkProcess.WaitForExit();
            //        return (index == WaitHandle.WaitTimeout);
            //    }
            //}
        }

        /// <summary>
        /// Cancels the currently running job and terminate its process.
        /// </summary>
        public void Cancel() {
            cancelWork?.Cancel();
        }

        /// <summary>
        /// Occurs when data is received from the executing application.
        /// </summary>
        private void FFmpeg_DataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data == null) {
                if (!isStarted && isFFmpeg)
                    ParseFileInfo();
                return;
            }

            output.AppendLine(e.Data);
            DataReceived?.Invoke(sender, e);

            if (isFFmpeg) {
                if (FileStreams == null && (e.Data.StartsWith("Output ") || e.Data.StartsWith("Press [q] to stop")))
                    ParseFileInfo();
                if (e.Data.StartsWith("Press [q] to stop") || e.Data.StartsWith("frame="))
                    isStarted = true;

                if (isStarted && e.Data.StartsWith("frame=")) {
                    FFmpegStatus ProgressInfo = FFmpegParser.ParseProgress(e.Data);
                    StatusUpdated?.Invoke(this, new StatusUpdatedEventArgs(ProgressInfo));
                }
            }
        }

        private void ParseFileInfo() {
            TimeSpan fileDuration;
            FileStreams = FFmpegParser.ParseFileInfo(output.ToString(), out fileDuration);
            FileDuration = fileDuration;
            if (Options.FrameCount > 0)
                FrameCount = Options.FrameCount;
            else if (VideoStream != null)
                FrameCount = (int)(FileDuration.TotalSeconds * VideoStream.FrameRate);
            InfoUpdated?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Gets the first video stream from FileStreams.
        /// </summary>
        /// <returns>A FFmpegVideoStreamInfo object.</returns>
        public FFmpegVideoStreamInfo VideoStream {
            get {
                if (FileStreams != null && FileStreams.Count > 0)
                    return (FFmpegVideoStreamInfo)FileStreams.FirstOrDefault(f => f.StreamType == FFmpegStreamType.Video);
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the first audio stream from FileStreams.
        /// </summary>
        /// <returns>A FFmpegAudioStreamInfo object.</returns>
        public FFmpegAudioStreamInfo AudioStream {
            get {
                if (FileStreams != null && FileStreams.Count > 0)
                    return (FFmpegAudioStreamInfo)FileStreams.FirstOrDefault(f => f.StreamType == FFmpegStreamType.Audio);
                else
                    return null;
            }
        }

        /// <summary>
        /// Returns the full command with arguments being run.
        /// </summary>
        public string CommandWithArgs {
            get {
                return string.Format(@"""{0}"" {1}", WorkProcess.StartInfo.FileName, WorkProcess.StartInfo.Arguments);
            }
        }
    }
}

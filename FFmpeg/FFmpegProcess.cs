using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace EmergenceGuardian.FFmpeg {
    /// <summary>
    /// Executes commands through FFmpeg assembly.
    /// </summary>
    public class FFmpegProcess {
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
        /// Occurs when progress update is received through FFmpeg's output stream.
        /// </summary>
        public event ProgressUpdatedEventHandler ProgressUpdated;
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
        /// Returns information about input streams.
        /// </summary>
        public List<FFmpegStreamInfo> FileStreams { get; private set; }

        private StringBuilder output;
        private bool isStarted;
        private CancellationTokenSource cancelWork;
        private List<KeyValuePair<DateTime, TimeSpan>> progressHistory;

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
            Options = options;
        }

        /// <summary>
        /// Runs FFmpeg with specified arguments.
        /// </summary>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <returns>The process completion status.</returns>
        public CompletedStatus RunFFmpeg(string arguments) {
            return Run(FFmpegConfig.FFmpegPath, arguments);
        }

        /// <summary>
        /// Runs FFmpeg with specified arguments through avs2yuv.
        /// </summary>
        /// <param name="arguments">FFmpeg startup arguments.</param>
        /// <returns>The process completion status.</returns>
        public CompletedStatus RunAvisynthToFFmpeg(string source, string arguments) {
            if (!File.Exists(FFmpegConfig.Avs2yuvPath))
                throw new FileNotFoundException(string.Format(@"File ""{0}"" specified by FFmpegConfig.Avs2yuvPath is not found.", FFmpegConfig.FFmpegPath));
            String Query = string.Format(@"""{0}"" ""{1}"" -o - | ""{2}"" {3}", FFmpegConfig.Avs2yuvPath, source, FFmpegConfig.FFmpegPath, arguments);
            return RunAsCommand(Query);
        }

        /// <summary>
        /// Runs the command as 'cmd /c', allowing the use of command line features such as piping.
        /// </summary>
        /// <param name="cmd">The full command to be executed with arguments.</param>
        /// <returns>The process completion status.</returns>
        public CompletedStatus RunAsCommand(string cmd) {
            return Run("cmd", string.Format(@" /c "" {0} """, cmd));
        }

        /// <summary>
        /// Runs specified application with specified arguments.
        /// </summary>
        /// <param name="fileName">The application to start.</param>
        /// <param name="arguments">The set of arguments to use when starting the application.</param>
        /// <returns>The process completion status.</returns>
        public CompletedStatus Run(string fileName, string arguments) {
            if (!File.Exists(FFmpegConfig.FFmpegPath))
                throw new FileNotFoundException(string.Format(@"File ""{0}"" specified by FFmpegConfig.FFmpegPath is not found.", FFmpegConfig.FFmpegPath));
            if (WorkProcess != null)
                throw new InvalidOperationException("This instance of FFmpeg is busy. You can run concurrent commands by creating other class instances.");

            Process P = new Process();
            WorkProcess = P;
            output = new StringBuilder();
            isStarted = false;
            FileStreams = null;
            cancelWork = new CancellationTokenSource();
            progressHistory = new List<KeyValuePair<DateTime, TimeSpan>>();

            P.StartInfo.FileName = fileName;
            P.StartInfo.Arguments = arguments;
            P.OutputDataReceived += FFmpeg_DataReceived;
            P.ErrorDataReceived += FFmpeg_DataReceived;

            if (Options.DisplayMode != FFmpegDisplayMode.Native) {
                if (Options.DisplayMode == FFmpegDisplayMode.Interface && FFmpegConfig.UserInterfaceManager != null)
                    FFmpegConfig.UserInterfaceManager.CreateInstance(this);
                P.StartInfo.CreateNoWindow = true;
                P.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                P.StartInfo.RedirectStandardError = true;
                P.StartInfo.UseShellExecute = false;
            }

            P.Start();
            try {
                if (!P.HasExited)
                    P.PriorityClass = Options.Priority;
            }
            catch { }
            P.BeginErrorReadLine();

            // Wait() allows using the CancellationToken but execution still continues.
            Wait();
            P.WaitForExit();

            // ExitCode is 0 for normal exit. Different value when closing the console.
            CompletedStatus Result = cancelWork.IsCancellationRequested ? CompletedStatus.Cancelled : P.ExitCode == 0 ? CompletedStatus.Success : CompletedStatus.Error;

            WorkProcess = null;
            isStarted = false;
            cancelWork = null;
            progressHistory = null;
            Completed?.Invoke(this, new CompletedEventArgs(Result));
            if (Result == CompletedStatus.Error && Options.DisplayMode == FFmpegDisplayMode.ErrorOnly)
                FFmpegConfig.UserInterfaceManager?.DisplayError(Options.DisplayTitle, Output);
            return Result;
        }

        /// <summary>
        /// Waits for the process to terminate while allowing the cancellation token to kill the process.
        /// </summary>
        private void Wait() {
            using (var waitHandle = new SafeWaitHandle(WorkProcess.Handle, false)) {
                using (var processFinishedEvent = new ManualResetEvent(false)) {
                    processFinishedEvent.SafeWaitHandle = waitHandle;

                    int index = WaitHandle.WaitAny(new[] { processFinishedEvent, cancelWork.Token.WaitHandle });
                    if (index == 1) {
                        WorkProcess.Kill();
                        //throw new OperationCanceledException();
                    }
                }
            }
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
                if (!isStarted)
                    ParseFileInfo();
                return;
            }

            output.AppendLine(e.Data);
            DataReceived?.Invoke(sender, e);

            if (FileStreams == null && (e.Data.StartsWith("Output ") || e.Data.StartsWith("Press [q] to stop")))
                ParseFileInfo();
            if (e.Data.StartsWith("Press [q] to stop"))
                isStarted = true;

            if (isStarted && e.Data.StartsWith("frame=")) {
                FFmpegProgress ProgressInfo = FFmpegParser.ParseProgress(e.Data);
                CalculateTimeLeft(ProgressInfo);
                ProgressUpdated?.Invoke(this, new ProgressUpdatedEventArgs(ProgressInfo));
            }
        }

        private void ParseFileInfo() {
            TimeSpan fileDuration;
            FileStreams = FFmpegParser.ParseFileInfo(output.ToString(), out fileDuration);
            FileDuration = fileDuration;
            InfoUpdated?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Calculates the time left.
        /// </summary>
        /// <param name="progress">An object containing the latest progress info.</param>
        private void CalculateTimeLeft(FFmpegProgress progress) {
            // progressHistory contains the last 5 progress values.
            progressHistory.Add(new KeyValuePair<DateTime, TimeSpan>(DateTime.Now, progress.Time));
            if (progressHistory.Count > 5)
                progressHistory.RemoveAt(0);
            if (progressHistory.Count > 1) {
                TimeSpan SampleWorkTime = progressHistory.Last().Key - progressHistory.First().Key;
                TimeSpan SampleFileTime = progressHistory.Last().Value - progressHistory.First().Value;
                double ProcessingTimePerSecond = SampleWorkTime.TotalSeconds / SampleFileTime.TotalSeconds;
                TimeSpan WorkLeft = FileDuration - progress.Time;
                progress.EstimatedTimeLeft = TimeSpan.FromSeconds(WorkLeft.TotalSeconds * ProcessingTimePerSecond);
            }
        }
    }
}

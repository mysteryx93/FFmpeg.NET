using System;
using System.Windows;
using System.Windows.Media;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.FFmpegExampleApplication {
    /// <summary>
    /// Interaction logic for FFmpegWindow.xaml
    /// </summary>
    public partial class FFmpegWindow : Window, IUserInterfaceWindow {
        public static FFmpegWindow Instance(Window parent, string title, bool autoClose) {
            FFmpegWindow F = new FFmpegWindow();
            F.Owner = parent;
            F.title = title;
            F.autoClose = autoClose;
            F.Show();
            return F;
        }

        protected IProcessManager host;
        protected IProcessManagerFFmpeg hostFFmpeg;
        protected IProcessManager task;
        protected bool autoClose;
        protected string title { get; set; }
        protected ITimeLeftCalculator timeCalc;

        public void Stop() => Dispatcher.Invoke(() => this.Close());

        public FFmpegWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            SetPageTitle(null);
        }

        public void DisplayTask(IProcessManager taskArg) {
            Dispatcher.Invoke(() => {
                if (taskArg.Options.IsMainTask) {
                    host = taskArg;
                    hostFFmpeg = host as IProcessManagerFFmpeg;
                    if (hostFFmpeg != null) {
                        hostFFmpeg.InfoUpdated += FFmpeg_InfoUpdated;
                        hostFFmpeg.StatusUpdated += FFmpeg_StatusUpdated;
                    }
                    host.ProcessCompleted += FFmpeg_Completed;
                    PercentText.Text = 0.ToString("p1");
                    SetPageTitle(PercentText.Text);
                } else {
                    task = taskArg;
                    TaskStatusText.Text = task.Options.Title;
                    task.ProcessCompleted += (sender, e) => {
                        ProcessManager Proc = (ProcessManager)sender;
                        Dispatcher.Invoke(() => {
                            if (e.Status == CompletionStatus.Failed && !Proc.WorkProcess.StartInfo.FileName.EndsWith("avs2yuv.exe"))
                                FFmpegErrorWindow.Instance(Owner, Proc);
                            TaskStatusText.Text = "";
                            task = null;
                            if (autoClose)
                                this.Close();
                        });
                    };
                }
            });
        }

        protected long ResumePos => hostFFmpeg.Options?.ResumePos ?? 0;

        private void SetPageTitle(string status) {
            this.Title = string.IsNullOrEmpty(status) ? title : string.Format("{0} ({1})", title, status);
        }

        private void FFmpeg_InfoUpdated(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                WorkProgressBar.Maximum = hostFFmpeg.FrameCount + ResumePos;
                timeCalc = new TimeLeftCalculator(hostFFmpeg.FrameCount + hostFFmpeg?.Options.ResumePos ?? 0);
            });
        }

        private bool EstimatedTimeLeftToggle = false;
        private void FFmpeg_StatusUpdated(object sender, FFmpeg.StatusUpdatedEventArgs e) {
            Dispatcher.Invoke(() => {
                WorkProgressBar.Value = e.Status.Frame + ResumePos;
                PercentText.Text = (WorkProgressBar.Value / WorkProgressBar.Maximum).ToString("p1");
                SetPageTitle(PercentText.Text);
                FpsText.Text = e.Status.Fps.ToString();

                // Time left will be updated only 1 out of 2 to prevent changing too quick.
                EstimatedTimeLeftToggle = !EstimatedTimeLeftToggle;
                if (EstimatedTimeLeftToggle) {
                    timeCalc?.Calculate(e.Status.Frame + ResumePos);
                    TimeSpan TimeLeft = timeCalc.ResultTimeLeft;
                    if (TimeLeft > TimeSpan.Zero)
                        TimeLeftText.Text = TimeLeft.ToString(TimeLeft.TotalHours < 1 ? "m\\:ss" : "h\\:mm\\:ss");
                }
            });
        }

        private void FFmpeg_Completed(object sender, FFmpeg.ProcessCompletedEventArgs e) {
            Dispatcher.Invoke(() => {
                ProcessManager Proc = sender as ProcessManager;
                if (e.Status == CompletionStatus.Failed && !Proc.WorkProcess.StartInfo.FileName.EndsWith("avs2yuv.exe"))
                    FFmpegErrorWindow.Instance(Owner, Proc);
                if (autoClose)
                    this.Close();
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            host?.Cancel();
            task?.Cancel();
        }
    }
}

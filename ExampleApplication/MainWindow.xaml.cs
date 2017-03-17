using System;
using System.IO;
using System.Windows;
using System.Threading.Tasks;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.FFmpegExampleApplication {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            FFmpegConfig.FFmpegPath = @"F:\AVSMeter\ffmpeg.exe";
            FFmpegConfig.UserInterfaceManager = new FFmpegUserInterfaceManager(this);
        }

        //private async void RunButton_Click(object sender, RoutedEventArgs e) {
        //    FFmpegProcess Worker = new FFmpegProcess();
        //    Worker.Options.DisplayMode = FFmpegDisplayMode.Interface;
        //    //string Args = ArgTextBox.Text;
        //    //await Task.Run(() => Worker.RunFFmpeg(Args)).ConfigureAwait(false);
        //}

        private void BrowseSource_Click(object sender, RoutedEventArgs e) {
            string Result = ShowFileDialog(SourceDirectory, null);
            if (Result != null)
                SourceTextBox.Text = Result;
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e) {
            string Result = ShowSaveFileDialog(DestinationDirectory, "MP4 Files|*.mp4");
            if (Result != null)
                DestinationTextBox.Text = Result;
        }

        private string SourceDirectory {
            get {
                try {
                    return Path.GetDirectoryName(SourceTextBox.Text);
                }
                catch {
                    return null;
                }
            }
        }

        private string DestinationDirectory {
            get {
                try {
                    return Path.GetDirectoryName(DestinationTextBox.Text);
                }
                catch {
                    return null;
                }
            }
        }

        public string ShowFileDialog(string defaultPath, string filter) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            try {
                if (!string.IsNullOrEmpty(defaultPath))
                    dlg.InitialDirectory = Path.GetDirectoryName(defaultPath);
            }
            catch { }
            dlg.Filter = filter;
            if (dlg.ShowDialog().Value == true) {
                return dlg.FileName;
            } else
                return null;
        }

        public static string ShowSaveFileDialog(string defaultPath, string filter) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            try {
                if (!string.IsNullOrEmpty(defaultPath))
                    dlg.InitialDirectory = Path.GetDirectoryName(defaultPath);
            }
            catch { }
            dlg.Filter = filter;
            if (dlg.ShowDialog().Value == true) {
                return dlg.FileName;
            } else
                return null;
        }

        private bool Validate() {
            return !string.IsNullOrEmpty(SourceTextBox.Text) && File.Exists(SourceTextBox.Text) && !string.IsNullOrEmpty(DestinationTextBox.Text) && SourceDirectory != null && DestinationDirectory != null;
        }

        private async void RunSimpleButton_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                ProcessStartOptions Options = new ProcessStartOptions(FFmpegDisplayMode.Interface, "Encoding to H264/AAC (Simple)");
                string Src = SourceTextBox.Text;
                string Dst = DestinationTextBox.Text;
                await Task.Run(() => {
                    MediaEncoder.Encode(Src, "h264", "aac", null, Dst, Options);
                });
            }
        }

        private static int jobId = 0;
        private async void RunComplexButton_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                string Src = SourceTextBox.Text;
                string Dst = DestinationTextBox.Text;
                CompletionStatus Result = await Task.Run(() => ExecuteComplex(Src, Dst));
                MessageBox.Show(Result.ToString(), "Encoding Result");
            }
        }

        private CompletionStatus ExecuteComplex(string src, string dst) {
            string DstEncode = GetPathWithoutExtension(dst) + "_.mp4";
            string DstExtract = GetPathWithoutExtension(dst) + "_.mkv";
            string DstAac = GetPathWithoutExtension(dst) + "_.aac";
            jobId++;
            CompletionStatus Result;

            FFmpegConfig.UserInterfaceManager.Start(jobId, "Encoding to H264/AAC (Complex)");

            ProcessStartOptions OptionsMain = new ProcessStartOptions(jobId, "", true);
            FFmpegProcess ProcessMain = null;
            OptionsMain.Started += (sender, e) => {
                ProcessMain = e.Process;
            };
            Task<CompletionStatus> TaskMain = Task.Run(() => MediaEncoder.Encode(src, "h264", null, "", DstEncode, OptionsMain));

            ProcessStartOptions Options = new ProcessStartOptions(jobId, "Extracting Audio", false);
            Result = MediaMuxer.ExtractAudio(src, DstExtract, Options);
            if (Result == CompletionStatus.Success) {
                Options.Title = "Encoding Audio";
                Result = MediaEncoder.Encode(DstExtract, null, "aac", null, DstAac, Options);
            }

            if (Result != CompletionStatus.Success)
                ProcessMain?.Cancel();

            TaskMain.Wait();
            CompletionStatus Result2 = TaskMain.Result;

            if (Result == CompletionStatus.Success && Result2 == CompletionStatus.Success) {
                Options.Title = "Muxing Audio and Video";
                Result = MediaMuxer.Muxe(DstEncode, DstAac, dst, Options);
            }

            File.Delete(DstEncode);
            File.Delete(DstExtract);
            File.Delete(DstAac);
            FFmpegConfig.UserInterfaceManager.Stop(jobId);
            return Result;
        }

        public static string GetPathWithoutExtension(string path) {
            int Pos = path.LastIndexOf('.');
            return Pos == -1 ? path : path.Substring(0, Pos);
        }
    }
}

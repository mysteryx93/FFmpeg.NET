using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            FFmpegConfig.UserInterfaceManager = new UserInterfaceManager();
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e) {
            FFmpegProcess Worker = new FFmpegProcess();
            Worker.Options.DisplayMode = FFmpegDisplayMode.Interface;
            string Args = ArgTextBox.Text;
            await Task.Run(() => Worker.RunFFmpeg(Args)).ConfigureAwait(false);
        }
    }
}

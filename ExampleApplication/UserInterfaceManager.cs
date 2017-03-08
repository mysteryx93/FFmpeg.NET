using System;
using System.Windows;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.FFmpegExampleApplication {
    public class UserInterfaceManager : IUserInterfaceManager {
        public void CreateInstance(FFmpegProcess host) {
            Application.Current.Dispatcher.Invoke(() => FFmpegWindow.Instance(host));            
        }

        public void DisplayError(string displayTitle, string output) {
            Application.Current.Dispatcher.Invoke(() => ErrorWindow.Instance(displayTitle, output));
        }
    }
}

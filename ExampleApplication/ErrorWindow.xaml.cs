using System;
using System.Windows;

namespace EmergenceGuardian.FFmpegExampleApplication {
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window {
        public static void Instance(string displayTitle, string log) {
            ErrorWindow F = new ErrorWindow();
            F.Title = "Failed: " + displayTitle;
            F.OutputText.Text = log;
            F.Show();
        }

        public ErrorWindow() {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}

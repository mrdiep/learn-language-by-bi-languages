using MahApps.Metro.Controls;

namespace VoiceSubtitle
{
    public partial class StartPage : MetroWindow
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void OpenFlyout(object sender, System.Windows.RoutedEventArgs e)
        {
            LeftFlyout.IsOpen = true;
        }

        private void OpenPropertyFlyout(object sender, System.Windows.RoutedEventArgs e)
        {
            PropertyFlyout.IsOpen = true;
        }
    }
}
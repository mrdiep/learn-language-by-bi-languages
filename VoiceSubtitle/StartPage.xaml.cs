using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using Microsoft.Practices.ServiceLocation;
using System.Windows;
using System.Windows.Controls;
using VoiceSubtitle.Model;
using VoiceSubtitle.ViewModel;

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

        private void StartSub(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (string.IsNullOrWhiteSpace(newCaption.VideoName))
            {
                MessageBox.Show("Please enter Video Name", App.AppTitle);
                return;
            }

            if (string.IsNullOrWhiteSpace(newCaption.VideoPath))
            {
                MessageBox.Show("Please drop Video Path", App.AppTitle);
                return;
            }

            if (string.IsNullOrWhiteSpace(newCaption.SubEngPath))
            {
                MessageBox.Show("Please drop English Subtitle", App.AppTitle);
                return;
            }

            if (string.IsNullOrWhiteSpace(newCaption.SubOtherPath))
            {
                MessageBox.Show("Please drop Other Subtitle", App.AppTitle);
                return;
            }

            string content = $"{newCaption.VideoName}\r\n{newCaption.VideoPath}\r\n{newCaption.SubEngPath}\r\n{newCaption.SubOtherPath}";
            var source = new SourcePath()
            {
                VideoName = newCaption.VideoName,
                PrimaryCaption = newCaption.SubEngPath,
                TranslatedCaption = newCaption.SubOtherPath,
                Video = newCaption.VideoPath
            };

            if ((button.Tag as string) == "saveAndStart")
            {
                ServiceLocator.Current.GetInstance<MainViewModel>().AddNewSource(ref source);
            }

            ServiceLocator.Current.GetInstance<PlayerViewModel>().SwitchSource.Execute(source);
        }

        private void OpenSettingFlyout(object sender, RoutedEventArgs e)
        {
            SettingFlyout.IsOpen = true;
        }

        private void SettingFlyout_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send(SettingFlyout.IsOpen || CaptionSeacherFlyout.IsOpen, "IsSettingFlyoutOpenToken");
        }
    }
}
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
            Closed += StartPage_Closed;
        }

        private void StartPage_Closed(object sender, System.EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void StartSub(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (string.IsNullOrWhiteSpace(newCaption.VideoName))
            {
                ServiceLocator.Current.GetInstance<NotifyViewModel>().MessageBox("Please enter Video Name");
                return;
            }

            if (string.IsNullOrWhiteSpace(newCaption.VideoPath))
            {
                ServiceLocator.Current.GetInstance<NotifyViewModel>().MessageBox("Please drop Video Path");
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

        private void SettingFlyout_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send(ProjectFlyout.IsOpen || SettingFlyout.IsOpen || CaptionSeacherFlyout.IsOpen || FavoriteFlyout.IsOpen || CambridgeFlyout.IsOpen, "StopOrResumeVideoToken");
        }
    }
}
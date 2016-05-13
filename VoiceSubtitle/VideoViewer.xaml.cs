﻿using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Vlc.DotNet.Forms;
using VoiceSubtitle.ViewModel;
using System.Linq;

namespace VoiceSubtitle
{
    public partial class VideoViewer : UserControl
    {
        private VlcControl MediaPlayer { get; set; }
        private VideoViewModel videoViewModel;
        private PlayerViewModel playerViewModel;
        private SettingViewModel settingViewModel;
        private NotifyViewModel notifyViewModel;
        private bool isChangeBySetter = false;
        private long totalVideoLength;
        private long currentPosition;
        bool isPlayingLast = false;

        public VideoViewer()
        {
            InitializeComponent();

            videoViewModel = ServiceLocator.Current.GetInstance<VideoViewModel>();
            playerViewModel = ServiceLocator.Current.GetInstance<PlayerViewModel>();
            settingViewModel = ServiceLocator.Current.GetInstance<SettingViewModel>();
            notifyViewModel = ServiceLocator.Current.GetInstance<NotifyViewModel>();

            try
            {
                myControl.MediaPlayer.VlcLibDirectoryNeeded += OnVlcControlNeedsLibDirectory;
                myControl.MediaPlayer.EndInit();

                MediaPlayer = myControl.MediaPlayer;

                MediaPlayer.Playing += (x, s) => { videoViewModel.IsPlaying = true; };
                MediaPlayer.Paused += (x, s) => { videoViewModel.IsPlaying = false; };
                MediaPlayer.Stopped += MediaPlayer_Stopped;
                MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;

                MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
                realSlider.ValueChanged += RealSlider_ValueChanged;
              

                Messenger.Default.Register<bool>(this, "InteruptWindowToggleToken", (isOpen) =>
                {
                    if (!isOpen)
                    {
                        isPlayingLast = MediaPlayer.IsPlaying;

                        if(MediaPlayer.IsPlaying) 
                            MediaPlayer.Pause();

                        videoViewModel.IsShowVideo = false;
                    }
                    else if (isPlayingLast)
                    {
                        if (!MediaPlayer.IsPlaying)
                            MediaPlayer.Play();

                        videoViewModel.IsShowVideo = true;
                    }
                });

                Messenger.Default.Register<string>(this, "SetSourceVideoToken", (videoPath) =>
                {

                    MediaPlayer.SetMedia(new Uri(videoPath, UriKind.RelativeOrAbsolute));
                    MediaPlayer.Play();
                });

                Messenger.Default.Register<long>(this, "PlayToPositionVideoToken", (x) =>
                {
                    if (!playerViewModel.IsShowViewer)
                        return;

                    MediaPlayer.Time = x;
                    MediaPlayer.Play();
                });
                Messenger.Default.Register<bool>(this, "PlayVideoToken", (x) =>
                {
                    if (!playerViewModel.IsShowViewer)
                        return;

                    if (!MediaPlayer.IsPlaying)
                        MediaPlayer.Play();
                });
                Messenger.Default.Register<bool>(this, "ToggleVideoToken", (x) =>
                {
                    if (!playerViewModel.IsShowViewer)
                        return;

                    if (MediaPlayer.IsPlaying)
                        MediaPlayer.Pause();
                    else
                        MediaPlayer.Play();
                });

                Messenger.Default.Register<bool>(this, "PauseVideoToken", (x) =>
                {
                    if (!playerViewModel.IsShowViewer)
                        return;

                    if (MediaPlayer.IsPlaying)
                        MediaPlayer.Pause();
                });

                Messenger.Default.Register<bool>(this, "StopVideoToken", (x) =>
                {
                    if (!playerViewModel.IsShowViewer)
                        return;

                    MediaPlayer.Stop();
                });
            }
            catch (Exception ex)
            {
                notifyViewModel.ShowMessageBox("Error loading Video Library");
            }
        }

        private void MediaPlayer_Stopped(object sender, Vlc.DotNet.Core.VlcMediaPlayerStoppedEventArgs e)
        {
        }

        private void RealSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isChangeBySetter)
                MediaPlayer.Time = (long)realSlider.Value;
        }

        private void MediaPlayer_LengthChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerLengthChangedEventArgs e)
        {
            totalVideoLength = MediaPlayer.Length;

            TimeSpan totalTime = TimeSpan.FromMilliseconds(MediaPlayer.Length);
            videoViewModel.VideoDuration = totalTime;
        }

        private void MediaPlayer_TimeChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerTimeChangedEventArgs e)
        {
            currentPosition = e.NewTime;

            if (settingViewModel.DisplayCaptionWhilePlaying)
            {
                TimeSpan currentTime = TimeSpan.FromMilliseconds(e.NewTime);
                var currentVideoCaption = playerViewModel.PrimaryCaption.Where(x => x.From <= currentTime && currentTime <= x.To).FirstOrDefault();
                if (currentVideoCaption != playerViewModel.SelectedPrimaryCaption)
                    playerViewModel.SelectedPrimaryCaption = currentVideoCaption;
            }
            Dispatcher.Invoke(() =>
            {
                position.Width = new GridLength(currentPosition, GridUnitType.Star);
                remain.Width = new GridLength(totalVideoLength - currentPosition, GridUnitType.Star);
            });

            videoViewModel.TimePosition = TimeSpan.FromMilliseconds(e.NewTime);
        }

        private void OnVlcControlNeedsLibDirectory(object sender, VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
                return;

            string vlcDir;
            if (AssemblyName.GetAssemblyName(currentAssembly.Location).ProcessorArchitecture == ProcessorArchitecture.X86)
                vlcDir = currentDirectory + @"\lib\x86\";
            else
                vlcDir = currentDirectory + @"\lib\x86\";

            e.VlcLibDirectory = new DirectoryInfo(vlcDir);
        }

        private void StaticSlider_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            realSlider.Visibility = Visibility.Visible;
            staticSlider.Visibility = Visibility.Collapsed;
            realSlider.Maximum = totalVideoLength;
            isChangeBySetter = true;
            realSlider.Value = currentPosition;
            isChangeBySetter = false;
        }

        private void StaticSlider_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            realSlider.Visibility = Visibility.Collapsed;
            staticSlider.Visibility = Visibility.Visible;
        }
    }
}
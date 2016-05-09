using GalaSoft.MvvmLight;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VoiceSubtitle.Model;

namespace VoiceSubtitle.ViewModel
{
    public class VideoViewModel : ViewModelBase
    {
        private MediaElement playVoiceMedia = new MediaElement();
        private DispatchService dispatchService;
        private CancellationTokenSource videoLoopTokenSource;
        private SettingViewModel settingViewModel;
        private MediaElement MediaPlayer => VideoViewer.MediaPlayer;
        public ICommand PlayPauseVideoCommand { get; }

        public VideoViewModel(DispatchService dispatchService, SettingViewModel settingViewModel)
        {
            this.dispatchService = dispatchService;
            this.settingViewModel = settingViewModel;

            playVoiceMedia.LoadedBehavior = MediaState.Manual;
            playVoiceMedia.UnloadedBehavior = MediaState.Manual;

            PlayPauseVideoCommand = new ActionCommand((x) =>
            {
            });
        }

        public string videoStatus;

        public string VideoStatus
        {
            get
            {
                return videoStatus;
            }
            set
            {
                Set(ref videoStatus, value);
            }
        }

        public TimeSpan timePosition;

        public TimeSpan TimePosition
        {
            get
            {
                return timePosition;
            }
            set
            {
                Set(ref timePosition, value);
            }
        }

        public TimeSpan videoDuration;

        public TimeSpan VideoDuration
        {
            get
            {
                return videoDuration;
            }
            set
            {
                Set(ref videoDuration, value);
            }
        }

        public void PlayVoice(string url)
        {
            try
            {
                playVoiceMedia.Stop();
                playVoiceMedia.Source = new Uri(url);
                playVoiceMedia.Play();
            }
            finally { }
        }

        public void BeginLoopVideo(int loop, TimeSpan from, TimeSpan to)
        {
            if (videoLoopTokenSource != null)
            {
                videoLoopTokenSource.Cancel();
            }
            videoLoopTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => LoopListening(loop, from, to, videoLoopTokenSource));
        }

        private async Task LoopListening(int loop, TimeSpan from, TimeSpan to, CancellationTokenSource tokenSource)
        {
            if (from >= to)
                return;

            CancellationToken ct = tokenSource.Token;

            try
            {
                for (int i = 0; i < loop; i++)
                {
                    VideoStatus = $"Listen loop {i + 1}/{loop}";
                    ct.ThrowIfCancellationRequested();
                    dispatchService.Invoke(() =>
                    {
                        MediaPlayer.Position = from;
                        MediaPlayer.Play();
                    });

                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(to - from);
                    ct.ThrowIfCancellationRequested();
                    if (i == loop - 1 && !settingViewModel.PlayAfterEndingLoop)
                    {
                        dispatchService.Invoke(() => MediaPlayer.Pause());
                    }

                    await Task.Delay(TimeSpan.FromSeconds(0));
                }
            }
            finally
            {
                videoLoopTokenSource = null;
                VideoStatus = string.Empty;
            }
        }

        private bool loadEvents = false;

        private void MediaEvents()
        {
            if (loadEvents)
                return;

            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();

            loadEvents = true;
        }

        private void MediaPlayer_MediaOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            VideoDuration = MediaPlayer.NaturalDuration.TimeSpan;
            VideoStatus = "Loading Success";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (TimePosition != MediaPlayer.Position)
                TimePosition = MediaPlayer.Position;
        }

        public void LoadVideo(string videoPath)
        {
            MediaEvents();

            try
            {
                MediaPlayer.Source = new Uri(videoPath, UriKind.RelativeOrAbsolute);
            }
            catch (Exception ex)
            {
                VideoStatus = "Loading Error";
            }
        }
    }
}
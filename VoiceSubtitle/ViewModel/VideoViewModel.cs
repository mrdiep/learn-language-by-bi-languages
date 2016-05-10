using GalaSoft.MvvmLight;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using VoiceSubtitle.Model;

namespace VoiceSubtitle.ViewModel
{
    public class VideoViewModel : ViewModelBase
    {
        public static readonly string[] VideoExtenstionSupported;

        private MediaElement playVoiceMedia;
        private DispatchService dispatchService;
        private CancellationTokenSource videoLoopTokenSource;
        private SettingViewModel settingViewModel;

        public ICommand PauseVideoCommand { get; }
        public ICommand PlayVideoCommand { get; }

        static VideoViewModel()
        {
            var ext = ".webm|.mkv|.flv|.vob|.ogv|.ogg|.drc|.avi|.mov|.qt|.wmv|.rm|.rmvb|.mp4|.m4p|.m4v|.mpg|.mp2|.mpeg|.mpe|.mpv|.mpg|.mpeg|.m2v|.3gp|.3g2|.mxf|.roq|.nsv|.flv|.f4v|.f4p|.f4a|.f4b";
            VideoExtenstionSupported = ext.Split('|');

        }

        public VideoViewModel(DispatchService dispatchService, SettingViewModel settingViewModel)
        {
            this.dispatchService = dispatchService;
            this.settingViewModel = settingViewModel;
            playVoiceMedia = new MediaElement();
            playVoiceMedia.LoadedBehavior = MediaState.Manual;
            playVoiceMedia.UnloadedBehavior = MediaState.Manual;

            PauseVideoCommand = new ActionCommand((x) =>
            {
                MessengerInstance.Send<bool>(true, "PauseVideoToken");
            });

            PlayVideoCommand = new ActionCommand((x) =>
            {
                MessengerInstance.Send<bool>(true, "PlayVideoToken");
            });
        }

        public bool isPlaying;

        public bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
            set
            {
                Set(ref isPlaying, value);
            }
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
                        long time = Convert.ToInt64(from.TotalMilliseconds);
                        MessengerInstance.Send(time, "PlayToPositionVideoToken");
                    });

                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(to - from,ct);
                    ct.ThrowIfCancellationRequested();
                    if (i == loop - 1 && !settingViewModel.PlayAfterEndingLoop)
                    {
                        MessengerInstance.Send(true, "PauseVideoToken");
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

        public void LoadVideo(string videoPath)
        {
            try
            {
                MessengerInstance.Send(videoPath, "SetSourceVideoToken");
            }
            catch (Exception ex)
            {
                VideoStatus = "Loading Error";
            }
        }
    }
}
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
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
        private NotifyViewModel notifyViewModel;
        public ICommand PauseVideoCommand { get; }
        public ICommand StopVideoCommand { get; }
        public ICommand PlayVideoCommand { get; }
        public ICommand TogglePlayVideo { get; set; }
        public ICommand CancelLoopVideo { get; set; }

        static VideoViewModel()
        {
            var ext = ".webm|.mkv|.flv|.vob|.ogv|.ogg|.drc|.avi|.mov|.qt|.wmv|.rm|.rmvb|.mp4|.m4p|.m4v|.mpg|.mp2|.mpeg|.mpe|.mpv|.mpg|.mpeg|.m2v|.3gp|.3g2|.mxf|.roq|.nsv|.flv|.f4v|.f4p|.f4a|.f4b";
            VideoExtenstionSupported = ext.Split('|');
        }

        public VideoViewModel(DispatchService dispatchService, SettingViewModel settingViewModel, NotifyViewModel notifyViewModel)
        {
            this.dispatchService = dispatchService;
            this.settingViewModel = settingViewModel;
            this.notifyViewModel = notifyViewModel;

            playVoiceMedia = new MediaElement();
            playVoiceMedia.LoadedBehavior = MediaState.Manual;
            playVoiceMedia.UnloadedBehavior = MediaState.Manual;

            CancelLoopVideo = new ActionCommand(() => videoLoopTokenSource?.Cancel());
            PauseVideoCommand = new ActionCommand(() =>
            {
                MessengerInstance.Send(true, "PauseVideoToken");
                videoLoopTokenSource?.Cancel();
            });
            StopVideoCommand = new ActionCommand(() => MessengerInstance.Send(true, "StopVideoToken"));
            PlayVideoCommand = new ActionCommand(() =>
            {
                MessengerInstance.Send(true, "PlayVideoToken");
                ShowVideo();
            });
            TogglePlayVideo = new ActionCommand(() =>
            {
                MessengerInstance.Send(true, "ToggleVideoToken");
                videoLoopTokenSource?.Cancel();
                ShowVideo();
            });

            MessengerInstance.Register<bool>(this, "CancelLoopVideoToken", (x) => videoLoopTokenSource?.Cancel());
        }

        private static void ShowVideo()
        {
            ServiceLocator.Current.GetInstance<CambridgeDictionaryViewModel>().IsShowPanel = false;
            ServiceLocator.Current.GetInstance<FavoriteViewModel>().IsShowPanel = false;
            ServiceLocator.Current.GetInstance<MainViewModel>().IsShowProjectPanel = false;
            ServiceLocator.Current.GetInstance<SettingViewModel>().IsShowPanel = false;
            ServiceLocator.Current.GetInstance<SubtitleDownloaderViewModel>().IsShowPanel = false;
            ServiceLocator.Current.GetInstance<VideoViewModel>().IsShowVideo = true;
        }

        public bool isShowVideo = true;

        public bool IsShowVideo
        {
            get
            {
                return isShowVideo;
            }
            set
            {
                Set(ref isShowVideo, value);
            }
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

        public void PlayCambridge(string url)
        {
            try
            {
                playVoiceMedia.Stop();
                playVoiceMedia.Source = new Uri(url);
                playVoiceMedia.Play();
            }
            catch { notifyViewModel.Text = "Play voice error"; }
            finally { }
        }

        public void BeginLoopVideo(int loop, TimeSpan from, TimeSpan to)
        {

            from = from - TimeSpan.FromMilliseconds(ExpandStart);
            to = to + TimeSpan.FromMilliseconds(ExpandEnd);

            videoLoopTokenSource?.Cancel();
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
                    await Task.Delay(to - from, ct);
                    ct.ThrowIfCancellationRequested();
                    if (i == loop - 1 && !settingViewModel.PlayAfterEndingLoop)
                    {
                        MessengerInstance.Send(true, "PauseVideoToken");
                    }

                    if (settingViewModel.PauseEachLoop > 0 && i !=loop - 1)
                    {
                        notifyViewModel.Text = "Wait...";
                        MessengerInstance.Send(true, "PauseVideoToken");
                        await Task.Delay(TimeSpan.FromMilliseconds(settingViewModel.PauseEachLoop));
                    }
                }
            }
            catch
            {
                notifyViewModel.Text = "Cancel loop";
            }
            finally
            {
                videoLoopTokenSource = null;
                VideoStatus = string.Empty;
            }
        }

        private double expanStart;

        public double ExpandStart
        {
            get { return expanStart; }
            set
            {
                Set(ref expanStart, value);
            }
        }

        private double expanEnd;

        public double ExpandEnd
        {
            get { return expanEnd; }
            set
            {
                Set(ref expanEnd, value);
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
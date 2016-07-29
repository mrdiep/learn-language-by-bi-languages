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

        private readonly MediaElement _playVoiceMedia;
        private readonly DispatchService _dispatchService;
        private CancellationTokenSource _videoLoopTokenSource;
        private readonly SettingViewModel _settingViewModel;
        private readonly NotifyViewModel _notifyViewModel;

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
            _dispatchService = dispatchService;
            _settingViewModel = settingViewModel;
            _notifyViewModel = notifyViewModel;

            _playVoiceMedia = new MediaElement();
            _playVoiceMedia.LoadedBehavior = MediaState.Manual;
            _playVoiceMedia.UnloadedBehavior = MediaState.Manual;

            CancelLoopVideo = new ActionCommand(() => _videoLoopTokenSource?.Cancel());
            PauseVideoCommand = new ActionCommand(() =>
            {
                MessengerInstance.Send(true, "PauseVideoToken");
                _videoLoopTokenSource?.Cancel();
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
                _videoLoopTokenSource?.Cancel();
                ShowVideo();
            });

            MessengerInstance.Register<bool>(this, "CancelLoopVideoToken", x => _videoLoopTokenSource?.Cancel());
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

        private bool _isShowVideo = true;

        public bool IsShowVideo
        {
            get
            {
                return _isShowVideo;
            }
            set
            {
                Set(ref _isShowVideo, value);
            }
        }

        private bool _isPlaying;

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                Set(ref _isPlaying, value);
            }
        }

        private string _videoStatus;

        public string VideoStatus
        {
            get
            {
                return _videoStatus;
            }
            set
            {
                Set(ref _videoStatus, value);
            }
        }

        private TimeSpan _timePosition;

        public TimeSpan TimePosition
        {
            get
            {
                return _timePosition;
            }
            set
            {
                Set(ref _timePosition, value);
            }
        }

        private TimeSpan _videoDuration;

        public TimeSpan VideoDuration
        {
            get
            {
                return _videoDuration;
            }
            set
            {
                Set(ref _videoDuration, value);
            }
        }

        public void PlayCambridge(string url)
        {
            try
            {
                _playVoiceMedia.Stop();
                _playVoiceMedia.Source = new Uri(url);
                _playVoiceMedia.Play();
            }
            catch { _notifyViewModel.Text = "Play voice error"; }
        }

        public void BeginLoopVideo(int loop, TimeSpan from, TimeSpan to)
        {
            from = from - TimeSpan.FromMilliseconds(ExpandStart);
            to = to + TimeSpan.FromMilliseconds(ExpandEnd);

            _videoLoopTokenSource?.Cancel();
            _videoLoopTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => LoopListening(loop, from, to, _videoLoopTokenSource));
        }

        private async Task LoopListening(int loop, TimeSpan from, TimeSpan to, CancellationTokenSource tokenSource)
        {
            if (from >= to)
                return;

            var ct = tokenSource.Token;

            try
            {
                for (var i = 0; i < loop; i++)
                {
                    VideoStatus = $"Listen loop {i + 1}/{loop}";
                    ct.ThrowIfCancellationRequested();
                    _dispatchService.Invoke(() =>
                    {
                        var time = Convert.ToInt64(from.TotalMilliseconds);
                        MessengerInstance.Send(time, "PlayToPositionVideoToken");
                    });

                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(to - from, ct);
                    ct.ThrowIfCancellationRequested();
                    if (i == loop - 1 && !_settingViewModel.PlayAfterEndingLoop)
                    {
                        MessengerInstance.Send(true, "PauseVideoToken");
                    }

                    if (_settingViewModel.PauseEachLoop > 0 && i != loop - 1)
                    {
                        _notifyViewModel.Text = "Wait...";
                        MessengerInstance.Send(true, "PauseVideoToken");
                        await Task.Delay(TimeSpan.FromMilliseconds(_settingViewModel.PauseEachLoop), ct);
                    }
                }
            }
            catch
            {
                _notifyViewModel.Text = "Cancel loop";
            }
            finally
            {
                _videoLoopTokenSource = null;
                VideoStatus = string.Empty;
            }
        }

        private double _expanStart = 300;

        public double ExpandStart
        {
            get
            {
                return _expanStart;
            }
            set
            {
                Set(ref _expanStart, value);
            }
        }

        private double _expanEnd = 200;

        public double ExpandEnd
        {
            get { return _expanEnd; }
            set
            {
                Set(ref _expanEnd, value);
            }
        }

        public void LoadVideo(string videoPath)
        {
            try
            {
                MessengerInstance.Send(videoPath, "SetSourceVideoToken");
            }
            catch (Exception)
            {
                VideoStatus = "Loading Error";
            }
        }
    }
}
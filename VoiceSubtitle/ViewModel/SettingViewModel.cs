using GalaSoft.MvvmLight;
using System;
using System.Globalization;
using VoiceSubtitle.Helper;
using VoiceSubtitle.Model;

namespace VoiceSubtitle.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly string _fileSetting;
        private IniFile _settings;

        public SettingViewModel()
        {
            _fileSetting = FolderManager.AssemblyPath + @"\settings.ini";
            Load();

            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", x => IsShowPanel = false);
        }

        private void Load()
        {
            _settings = new IniFile(_fileSetting);
            if (!_settings.HasFile)
                Save();

            _playAfterEndingLoop = Convert.ToBoolean(_settings["PlayAfterEndingLoop"] ?? "False");
            _displayCaptionWhilePlaying = Convert.ToBoolean(_settings["DisplayCaptionWhilePlaying"] ?? "False");
            _pauseEachLoop = Convert.ToDouble(_settings["PauseEachLoop"] ?? "0");

            DownloadCaptionLanguage = _settings["DownloadCaptionLanguage"].Split(",".ToCharArray());

            RaisePropertyChanged("PlayAfterEndingLoop");
            RaisePropertyChanged("DisplayCaptionWhilePlaying");
            RaisePropertyChanged("PauseEachLoop");
        }

        private void Save()
        {
            _settings["PlayAfterEndingLoop"] = PlayAfterEndingLoop.ToString();
            _settings["DisplayCaptionWhilePlaying"] = DisplayCaptionWhilePlaying.ToString();
            _settings["DownloadCaptionLanguage"] = string.Join(",", DownloadCaptionLanguage);
            _settings["PauseEachLoop"] = PauseEachLoop.ToString(CultureInfo.InvariantCulture);
            _settings["PrimaryCaptionZoom"] = PrimaryCaptionZoom.ToString(CultureInfo.InvariantCulture);

            _settings.Save();
        }

        private bool _isShowPanel;

        public bool IsShowPanel
        {
            get
            {
                return _isShowPanel;
            }
            set
            {
                Set(ref _isShowPanel, value);
            }
        }

        public string[] DownloadCaptionLanguage { get; set; } = { "Vietnamese", "English" };

        private bool _playAfterEndingLoop;

        public bool PlayAfterEndingLoop
        {
            get
            {
                return _playAfterEndingLoop;
            }
            set
            {
                Set(ref _playAfterEndingLoop, value);
                Save();
            }
        }

        private bool _displayCaptionWhilePlaying;

        public bool DisplayCaptionWhilePlaying
        {
            get
            {
                return _displayCaptionWhilePlaying;
            }
            set
            {
                Set(ref _displayCaptionWhilePlaying, value);
                Save();
            }
        }

        private double _pauseEachLoop;

        public double PauseEachLoop
        {
            get
            {
                return _pauseEachLoop;
            }
            set
            {
                Set(ref _pauseEachLoop, value);
                Save();
            }
        }

        private double _primaryCaptionZoom = 1.3;

        public double PrimaryCaptionZoom
        {
            get
            {
                if (_primaryCaptionZoom <= 0.5)
                    _primaryCaptionZoom = 0.5;

                if (_primaryCaptionZoom >= 3)
                    _primaryCaptionZoom = 3;

                return _primaryCaptionZoom;
            }
            set
            {
                if (_primaryCaptionZoom <= 0.5)
                    _primaryCaptionZoom = 0.5;

                if (_primaryCaptionZoom >= 3)
                    _primaryCaptionZoom = 3;

                Set(ref _primaryCaptionZoom, value);
                Save();
            }
        }
    }
}
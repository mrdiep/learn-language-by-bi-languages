using GalaSoft.MvvmLight;
using System;
using VoiceSubtitle.Helper;
using VoiceSubtitle.Model;

namespace VoiceSubtitle.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly string FileSetting;
        private IniFile settings;

        public SettingViewModel()
        {
            FileSetting = FolderManager.AssemblyPath + @"\settings.ini";
            Load();

            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", (x) => IsShowPanel = false);
        }

        private void Load()
        {
            settings = new IniFile(FileSetting);
            if (!settings.HasFile)
                Save();

            playAfterEndingLoop = Convert.ToBoolean(settings["PlayAfterEndingLoop"] ?? "False");
            displayCaptionWhilePlaying = Convert.ToBoolean(settings["DisplayCaptionWhilePlaying"]??"False");
            pauseEachLoop = Convert.ToDouble(settings["PauseEachLoop"]??"0");

            DownloadCaptionLanguage = settings["DownloadCaptionLanguage"].Split(",".ToCharArray());

            RaisePropertyChanged("PlayAfterEndingLoop");
            RaisePropertyChanged("DisplayCaptionWhilePlaying");
            RaisePropertyChanged("PauseEachLoop");
        }

        private void Save()
        {
            settings["PlayAfterEndingLoop"] = PlayAfterEndingLoop.ToString();
            settings["DisplayCaptionWhilePlaying"] = DisplayCaptionWhilePlaying.ToString();
            settings["DownloadCaptionLanguage"] = string.Join(",", DownloadCaptionLanguage);
            settings["PauseEachLoop"] = PauseEachLoop.ToString();

            settings.Save();
        }

        private bool isShowPanel;

        public bool IsShowPanel
        {
            get
            {
                return isShowPanel;
            }
            set
            {
                Set(ref isShowPanel, value);
            }
        }

        public string[] DownloadCaptionLanguage { get; set; } = { "Vietnamese", "English" };

        private bool playAfterEndingLoop;

        public bool PlayAfterEndingLoop
        {
            get
            {
                return playAfterEndingLoop;
            }
            set
            {
                Set(ref playAfterEndingLoop, value);
                Save();
            }
        }

        private bool displayCaptionWhilePlaying;

        public bool DisplayCaptionWhilePlaying
        {
            get
            {
                return displayCaptionWhilePlaying;
            }
            set
            {
                Set(ref displayCaptionWhilePlaying, value);
                Save();
            }
        }

        private double pauseEachLoop;

        public double PauseEachLoop
        {
            get
            {
                return pauseEachLoop;
            }
            set
            {
                Set(ref pauseEachLoop, value);
                Save();
            }
        }


        
    }
}
using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using VoiceSubtitle.Helper;

namespace VoiceSubtitle.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly string FileSetting;
        private IniFile settings;

        public SettingViewModel()
        {
            FileSetting = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\settings.ini";
            Load();

            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", (x) => IsShowPanel = false);
        }

        private void Load()
        {
            settings = new IniFile(FileSetting);
            if (!settings.HasFile)
                Save();

            PlayAfterEndingLoop = Convert.ToBoolean(settings["PlayAfterEndingLoop"]);
            DisplayCaptionWhilePlaying = Convert.ToBoolean(settings["DisplayCaptionWhilePlaying"]);
            DownloadCaptionLanguage = settings["DownloadCaptionLanguage"].Split(",".ToCharArray());
        }

        private void Save()
        {
            settings["PlayAfterEndingLoop"] = PlayAfterEndingLoop.ToString();
            settings["DisplayCaptionWhilePlaying"] = DisplayCaptionWhilePlaying.ToString();
            settings["DownloadCaptionLanguage"] = string.Join(",", DownloadCaptionLanguage);

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

        public string[] DownloadCaptionLanguage { get; set; } = { "Vietnamese","English" };

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
    }
}
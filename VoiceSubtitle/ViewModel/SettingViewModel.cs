using GalaSoft.MvvmLight;
using System;
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
        }

        private void Load()
        {            
            settings = new IniFile(FileSetting);
            if (!settings.HasFile)
                Save();

            PlayAfterEndingLoop = Convert.ToBoolean(settings["PlayAfterEndingLoop"]);            
        }

        private void Save()
        {
            settings["PlayAfterEndingLoop"] = PlayAfterEndingLoop.ToString();
            settings.Save();
        }

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
    }
}
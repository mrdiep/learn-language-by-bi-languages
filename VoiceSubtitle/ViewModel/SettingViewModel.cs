using GalaSoft.MvvmLight;
using System;
using System.IO;
using System.Reflection;
using System.Linq;

namespace VoiceSubtitle.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly string FileSetting;
        public SettingViewModel()
        {
            FileSetting = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\settings.txt";
            Load();
        }

        private void Load()
        {
            
            if (!File.Exists(FileSetting))
            {
                Save();
            }

            var settings = File.ReadLines(FileSetting).ToList();
            PlayAfterEndingLoop = Convert.ToBoolean(settings[0]);
        }

        private void Save()
        {
            File.WriteAllText(FileSetting, $"{PlayAfterEndingLoop}");
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
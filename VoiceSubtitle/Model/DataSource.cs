using GalaSoft.MvvmLight;
using System.IO;
using System;

namespace VoiceSubtitle.Model
{
    public class SourcePath : ViewModelBase
    {
        public string Path { get; set; }

        private string videoName;

        public string VideoName
        {
            get { return videoName; }
            set { Set(ref videoName, value); }
        }

        private string video;

        public string Video
        {
            get { return video; }
            set
            {
                Set(ref video, value);
            }
        }

        private string primaryCaption;

        public string PrimaryCaption
        {
            get { return primaryCaption; }
            set { Set(ref primaryCaption, value); }
        }

        private string translatedCaption;

        public string TranslatedCaption
        {
            get { return translatedCaption; }
            set { Set(ref translatedCaption, value); }
        }

        public void Save()
        {
            string content = $"{VideoName}\r\n{Video}\r\n{PrimaryCaption}\r\n{TranslatedCaption}";
            if (!string.IsNullOrEmpty(Path) && File.Exists(Path))
            {
                File.WriteAllText(Path, content);
                return;
            }

            string file = FolderManager.FolderCaptionPath + $@"\{DateTime.Now.Ticks}.cap";
            File.WriteAllText(file, content);
            Path = file;
        }
    }
}
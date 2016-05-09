using GalaSoft.MvvmLight;
using System.IO;
using System;
using System.Reflection;

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
            string content = $"{this.VideoName}\r\n{this.Video}\r\n{this.PrimaryCaption}\r\n{this.TranslatedCaption}";
            if(!string.IsNullOrEmpty(Path) && File.Exists(Path))
            {
                File.WriteAllText(Path, content);
                return;
            }

            string folder = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\captions";
            string file = folder + $@"\{DateTime.Now.Ticks}.cap";
            File.WriteAllText(file, content);
            Path = file;
        }
    }
}
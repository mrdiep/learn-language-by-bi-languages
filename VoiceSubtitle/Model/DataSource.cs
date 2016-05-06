﻿using GalaSoft.MvvmLight;
using System.IO;

namespace VoiceSubtitle.Model
{
    public class SourcePath : ViewModelBase
    {
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

                //string videoName = string.Empty;
                //if (!string.IsNullOrWhiteSpace(value))
                //{
                //    videoName = Path.GetFileName(value);
                //}

                //RaisePropertyChanged("VideoName");
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
    }
}
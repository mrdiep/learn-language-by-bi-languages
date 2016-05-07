using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using VoiceSubtitle.Model;
using System.Linq;
using VoiceSubtitle.Helper;

namespace VoiceSubtitle.ViewModel
{
    public class PlayerViewModel : ViewModelBase
    {
        private static Regex Unit = new Regex(
            @"(?<sequence>\d+)\r\n(?<start>\d{2}\:\d{2}\:\d{2},\d{3}) --\> (?<end>\d{2}\:\d{2}\:\d{2},\d{3})\r\n(?<text>[\s\S]*?\r\n\r\n)",
            RegexOptions.Compiled | RegexOptions.ECMAScript);

        private static readonly string FormatTime = @"hh\:mm\:ss\,fff";

        private DispatchService dispatchService;
        private CambridgeDictionaryViewModel cambridgeDictionaryViewModel;
        private VideoViewModel videoViewModel;

        public ICommand Listen { get; }
        public ICommand PlayVoice { get; }
        public ICommand SwitchSource { get; }

        public PlayerViewModel(DispatchService dispatchService, CambridgeDictionaryViewModel cambridgeDictionaryViewModel, VideoViewModel videoViewModel)
        {
            this.dispatchService = dispatchService;
            this.cambridgeDictionaryViewModel = cambridgeDictionaryViewModel;
            this.videoViewModel = videoViewModel;

            PrimaryCaption = new ObservableCollection<PartialCaption>();
            TranslateCaption = new ObservableCollection<PartialCaption>();

            PlayVoice = new ActionCommand((link) =>
            {
                string url = link as string;
                this.videoViewModel.PlayVoice(url);
            }
            );

            Listen = new ActionCommand(loop =>
            {
                if (SelectedPrimaryCaption == null)
                    return;

                int timeLoop = Convert.ToInt32(loop as string);

                this.videoViewModel.BeginLoopVideo(timeLoop, SelectedPrimaryCaption.From, SelectedPrimaryCaption.To);
            }
             );

            SwitchSource = new ActionCommand(async x =>
            {
                var source = x as SourcePath;
                if (source == null)
                    return;
                List<PartialCaption> primaryCaption = null, translatedCaption = null;

                var task1 = Task.Factory.StartNew(() => { primaryCaption = LoadSubFormFile(source.PrimaryCaption); });
                var task2 = Task.Factory.StartNew(() => { translatedCaption = LoadSubFormFile(source.TranslatedCaption); });

                await Task.WhenAll(task1, task2);

                dispatchService.Invoke(() =>
                {
                    PrimaryCaption.Clear();
                    TranslateCaption.Clear();

                    primaryCaption.ForEach(c => { PrimaryCaption.Add(c); });
                    translatedCaption.ForEach(c => { TranslateCaption.Add(c); });

                    VideoPath = source.Video;
                    VideoName = source.VideoName;

                    videoViewModel.LoadVideo(VideoPath);

                    IsShowViewer = true;
                });
            });
        }

        public ObservableCollection<PartialCaption> PrimaryCaption { get; private set; }
        public ObservableCollection<PartialCaption> TranslateCaption { get; private set; }

        private string videoPath;

        public string VideoPath
        {
            get { return videoPath; }
            set
            {
                Set(ref videoPath, value);
            }
        }

        private string videoName;

        public string VideoName
        {
            get
            {
                return videoName;
            }
            set
            {
                Set(ref videoName, value);
            }
        }

        private LanguageDetail languageDetail;

        public LanguageDetail LanguageDetail
        {
            get
            { return languageDetail; }
            set
            {
                Set(ref languageDetail, value);
            }
        }

        private bool isShowViewer;

        public bool IsShowViewer
        {
            get { return isShowViewer; }
            set
            {
                Set(ref isShowViewer, value);
            }
        }

        private PartialCaption selectedPrimaryCaption;

        public PartialCaption SelectedPrimaryCaption
        {
            get
            {
                return selectedPrimaryCaption;
            }
            set
            {
                Set(ref selectedPrimaryCaption, value);
                if (value == null)
                    return;

                selectedTranslateCaption = ReflectCaptionHandler(value, TranslateCaption);
                RaisePropertyChanged("SelectedTranslateCaption");
                RaiseChangePrononce();
            }
        }

        private PartialCaption selectedTranslateCaption;

        public PartialCaption SelectedTranslateCaption
        {
            get
            {
                return selectedTranslateCaption;
            }
            set
            {
                Set(ref selectedTranslateCaption, value);
                if (value == null)
                    return;

                selectedPrimaryCaption = ReflectCaptionHandler(value, PrimaryCaption);
                RaisePropertyChanged("SelectedPrimaryCaption");
                RaiseChangePrononce();
            }
        }

        private void RaiseChangePrononce()
        {
            if (selectedPrimaryCaption == null)
            {
                LanguageDetail = null;
                return;
            }
            Task.Factory.StartNew(() =>
            {
                var prononce = cambridgeDictionaryViewModel.Prononce(selectedPrimaryCaption.Text);
                LanguageDetail = new LanguageDetail(selectedPrimaryCaption.Text, prononce.Where(x => x != null).ToList());
            });
        }

        private PartialCaption ReflectCaptionHandler(object x, IEnumerable<PartialCaption> captionRef)
        {
            var caption = x as PartialCaption;
            if (caption == null)
                return null;

            var t = from c in captionRef
                    where Intersects(caption.From, caption.To, c.From, c.To) > 0
                    select c;
            var k = t.ToList().OrderByDescending(c => Intersects(caption.From, caption.To, c.From, c.To)).FirstOrDefault();

            return k;
        }

        private double Intersects(TimeSpan f1, TimeSpan t1, TimeSpan f2, TimeSpan t2)
        {
            if (f1 > t1 || f2 > t2)
                return 0;

            if (f1 == t1 || f2 == t2)
                return 0; // No actual date range

            if (f1 == f2 || t1 == t2)
                return (t1 - f1).TotalMilliseconds; // If any set is the same time, then by default there must be some overlap.

            if (f1 < f2)
            {
                if (t1 > f2 && t1 < t2)
                    return (t1 - f2).TotalMilliseconds; // Condition 1

                if (t1 > t2)
                    return (t2 - f2).TotalMilliseconds; // Condition 3
            }
            else
            {
                if (t2 > f1 && t2 < t1)
                    return (t2 - f1).TotalMilliseconds; // Condition 2

                if (t2 > t1)
                    return (t1 - f1).TotalMilliseconds; // Condition 4
            }

            return 0;
        }

        private List<PartialCaption> LoadSubFormFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<PartialCaption>();
            }

            var textSource = File.ReadAllText(filePath);
            var captions = new List<PartialCaption>();
            var matches = Unit.Matches(textSource);
            foreach (Match e in matches)
            {
                var index = Convert.ToInt32(e.Groups[1].Value);
                var from = TimeSpan.ParseExact(e.Groups[2].Value, FormatTime, CultureInfo.InvariantCulture);
                var to = TimeSpan.ParseExact(e.Groups[3].Value, FormatTime, CultureInfo.InvariantCulture);
                var text = e.Groups[4].Value.Replace("\r\n", " ").Replace("  ", " ");

                text = WebHelper.InnerHtmlText(text);
                var caption = new PartialCaption(index, from, to, text);
                captions.Add(caption);
            }

            return captions;
        }
    }
}
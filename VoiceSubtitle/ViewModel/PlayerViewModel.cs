using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using VoiceSubtitle.Model;
using System.Linq;

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

        public PlayerViewModel(DispatchService dispatchService, CambridgeDictionaryViewModel cambridgeDictionaryViewModel)
        {
            this.dispatchService = dispatchService;
            this.cambridgeDictionaryViewModel = cambridgeDictionaryViewModel;

            PrimaryCaption = new ObservableCollection<PartialCaption>();
            TranslateCaption = new ObservableCollection<PartialCaption>();
        }

        private LanguageDetail languageDetail;
        public LanguageDetail LanguageDetail { get { return languageDetail; } set { Set(ref languageDetail, value); } }

        public ObservableCollection<PartialCaption> PrimaryCaption { get; private set; }
        public ObservableCollection<PartialCaption> TranslateCaption { get; private set; }

        public List<PartialCaption> LoadSubFormFile(string filePath)
        {
            var textSource = File.ReadAllText(filePath);
            var captions = new List<PartialCaption>();
            var matches = Unit.Matches(textSource);
            foreach (Match e in matches)
            {
                var index = Convert.ToInt32(e.Groups[1].Value);
                var from = TimeSpan.ParseExact(e.Groups[2].Value, FormatTime, CultureInfo.InvariantCulture);
                var to = TimeSpan.ParseExact(e.Groups[3].Value, FormatTime, CultureInfo.InvariantCulture);
                var text = e.Groups[4].Value.Replace("\r\n", " ").Replace("  ", " ");

                text = HttpUtility.HtmlDecode(text);
                var caption = new PartialCaption(index, from, to, text);
                captions.Add(caption);
            }

            return captions;
        }

        private PartialCaption selectedPrimaryCaption;

        public  PartialCaption SelectedPrimaryCaption
        {
            get
            {
                return selectedPrimaryCaption;
            }
            set
            {
                Set(ref selectedPrimaryCaption, value);
                selectedTranslateCaption = ReflectCaptionHandler(value, TranslateCaption);
                RaisePropertyChanged("SelectedTranslateCaption");
                Task.Factory.StartNew(() => {
                    var prononce = cambridgeDictionaryViewModel.Prononce(value.Text).Result;
                    LanguageDetail = new LanguageDetail(value.Text, prononce.Where(x => x != null).ToList())
                    {
                        
                    };
                });

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
                selectedPrimaryCaption = ReflectCaptionHandler(value, PrimaryCaption);
                RaisePropertyChanged("SelectedPrimaryCaption");
            }
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

        public double Intersects(TimeSpan f1, TimeSpan t1, TimeSpan f2, TimeSpan t2)
        {
            if (f1 > t1 || f2 > t2)
                return 0;

            if (f1 == t1 || f2 == t2)
                return 0; // No actual date range

            if (f1 == f2 || t1 == t2)
                return (t1 - t2).TotalMilliseconds; // If any set is the same time, then by default there must be some overlap.

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

        private ICommand switchSource = null;

        public ICommand SwitchSource
        {
            get
            {
                return switchSource ?? (switchSource = new ActionCommand(async x =>
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
                    });

                    Messenger.Default.Send(true, "UpdateNewSource");
                }));
            }
        }
    }
}
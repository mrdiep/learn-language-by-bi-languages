using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using VoiceSubtitle.Model;
using System.Linq;
using System.Windows;
using static VoiceSubtitle.Helper.ConverterExtensions;
using GalaSoft.MvvmLight.Messaging;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.Practices.ServiceLocation;

namespace VoiceSubtitle.ViewModel
{
    public class PlayerViewModel : ViewModelBase
    {
        private DispatchService dispatchService;
        private CambridgeDictionaryViewModel cambridgeDictionaryViewModel;
        private VideoViewModel videoViewModel;
        private NotifyViewModel notifyViewModel;

        public SourcePath CurrentSource { get; private set; }

        public ICommand Listen { get; }
        public ICommand PlayVoice { get; }
        public ICommand SwitchSource { get; }
        public ICommand SearchBack { get; }
        public ICommand SearchNext { get; }
        public ICommand SyncPrimaryCaptionCommand { get; }
        public ICommand SyncTranslatedCaptionCommand { get; }
        public ICommand OpenExternalPath { get; }
        public ICommand SaveCaptionFile { get; }
        public ICommand SaveAsCaptionFile { get; }

        public PlayerViewModel(DispatchService dispatchService, 
            CambridgeDictionaryViewModel cambridgeDictionaryViewModel, 
            VideoViewModel videoViewModel, 
            NotifyViewModel notifyViewModel
            )
        {
            this.dispatchService = dispatchService;
            this.cambridgeDictionaryViewModel = cambridgeDictionaryViewModel;
            this.videoViewModel = videoViewModel;
            this.notifyViewModel = notifyViewModel;

            PrimaryCaption = new ObservableCollection<PartialCaption>();
            TranslateCaption = new ObservableCollection<PartialCaption>();

            SaveCaptionFile = new ActionCommand((x) =>
            {
                string type = x as string;
                string newline = "\r\n";
                string content = "";
                if (type == "PrimaryCaption")
                {
                    content = string.Join("", PrimaryCaption.Select(c => $@"{c.Index}{newline}{c.From.ToString(@"hh\:mm\:ss\,fff")} --> {c.To.ToString(@"hh\:mm\:ss\,fff")}{newline}{c.Text}{newline}{newline}"));
                    try
                    {
                        File.WriteAllText(CurrentSource.PrimaryCaption, content);
                    }
                    catch
                    {
                        notifyViewModel.MessageBox("Can not save.");
                    }
                }
                else if (type == "TranslateCaption")
                {
                    content = string.Join("", TranslateCaption.Select(c => $@"{c.Index}{newline}{c.From.ToString(@"hh\:mm\:ss\,fff")} --> {c.To.ToString(@"hh\:mm\:ss\,fff")}{newline}{c.Text}{newline}{newline}"));
                    try
                    {
                        File.WriteAllText(CurrentSource.TranslatedCaption, content);
                    }
                    catch
                    {
                        notifyViewModel.MessageBox("Can not save.");
                    }
                }
                else
                {
                    return;
                }
            });

            SaveAsCaptionFile = new ActionCommand((x) =>
            {
                string type = x as string;
                string newline = "\r\n";
                string content = "";
                if (type == "PrimaryCaption")
                {
                    content = string.Join("", PrimaryCaption.Select(c => $@"{c.Index}{newline}{c.From.ToString(@"hh\:mm\:ss\,fff")} --> {c.To.ToString(@"hh\:mm\:ss\,fff")}{newline}{c.Text}{newline}{newline}"));
                }
                else if (type == "TranslateCaption")
                {
                    content = string.Join("", TranslateCaption.Select(c => $@"{c.Index}{newline}{c.From.ToString(@"hh\:mm\:ss\,fff")} --> {c.To.ToString(@"hh\:mm\:ss\,fff")}{newline}{c.Text}{newline}{newline}"));
                }
                else
                {
                    return;
                }

                SaveFileDialog dialog = new SaveFileDialog()
                {
                    Filter = "Srt Files(*.srt)|*.srt|All(*.*)|*"
                };

                if (dialog.ShowDialog() == true)
                {
                    string fileName = dialog.FileName;
                    File.WriteAllText(fileName, content);
                }
            });

            SearchBack = new ActionCommand((text) =>
            {
                 SearchPrimaryCaption(text as string, false);
            });

            OpenExternalPath = new ActionCommand((x) =>
            {
                try
                {
                    Process.Start(Path.GetDirectoryName(VideoPath));
                }
                catch (Exception ex)
                {
                    notifyViewModel.MessageBox($@"Can not open path '{Path.GetDirectoryName(VideoPath)}'");
                }
            });

            SearchNext = new ActionCommand((text) =>
            {
                SearchPrimaryCaption(text as string);
            });

            PlayVoice = new ActionCommand((link) =>
            {
                string url = link as string;
                this.videoViewModel.PlayVoice(url);
            });

            Listen = new ActionCommand(loop =>
            {
                if (SelectedPrimaryCaption == null)
                    return;

                int timeLoop = Convert.ToInt32(loop as string);

                this.videoViewModel.BeginLoopVideo(timeLoop, SelectedPrimaryCaption.From, SelectedPrimaryCaption.To);
            });

            Action<ObservableCollection<PartialCaption>, object> syncCaption = (list, timeInMs) =>
            {
                 long time = Convert.ToInt64(timeInMs);
                 var timegap = TimeSpan.FromMilliseconds(time);
                 foreach (var caption in list)
                 {
                     caption.From = caption.From.Add(timegap);
                     caption.To = caption.To.Add(timegap);
                 }
                 string w = time > 0 ? "forward" : "back";
                 this.notifyViewModel.Text = $"Sync {w} {timegap.ToString(@"ss\.fff")} seconds";
            };

            SyncPrimaryCaptionCommand = new ActionCommand((x) => syncCaption.Invoke(PrimaryCaption, x));

            SyncTranslatedCaptionCommand = new ActionCommand((x) => syncCaption.Invoke(TranslateCaption, x));

            SwitchSource = new ActionCommand(async x =>
            {
                var source = x as SourcePath;
                CurrentSource = source;

                if (source == null)
                {
                    return;
                }
                List<PartialCaption> primaryCaption = null, translatedCaption = null;

                var task1 = Task.Factory.StartNew(() => { primaryCaption = LoadSubFormFile(source.PrimaryCaption); });
                var task2 = Task.Factory.StartNew(() => { translatedCaption = LoadSubFormFile(source.TranslatedCaption); });

                await Task.WhenAll(task1, task2);

                dispatchService.Invoke(() =>
                {
                    videoViewModel.StopVideoCommand.Execute(null);

                    LoadPrimaryCaption(primaryCaption);
                    LoadTranslateCaption(translatedCaption);

                    VideoPath = source.Video;
                    VideoName = source.VideoName;

                    videoViewModel.LoadVideo(VideoPath);

                    IsShowViewer = true;

                    MessengerInstance.Send(true, "CloseAllFlyoutToken");
                });
            });
        }

        private void LoadPrimaryCaption(List<PartialCaption> caption)
        {
            PrimaryCaption.Clear();
            caption.ForEach(c => { PrimaryCaption.Add(c); });
        }

        private void LoadTranslateCaption(List<PartialCaption> caption)
        {
            TranslateCaption.Clear();
            caption.ForEach(c => { TranslateCaption.Add(c); });
        }

        public void UpdatePrivateCaption(string fileCaption)
        {
            var primaryCaption = LoadSubFormFile(fileCaption);
            LoadPrimaryCaption(primaryCaption);
            CurrentSource.PrimaryCaption = fileCaption;
            CurrentSource.Save();
        }

        public void UpdateTranslatedCaption(string fileCaption)
        {
            var translateCaption = LoadSubFormFile(fileCaption);
            LoadTranslateCaption(translateCaption);
            CurrentSource.TranslatedCaption = fileCaption;
            CurrentSource.Save();
        }

        public void UpdateVideoPath(string fileVideo)
        {
            videoViewModel.LoadVideo(VideoPath);

            CurrentSource.Video = fileVideo;
            CurrentSource.Save();
        }

        public ObservableCollection<PartialCaption> PrimaryCaption { get; private set; }
        public ObservableCollection<PartialCaption> TranslateCaption { get; private set; }

        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                Set(ref status, value);
            }
        }

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
                Messenger.Default.Send(!value, "StopOrResumeVideoToken");
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

        #region Search Text

        private string lastSearch = string.Empty;
        private int searchTime;

        public void SearchPrimaryCaption(string text, bool searchDown = true, bool restart = false)
        {
            Status = $"No matching";

            if (text == string.Empty)
            {
                lastSearch = string.Empty;
                return;
            }

            if (restart)
            {
                lastSearch = string.Empty;
            }

            if (lastSearch != text)
            {
                searchTime = -1;
            }

            lastSearch = text;
            text = text.ToLower();
            var matches = PrimaryCaption.Where(x => x.Text.ToLower().Contains(text));
            int matchCount = matches.Count();

            if (searchDown)
            {
                searchTime++;
                if (searchTime > matchCount)
                    searchTime = 0;
            }
            else
            {
                searchTime--;
                if (searchTime < 0)
                    searchTime = matchCount - 1;
            }

            var caption = matches.Skip(searchTime).Take(1)?.FirstOrDefault();
            if (caption == null)
            {
                searchTime = 0;
                caption = matches.Take(1)?.FirstOrDefault();
            }

            if (caption == null)
            {
                notifyViewModel.MessageBox("No word found");
                return;
            }
            Status = $"Search {matchCount} Matches";
            SelectedPrimaryCaption = caption;
        }

        #endregion Search Text
    }
}
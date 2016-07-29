using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using VoiceSubtitle.Model;
using System.Linq;
using static VoiceSubtitle.Helper.ConverterExtensions;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace VoiceSubtitle.ViewModel
{
    public class PlayerViewModel : ViewModelBase
    {
        private DispatchService _dispatchService;
        private CambridgeDictionaryViewModel _cambridgeDictionaryViewModel;
        private VideoViewModel _videoViewModel;
        private NotifyViewModel _notifyViewModel;

        public SourcePath CurrentSource { get; private set; }

        public ICommand ToggleSync { get; }
        public ICommand Listen { get; }
        public ICommand PlayCambridgeWord { get; }
        public ICommand SwitchSource { get; }
        public ICommand SearchBack { get; }
        public ICommand SearchNext { get; }
        public ICommand SyncPrimaryCaptionCommand { get; }
        public ICommand SyncTranslatedCaptionCommand { get; }
        public ICommand OpenExternalPath { get; }
        public ICommand SaveCaptionFile { get; }
        public ICommand SaveAsCaptionFile { get; }

        public ICommand GoToNearPosition { get; }
        public ICommand OpenVlc { get; }

        public PlayerViewModel(DispatchService dispatchService,
            CambridgeDictionaryViewModel cambridgeDictionaryViewModel,
            VideoViewModel videoViewModel,
            NotifyViewModel notifyViewModel
            )
        {
            this._dispatchService = dispatchService;
            this._cambridgeDictionaryViewModel = cambridgeDictionaryViewModel;
            this._videoViewModel = videoViewModel;
            this._notifyViewModel = notifyViewModel;

            PrimaryCaption = new ObservableCollection<PartialCaption>();
            TranslateCaption = new ObservableCollection<PartialCaption>();

            OpenVlc = new ActionCommand(() =>
            {
                string pathVlc = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)}\VideoLAN\VLC\vlc.exe";
                if (File.Exists(pathVlc))
                {
                    Process.Start(pathVlc, $"'{VideoPath}' --sub-file='{CurrentSource.PrimaryCaption}'");
                    return;
                }

                pathVlc = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\VideoLAN\VLC\vlc.exe";
                if (File.Exists(pathVlc))
                {
                   // Process.Start(pathVlc, $"{VideoPath} --sub-file={CurrentSource.PrimaryCaption}");
                    return;
                }
            });
            GoToNearPosition = new ActionCommand(x =>
            {
                var time = (TimeSpan)x;
                var caption = PrimaryCaption.OrderBy(t => Math.Abs((time - t.From).TotalMilliseconds)).FirstOrDefault();
                SelectedPrimaryCaption = caption;
            });

            ToggleSync = new ActionCommand(() =>
            {
                IsOpenSync = !IsOpenSync;
            });
            SaveCaptionFile = new ActionCommand(x =>
             {
                 if (!IsShowViewer)
                     return;

                 var type = x as string;
                 var newline = "\r\n";
                 var content = "";
                 if (type == "PrimaryCaption")
                 {
                     content = string.Join("", PrimaryCaption.Select(c => $@"{c.Index}{newline}{c.From.ToString(@"hh\:mm\:ss\,fff")} --> {c.To.ToString(@"hh\:mm\:ss\,fff")}{newline}{c.Text}{newline}{newline}"));
                     try
                     {
                         File.WriteAllText(CurrentSource.PrimaryCaption, content);
                     }
                     catch
                     {
                         notifyViewModel.ShowMessageBox("Can not save.");
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
                         notifyViewModel.ShowMessageBox("Can not save.");
                     }
                 }
             });

            SaveAsCaptionFile = new ActionCommand(x =>
            {
                if (!IsShowViewer)
                    return;

                var type = x as string;
                var newline = "\r\n";
                var content = "";
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

                var dialog = new SaveFileDialog()
                {
                    Filter = "Srt Files(*.srt)|*.srt|All(*.*)|*"
                };

                if (dialog.ShowDialog() == true)
                {
                    var fileName = dialog.FileName;
                    File.WriteAllText(fileName, content);
                }
            });

            SearchBack = new ActionCommand(text =>
            {
                if (!IsShowViewer)
                    return;

                SearchPrimaryCaption(text as string, false);
            });

            OpenExternalPath = new ActionCommand(x =>
            {
                if (!IsShowViewer)
                    return;

                try
                {
                    Process.Start(Path.GetDirectoryName(VideoPath));
                }
                catch (Exception ex)
                {
                    notifyViewModel.ShowMessageBox($@"Can not open path '{Path.GetDirectoryName(VideoPath)}'");
                }
            });

            SearchNext = new ActionCommand(text =>
            {
                SearchPrimaryCaption(text as string);
            });

            PlayCambridgeWord = new ActionCommand(link =>
            {
                if (!IsShowViewer)
                    return;

                var url = link as string;
                this._videoViewModel.PlayCambridge(url);
            });

            Listen = new ActionCommand(loop =>
            {
                if (!IsShowViewer)
                    return;

                if (SelectedPrimaryCaption == null)
                    return;

                var timeLoop = Convert.ToInt32(loop as string);

                this._videoViewModel.BeginLoopVideo(timeLoop, SelectedPrimaryCaption.From, SelectedPrimaryCaption.To);
            });

            Action<ObservableCollection<PartialCaption>, object> syncCaption = (list, timeInMs) =>
            {
                var time = Convert.ToInt64(timeInMs);
                var timegap = TimeSpan.FromMilliseconds(time);
                foreach (var caption in list)
                {
                    caption.From = caption.From.Add(timegap);
                    caption.To = caption.To.Add(timegap);
                }
                var w = time > 0 ? "forward" : "back";
                this._notifyViewModel.Text = $"Sync {w} {timegap.ToString(@"ss\.fff")} seconds";
            };

            SyncPrimaryCaptionCommand = new ActionCommand(x => syncCaption.Invoke(PrimaryCaption, x));

            SyncTranslatedCaptionCommand = new ActionCommand(x => syncCaption.Invoke(TranslateCaption, x));

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
            _videoViewModel.LoadVideo(VideoPath);

            CurrentSource.Video = fileVideo;
            CurrentSource.Save();
        }

        public ObservableCollection<PartialCaption> PrimaryCaption { get; private set; }
        public ObservableCollection<PartialCaption> TranslateCaption { get; private set; }

        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                Set(ref _status, value);
            }
        }

        private string _videoPath;

        public string VideoPath
        {
            get { return _videoPath; }
            set
            {
                Set(ref _videoPath, value);
            }
        }

        private string _videoName;

        public string VideoName
        {
            get
            {
                return _videoName;
            }
            set
            {
                Set(ref _videoName, value);
            }
        }

        private LanguageDetail _languageDetail;

        public LanguageDetail LanguageDetail
        {
            get
            { return _languageDetail; }
            set
            {
                Set(ref _languageDetail, value);
            }
        }

        private bool _isShowViewer;

        public bool IsShowViewer
        {
            get { return _isShowViewer; }
            set
            {
                Set(ref _isShowViewer, value);
                MessengerInstance.Send(value, "InteruptWindowToggleToken");
            }
        }

        private bool _isOpenSync;

        public bool IsOpenSync
        {
            get { return _isOpenSync; }
            set
            {
                Set(ref _isOpenSync, value);
            }
        }

        private PartialCaption _selectedPrimaryCaption;

        public PartialCaption SelectedPrimaryCaption
        {
            get
            {
                return _selectedPrimaryCaption;
            }
            set
            {
                Set(ref _selectedPrimaryCaption, value);
                if (value == null)
                    return;

                _selectedTranslateCaption = ReflectCaptionHandler(value, TranslateCaption);
                RaisePropertyChanged("SelectedTranslateCaption");
                RaiseChangePrononce();
            }
        }

        private PartialCaption _selectedTranslateCaption;

        public PartialCaption SelectedTranslateCaption
        {
            get
            {
                return _selectedTranslateCaption;
            }
            set
            {
                Set(ref _selectedTranslateCaption, value);
                if (value == null)
                    return;

                _selectedPrimaryCaption = ReflectCaptionHandler(value, PrimaryCaption);
                RaisePropertyChanged("SelectedPrimaryCaption");
                RaiseChangePrononce();
            }
        }

        private void RaiseChangePrononce()
        {
            if (_selectedPrimaryCaption == null)
            {
                LanguageDetail = null;
                return;
            }
            Task.Factory.StartNew(() =>
            {
                var prononce = _cambridgeDictionaryViewModel.Prononce(_selectedPrimaryCaption.Text);
                LanguageDetail = new LanguageDetail(_selectedPrimaryCaption.Text, prononce.Where(x => x != null).ToList());
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

        private string _lastSearch = string.Empty;
        private int _searchTime;

        public void SearchPrimaryCaption(string text, bool searchDown = true, bool restart = false)
        {
            Status = $"No matching";

            if (text == string.Empty)
            {
                _lastSearch = string.Empty;
                return;
            }

            if (restart)
            {
                _lastSearch = string.Empty;
            }

            if (_lastSearch != text)
            {
                _searchTime = -1;
            }

            _lastSearch = text;
            text = text.ToLower();
            var matches = PrimaryCaption.Where(x => x.Text.ToLower().Contains(text));
            var matchCount = matches.Count();

            if (searchDown)
            {
                _searchTime++;
                if (_searchTime > matchCount)
                    _searchTime = 0;
            }
            else
            {
                _searchTime--;
                if (_searchTime < 0)
                    _searchTime = matchCount - 1;
            }

            var caption = matches.Skip(_searchTime).Take(1)?.FirstOrDefault();
            if (caption == null)
            {
                _searchTime = 0;
                caption = matches.Take(1)?.FirstOrDefault();
            }

            if (caption == null)
            {
                _notifyViewModel.ShowMessageBox("No word found");
                return;
            }
            Status = $"Search {matchCount} Matches";
            SelectedPrimaryCaption = caption;
        }

        #endregion Search Text
    }
}
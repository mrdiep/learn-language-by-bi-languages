using HtmlAgilityPack;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using VoiceSubtitle.Model;
using GalaSoft.MvvmLight;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ionic.Zip;
using static VoiceSubtitle.Helper.ConverterExtensions;
using Microsoft.Practices.ServiceLocation;

namespace VoiceSubtitle.ViewModel
{
    public class SubtitleDownloaderViewModel : ViewModelBase
    {
        private DispatchService _dispatchService;
        private SettingViewModel _settingViewModel;
        private NotifyViewModel _notifyViewModel;
        public ICommand SearchCaptionOnline { get; }
        public ICommand AddPrimaryCaptionCommand { get; }
        public ICommand AddTranslatedCaptionCommand { get; }

        public ObservableCollection<FilmInfo> FilmInfos { get; private set; }
        public ObservableCollection<SubtitleInfo> SubtitleInfos { get; private set; }
        private List<SubtitleInfo> _subtitleInfos;

        public SubtitleDownloaderViewModel(SettingViewModel settingViewModel, DispatchService dispatchService, NotifyViewModel notifyViewModel)
        {
            this._dispatchService = dispatchService;
            this._settingViewModel = settingViewModel;
            this._notifyViewModel = notifyViewModel;
            FilmInfos = new ObservableCollection<FilmInfo>();
            SubtitleInfos = new ObservableCollection<SubtitleInfo>();

            _subtitleInfos = new List<SubtitleInfo>();
            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", x => IsShowPanel = false);
            SearchCaptionOnline = new ActionCommand(async text =>
            {
                IsShowPanel = true;

                if (_lastSearch == text as string && _subtitleInfos.Count != 0)
                    return;

                IsFilmInfoDownloading = true;
                _lastSearch = text as string;
                var films = await SearchTitle(text as string);
                IsFilmInfoDownloading = false;

                this._dispatchService.Invoke(() =>
                {
                    FilmInfos = new ObservableCollection<FilmInfo>(films);
                    RaisePropertyChanged("FilmInfos");
                });

                if (films.Count == 0)
                {
                    var textSearch = WebUtility.UrlEncode(text as string);
                    DownloadCaptionList($@"https://subscene.com/subtitles/title?q={textSearch}&l=");
                }
            });

            AddPrimaryCaptionCommand = new ActionCommand(async x =>
            {
                var captionFile = await LocalFileFromDownloader(x as SubtitleInfo);
                if (captionFile != null)
                    ServiceLocator.Current.GetInstance<PlayerViewModel>().UpdatePrivateCaption(captionFile);
            }
            );

            AddTranslatedCaptionCommand = new ActionCommand(async x =>
            {
                var captionFile = await LocalFileFromDownloader(x as SubtitleInfo);
                if (captionFile != null)
                    ServiceLocator.Current.GetInstance<PlayerViewModel>().UpdateTranslatedCaption(captionFile);
            }
          );
        }

        private FilmInfo _currentFilmInfo;

        public FilmInfo CurrentFilmInfo
        {
            get
            {
                return _currentFilmInfo;
            }
            set
            {
                Set(ref _currentFilmInfo, value);
                SubtitleInfos.Clear();
                if (value == null)
                    return;

                DownloadCaptionList(value?.Link);
            }
        }

        private void DownloadCaptionList(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                _subtitleInfos.Clear();
                SubtitleFilter = string.Empty;
                IsCaptionListDownloading = false;
            }

            Task.Factory.StartNew(async () =>
            {
                IsCaptionListDownloading = true;
                var captions = await GetSubtitleExtract(link);
                _subtitleInfos.Clear();
                _dispatchService.Invoke(() =>
                {
                    _subtitleInfos.AddRange(captions);
                    SubtitleFilter = string.Empty;
                });
                IsCaptionListDownloading = false;
            });
        }

        private bool _isFilmInfoDownloading;

        public bool IsFilmInfoDownloading
        {
            get
            {
                return _isFilmInfoDownloading;
            }
            set
            {
                Set(ref _isFilmInfoDownloading, value);
            }
        }

        private bool _isCaptionListDownloading;

        public bool IsCaptionListDownloading
        {
            get
            {
                return _isCaptionListDownloading;
            }
            set
            {
                Set(ref _isCaptionListDownloading, value);
            }
        }

        private string _searchText;

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                Set(ref _searchText, value);
            }
        }

        private string _subtitleFilter;

        public string SubtitleFilter
        {
            get
            {
                return _subtitleFilter;
            }
            set
            {
                Set(ref _subtitleFilter, value);
                SubtitleInfos.Clear();
                foreach (var item in _subtitleInfos.Where(x => x.Title.Contains(value)))
                {
                    SubtitleInfos.Add(item);
                }
            }
        }

        private async Task<string> LocalFileFromDownloader(SubtitleInfo info)
        {
            if (info == null)
                return null;

            var captionText = await DownloadCaptionText(await GetLinkDownload(info.LinkDownload));
            var captions = LoadSourceFromText(captionText);
            if (captions.Count == 0)
            {
                _notifyViewModel.ShowMessageBox("No source found, please choose another");
                return null; ;
            }

            string captionFile = $@"{FolderManager.FolderTempDownloadCaptionPath}\{info.Title}.srt";
            File.WriteAllText(captionFile, captionText);
            return captionFile;
        }

        private bool _isShowPanel;

        public bool IsShowPanel
        {
            get { return _isShowPanel; }
            set
            {
                Set(ref _isShowPanel, value);
            }
        }

        private string _lastSearch = string.Empty;

        private async Task<List<FilmInfo>> SearchTitle(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<FilmInfo>();

            text = text.Replace(" ", "+");
            string searchUrl = $@"https://subscene.com/subtitles/title?q={text}&l=";
            var values = new List<FilmInfo>();
            using (var webClient = new WebClient() { Encoding = Encoding.UTF8 })
            {
                try
                {
                    var html = new HtmlDocument();
                    var hString = await webClient.DownloadStringTaskAsync(new Uri(searchUrl, UriKind.RelativeOrAbsolute));
                    html.LoadHtml(hString);
                    var links = html.DocumentNode.Descendants("div").Where(n => n.GetAttributeValue("class", "").StartsWith("title"))
                        .SelectMany(x => x.Descendants("a").Where(n => n.GetAttributeValue("href", "").StartsWith("/subtitles")));
                    foreach (var link in links)
                    {
                        var title = link.InnerText.Trim();
                        var href = new Uri("https://subscene.com" + link.GetAttributeValue("href", ""));

                        var item = new FilmInfo()
                        {
                            Link = href.ToString(),
                            Title = title
                        };
                        values.Add(item);
                    }
                }
                catch
                {
                }
                finally
                {
                }
            }

            return values;
        }

        private async Task<List<SubtitleInfo>> GetSubtitleExtract(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return new List<SubtitleInfo>();

            var values = new List<SubtitleInfo>();
            using (var webClient = new WebClient() { Encoding = Encoding.UTF8 })
            {
                try
                {
                    var html = new HtmlDocument();
                    var hString = await webClient.DownloadStringTaskAsync(new Uri(url, UriKind.RelativeOrAbsolute));
                    html.LoadHtml(hString);
                    var links = html.DocumentNode.Descendants("a").Where(n => n.GetAttributeValue("href", "").StartsWith("/subtitles"));
                    var tasks = new List<Task<SubtitleInfo>>();
                    foreach (var link in links)
                    {
                        var taskParse = Task.Factory.StartNew<SubtitleInfo>(() =>
                        {
                            var language = link.Descendants("span")?.FirstOrDefault()?.InnerText?.Trim();
                            if (!_settingViewModel.DownloadCaptionLanguage.Contains(language))
                                return null;

                            var title = link.Descendants("span").ElementAt(1).InnerText.Trim();
                            var href = new Uri("https://subscene.com" + link.GetAttributeValue("href", ""));

                            return new SubtitleInfo()
                            {
                                Language = language,
                                LinkDownload = href.ToString(),
                                Title = title
                            };
                        }
                        );

                        tasks.Add(taskParse);
                    }

                    var result = await Task.WhenAll<SubtitleInfo>(tasks);
                    return result.Where(x => x != null).OrderBy(x => x.Language + x.Title).ToList();
                }
                catch
                {
                }
                finally
                {
                }
            }

            return values;
        }

        private async Task<string> GetLinkDownload(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            using (var webClient = new WebClient() { Encoding = Encoding.UTF8 })
            {
                try
                {
                    var html = new HtmlDocument();
                    var hString = await webClient.DownloadStringTaskAsync(new Uri(url, UriKind.RelativeOrAbsolute));
                    html.LoadHtml(hString);
                    var link = html.DocumentNode.Descendants("div").Where(n => n.GetAttributeValue("class", "").Equals("download")).FirstOrDefault()?.
                        Descendants().Where(x => x.GetAttributeValue("href", "").StartsWith(@"/subtitle/download")).FirstOrDefault()?.GetAttributeValue("href", "");
                    return "https://subscene.com" + link;
                }
                catch
                {
                }
                finally
                {
                }
            }

            return null;
        }

        private async Task<string> DownloadCaptionText(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                var client = new WebClient();
                var uri = new Uri(url);
                var zipStream = await client.OpenReadTaskAsync(uri);
                using (var zMemory = new MemoryStream())
                {
                    zipStream.CopyTo(zMemory);
                    zMemory.Seek(0, SeekOrigin.Begin);
                    using (var zip = ZipFile.Read(zMemory))
                    {
                        foreach (var zipEntry in zip)
                        {
                            using (var captionStream = new MemoryStream())
                            {
                                zipEntry.Extract(captionStream);
                                captionStream.Seek(0, SeekOrigin.Begin);

                                var sr = new StreamReader(captionStream);
                                var stringCaption = sr.ReadToEnd();
                                return stringCaption;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
            }
            return string.Empty;
        }
    }
}
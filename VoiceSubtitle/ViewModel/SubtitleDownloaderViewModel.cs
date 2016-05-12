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
using System.Reflection;
using Microsoft.Practices.ServiceLocation;

namespace VoiceSubtitle.ViewModel
{
    public class SubtitleDownloaderViewModel : ViewModelBase
    {
        private DispatchService dispatchService;
        private SettingViewModel settingViewModel;
        private NotifyViewModel notifyViewModel;
        public ICommand SearchCaptionOnline { get; }
        public ICommand AddPrimaryCaptionCommand { get; }
        public ICommand AddTranslatedCaptionCommand { get; }

        public ObservableCollection<FilmInfo> FilmInfos { get; private set; }
        public ObservableCollection<SubtitleInfo> SubtitleInfos { get; private set; }
        private List<SubtitleInfo> _subtitleInfos;

        public SubtitleDownloaderViewModel(SettingViewModel settingViewModel, DispatchService dispatchService, NotifyViewModel notifyViewModel)
        {
            this.dispatchService = dispatchService;
            this.settingViewModel = settingViewModel;
            this.notifyViewModel = notifyViewModel;
            FilmInfos = new ObservableCollection<FilmInfo>();
            SubtitleInfos = new ObservableCollection<SubtitleInfo>();

            _subtitleInfos = new List<SubtitleInfo>();
            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", (x) => IsShowPanel = false);
            SearchCaptionOnline = new ActionCommand(async (text) =>
            {
                IsShowPanel = true;

                if (lastSearch == text as string && _subtitleInfos.Count != 0)
                    return;

                IsFilmInfoDownloading = true;
                lastSearch = text as string;
                var films = await SearchTitle(text as string);
                IsFilmInfoDownloading = false;

                this.dispatchService.Invoke(() =>
                {
                    FilmInfos = new ObservableCollection<FilmInfo>(films);
                    RaisePropertyChanged("FilmInfos");
                });

                if (films.Count == 0)
                {
                    string textSearch = WebUtility.UrlEncode(text as string);
                    DownloadCaptionList($@"https://subscene.com/subtitles/title?q={textSearch}&l=");
                }
            });

            AddPrimaryCaptionCommand = new ActionCommand(async (x) =>
            {
                var captionFile = await LocalFileFromDownloader(x as SubtitleInfo);
                if (captionFile != null)
                    ServiceLocator.Current.GetInstance<PlayerViewModel>().UpdatePrivateCaption(captionFile);
            }
            );

            AddTranslatedCaptionCommand = new ActionCommand(async (x) =>
            {
                var captionFile = await LocalFileFromDownloader(x as SubtitleInfo);
                if (captionFile != null)
                    ServiceLocator.Current.GetInstance<PlayerViewModel>().UpdateTranslatedCaption(captionFile);
            }
          );
        }

        private FilmInfo currentFilmInfo;

        public FilmInfo CurrentFilmInfo
        {
            get
            {
                return currentFilmInfo;
            }
            set
            {
                Set(ref currentFilmInfo, value);
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
                dispatchService.Invoke(() =>
                {
                    _subtitleInfos.AddRange(captions);
                    SubtitleFilter = string.Empty;
                });
                IsCaptionListDownloading = false;
            });
        }

        private bool isFilmInfoDownloading;

        public bool IsFilmInfoDownloading
        {
            get
            {
                return isFilmInfoDownloading;
            }
            set
            {
                Set(ref isFilmInfoDownloading, value);
            }
        }

        private bool isCaptionListDownloading;

        public bool IsCaptionListDownloading
        {
            get
            {
                return isCaptionListDownloading;
            }
            set
            {
                Set(ref isCaptionListDownloading, value);
            }
        }

        private string searchText;

        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                Set(ref searchText, value);
            }
        }

        private string subtitleFilter;

        public string SubtitleFilter
        {
            get
            {
                return subtitleFilter;
            }
            set
            {
                Set(ref subtitleFilter, value);
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
                notifyViewModel.ShowMessageBox("No source found, please choose another");
                return null; ;
            }

            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\temp captions";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string captionFile = folder + $@"\{info.Title}.srt";
            File.WriteAllText(captionFile, captionText);
            return captionFile;
        }

        private bool isShowPanel;

        public bool IsShowPanel
        {
            get { return isShowPanel; }
            set
            {
                Set(ref isShowPanel, value);
            }
        }

        private string lastSearch = string.Empty;

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
                        string title = link.InnerText.Trim();
                        Uri href = new Uri("https://subscene.com" + link.GetAttributeValue("href", ""));

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
                            string language = link.Descendants("span")?.FirstOrDefault()?.InnerText?.Trim();
                            if (!settingViewModel.DownloadCaptionLanguage.Contains(language))
                                return null;

                            string title = link.Descendants("span").ElementAt(1).InnerText.Trim();
                            Uri href = new Uri("https://subscene.com" + link.GetAttributeValue("href", ""));

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
                WebClient client = new WebClient();
                Uri uri = new Uri(url);
                var zipStream = await client.OpenReadTaskAsync(uri);
                using (var zMemory = new MemoryStream())
                {
                    zipStream.CopyTo(zMemory);
                    zMemory.Seek(0, SeekOrigin.Begin);
                    using (ZipFile zip = ZipFile.Read(zMemory))
                    {
                        foreach (ZipEntry zip_entry in zip)
                        {
                            using (var captionStream = new MemoryStream())
                            {
                                zip_entry.Extract(captionStream);
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
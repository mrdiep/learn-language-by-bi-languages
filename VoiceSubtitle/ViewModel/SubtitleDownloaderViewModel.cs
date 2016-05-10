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
using Ionic.Zip;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VoiceSubtitle.ViewModel
{
    public class SubtitleDownloaderViewModel : ViewModelBase
    {
        public ICommand SearchCaptionOnline { get; }
        public ICommand AddPrimaryCaptionCommand { get; }
        public ICommand AddTranslatedCaptionCommand { get; }

        private SettingViewModel settingViewModel;

        public ObservableCollection<FilmInfo> FilmInfos { get; private set; }
        public ObservableCollection<SubtitleInfo> SubtitleInfos { get; private set; }

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

                Task.Factory.StartNew(async () =>
                {
                    var captions = await GetSubtitleExtract(value.Link);
                    dispatchService.Invoke(() => captions.ForEach(x => SubtitleInfos.Add(x)));
                });
            }
        }

        private DispatchService dispatchService;

        public SubtitleDownloaderViewModel(SettingViewModel settingViewModel, DispatchService dispatchService)
        {
            this.dispatchService = dispatchService;
            this.settingViewModel = settingViewModel;
            FilmInfos = new ObservableCollection<FilmInfo>();
            SubtitleInfos = new ObservableCollection<SubtitleInfo>();

            SearchCaptionOnline = new ActionCommand(async (text) =>
            {
                IsShowCaptionOnline = true;
                var films = await SearchTitle(text as string);
                FilmInfos.Clear();
                this.dispatchService.Invoke(() =>
                {
                    films.ForEach(x => FilmInfos.Add(x));
                });
                RaisePropertyChanged("FilmInfos");
            });

            AddPrimaryCaptionCommand = new ActionCommand(async (x) =>
            {
                SubtitleInfo info = x as SubtitleInfo;
                var captionText = await DownloadCaptionText(await GetLinkDownload(info.LinkDownload));
            }
            );
        }

        private bool isShowCaptionOnline;

        public bool IsShowCaptionOnline
        {
            get { return isShowCaptionOnline; }
            set
            {
                Set(ref isShowCaptionOnline, value);
            }
        }

        public async Task<List<FilmInfo>> SearchTitle(string text)
        {
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
                finally
                {
                }
            }

            return values;
        }

        public async Task<List<SubtitleInfo>> GetSubtitleExtract(string url)
        {
            var values = new List<SubtitleInfo>();
            using (var webClient = new WebClient() { Encoding = Encoding.UTF8 })
            {
                try
                {
                    var html = new HtmlDocument();
                    var hString = await webClient.DownloadStringTaskAsync(new Uri(url, UriKind.RelativeOrAbsolute));
                    html.LoadHtml(hString);
                    var links = html.DocumentNode.Descendants("a").Where(n => n.GetAttributeValue("href", "").StartsWith("/subtitles"));
                    foreach (var link in links)
                    {
                        string language = link.Descendants("span").ElementAt(0).InnerText.Trim();
                        if (settingViewModel.DownloadCaptionLanguage.Contains(language))
                        {
                            string title = link.Descendants("span").ElementAt(1).InnerText.Trim();
                            Uri href = new Uri("https://subscene.com" + link.GetAttributeValue("href", ""));

                            var item = new SubtitleInfo()
                            {
                                Language = language,
                                LinkDownload = href.ToString(),
                                Title = title
                            };
                            values.Add(item);
                        }
                    }
                }
                finally
                {
                }
            }

            return values;
        }

        public async Task<string> GetLinkDownload(string url)
        {
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
                finally
                {
                }
            }

            return null;
        }

        public async Task<string> DownloadCaptionText(string url)
        {
            try
            {
                WebClient client = new WebClient();
                Uri uri = new Uri(url);
                var zipStream = await client.OpenReadTaskAsync(uri);
                //using (var fileStream = File.Create(@"d:\t.zip"))
                //{
                //    zipStream.CopyTo(fileStream);
                //}

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
            finally
            {
            }
            return string.Empty;
        }
    }
}
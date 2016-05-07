using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VoiceSubtitle.Model;

namespace VoiceSubtitle.Helper
{
    public class CambridgeOnlineHelper
    {
        public void CrawToDatabase()
        {
            SQLiteConnection connection = new SQLiteConnection(@"Data Source=C:\Users\diepnguyenv\Downloads\anh_viet\anh_viet\anh_viet.db;Version=3;");
            List<string> words = new List<string>();
            Regex regex = new Regex(@"^[a-zA-Z]*$");
            string allword = string.Empty;
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "SELECT `word` FROM anh_viet";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string a = reader[0] as string;
                        if (regex.Match(a).Success && !(new Regex("^[A-Z]*$").Match(a).Success))
                        {
                            words.Add(a);
                        }
                    }
                }

                words = words.Distinct().ToList();
                allword = string.Join(" ", words);
            }
            Task.Factory.StartNew(async () =>
            {
               
                var tasks = new List<Task<WordPronunciation>>();
               
                foreach (var word in words)
                {
                    if (word.Trim().Length > 0)
                    {
                        tasks.Add(DownloadPrononciation(word));
                    }
                }

                var listItems = await Task.WhenAll(tasks);

                using (var conn = new SQLiteConnection(@"Data Source=C:\Users\diepnguyenv\Downloads\anh_viet\anh_viet\dict - Copy.db;Version=3;"))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(conn))
                    {
                        using (var transaction = conn.BeginTransaction())
                        {
                            // 100,000 inserts
                            foreach (var item in e2)
                            {
                                cmd.CommandText =
                                    "INSERT INTO english (`word`,`uk`,`us`,`ukvoice`,`usvoice`) VALUES (@word,@uk,@us,@ukvoice,@usvoice);";

                                cmd.Parameters.AddWithValue("@word", item.Text);
                                cmd.Parameters.AddWithValue("@uk", item.UK);
                                cmd.Parameters.AddWithValue("@us", item.US);
                                cmd.Parameters.AddWithValue("@ukvoice", item.UKVoiceLink);
                                cmd.Parameters.AddWithValue("@usvoice", item.USVoiceLink);

                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                    }
                    conn.Close();
                }

            });
        }

        private static async Task<WordPronunciation> DownloadPrononciation(string word)
        {
            using (var webClient = new WebClient() { Encoding = Encoding.UTF8 })
            {
                try
                {
                    string url = $@"http://dictionary.cambridge.org/dictionary/english/{HttpUtility.UrlEncode(word.ToLower())}";
                    var html = new HtmlDocument();
                    var hString = await webClient.DownloadStringTaskAsync(new Uri(url, UriKind.RelativeOrAbsolute));
                    html.LoadHtml(hString);

                    var root = html.DocumentNode;
                    var posHeader = root.Descendants().Where(n => n.GetAttributeValue("class", "").Equals("pos-header")).FirstOrDefault();

                    var audioUk = posHeader?.Descendants("span").Where(x => x.GetAttributeValue("class", "").Equals("uk"))?.FirstOrDefault()
                        .Descendants().Where(x => x.GetAttributeValue("title", "").Contains("listen to British English pronunciation"))
                        .FirstOrDefault().GetAttributeValue("data-src-mp3", "");

                    var prononceUk = posHeader?.Descendants("span").Where(x => x.GetAttributeValue("class", "").Equals("uk"))?.Skip(1)
                        .Select(p => p.Descendants().Where(x => x.GetAttributeValue("class", "").Contains("pron")).FirstOrDefault().InnerText);

                    var audioUs = posHeader?.Descendants("span").Where(x => x.GetAttributeValue("class", "").Equals("us"))?.FirstOrDefault()
                        .Descendants().Where(x => x.GetAttributeValue("title", "").Contains("listen to American pronunciation"))
                        .FirstOrDefault().GetAttributeValue("data-src-mp3", "");
                    var prononceUs = posHeader?.Descendants("span").Where(x => x.GetAttributeValue("class", "").Equals("us"))?.FirstOrDefault()
                        .Descendants().Where(x => x.GetAttributeValue("class", "").Equals("pron")).FirstOrDefault().InnerText;

                    var wordPronunciation = new WordPronunciation()
                    {
                        Text = word,
                        UK = string.Join(" ", prononceUk),
                        UKVoiceLink = audioUk,
                        US = prononceUs,
                        USVoiceLink = audioUs
                    };

                    return wordPronunciation;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}

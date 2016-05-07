using GalaSoft.MvvmLight;
using System.Threading.Tasks;
using VoiceSubtitle.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Linq;

namespace VoiceSubtitle.ViewModel
{
    public class CambridgeDictionaryViewModel : ViewModelBase
    {
        protected Dictionary<string, WordPronunciation> DictRef { get; private set; }

        public CambridgeDictionaryViewModel()
        {
            DictRef = new Dictionary<string, WordPronunciation>();
            Task.Factory.StartNew(FetchData);
        }

        private void FetchData()
        {
            string connectionString = $@"Data Source={Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Dict\dict.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM `english`";
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string word = reader["word"] as string;

                            var pro = new WordPronunciation()
                            {
                                Text = reader["word"] as string,
                                UK = reader["uk"] as string,
                                US = reader["us"] as string,
                                USVoiceLink = reader["usvoice"] as string,
                                UKVoiceLink = reader["ukvoice"] as string,
                            };

                            DictRef.Add(word, pro);
                        }
                    }
                }
            }
        }

        public List<WordPronunciation> Prononce(string sentence)
        {
            var tasks = new List<Task<WordPronunciation>>();
            var words = sentence.Split(@" ,/.-&!@#$%^&*()~`?<>;{}[],./\|".ToCharArray()).Distinct();

            var pronce = new List<WordPronunciation>();
            foreach (var word in words)
            {
                if (word.Trim().Length > 0)
                {
                    WordPronunciation WordPronunciation;
                    if (DictRef.TryGetValue(word.Trim().ToLower(), out WordPronunciation) && WordPronunciation != null)
                        pronce.Add(WordPronunciation);
                }
            }

            return pronce;
        }
    }
}
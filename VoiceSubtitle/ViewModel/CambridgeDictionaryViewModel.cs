using GalaSoft.MvvmLight;
using System.Threading.Tasks;
using VoiceSubtitle.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.ObjectModel;
using System;
using System.Windows.Input;

namespace VoiceSubtitle.ViewModel
{
    public class CambridgeDictionaryViewModel : ViewModelBase
    {
        private DispatchService dispatchService;
        protected Dictionary<string, WordPronunciation> DictRef { get; private set; }
        private List<WordPronunciation> items;



        public ICommand SearchCommand { get; }

        public CambridgeDictionaryViewModel(DispatchService dispatchService)
        {
            this.dispatchService = dispatchService;
            SearchCommand = new ActionCommand((x) => { });
            DictRef = new Dictionary<string, WordPronunciation>();
            FilteredItems = new ObservableCollection<WordPronunciation>();
            items = new List<WordPronunciation>();
            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", (x) => IsShowPanel = false);
            Task.Factory.StartNew(FetchData);
        }

        private ObservableCollection<WordPronunciation> filteredItems;

        public ObservableCollection<WordPronunciation> FilteredItems
        {
            get
            {
                return filteredItems;
            }
            set
            {
                Set(ref filteredItems, value);
            }
        }

        private void FetchData()
        {
            DateTime time = DateTime.Now;
            string connectionString = $@"Data Source={FolderManager.FolderDictionaryPath}\dict.db;Version=3;";
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
                            items.Add(pro);
                        }
                    }
                }
            }
            dispatchService.Invoke(() => FilteredItems = new ObservableCollection<WordPronunciation>(items));
            var time2 = DateTime.Now - time;
        }

        private bool isShowPanel;

        public bool IsShowPanel
        {
            get
            {
                return isShowPanel;
            }
            set
            {
                Set(ref isShowPanel, value);
            }
        }

        private string textFilter;

        public string TextFilter
        {
            get
            {
                return textFilter;
            }
            set
            {
                Set(ref textFilter, value);
                FilteredItems.Clear();
                if (string.IsNullOrWhiteSpace(value))
                {
                    dispatchService.Invoke(() => FilteredItems = new ObservableCollection<WordPronunciation>(items));
                    return;
                }

                var itemList = items.Where(x => x.Text.ToLower().Contains(textFilter.ToLower()));
                dispatchService.Invoke(() => FilteredItems = new ObservableCollection<WordPronunciation>(itemList));
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
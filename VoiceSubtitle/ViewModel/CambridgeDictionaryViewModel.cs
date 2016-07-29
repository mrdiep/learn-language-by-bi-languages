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
        private DispatchService _dispatchService;
        protected Dictionary<string, WordPronunciation> DictRef { get; private set; }
        private List<WordPronunciation> _items;



        public ICommand SearchCommand { get; }

        public CambridgeDictionaryViewModel(DispatchService dispatchService)
        {
            this._dispatchService = dispatchService;
            SearchCommand = new ActionCommand(x => { });
            DictRef = new Dictionary<string, WordPronunciation>();
            FilteredItems = new ObservableCollection<WordPronunciation>();
            _items = new List<WordPronunciation>();
            MessengerInstance.Register<bool>(this, "CloseAllFlyoutToken", x => IsShowPanel = false);
            Task.Factory.StartNew(FetchData);
        }

        private ObservableCollection<WordPronunciation> _filteredItems;

        public ObservableCollection<WordPronunciation> FilteredItems
        {
            get
            {
                return _filteredItems;
            }
            set
            {
                Set(ref _filteredItems, value);
            }
        }

        private void FetchData()
        {
            var time = DateTime.Now;
            string connectionString = $@"Data Source={FolderManager.FolderDictionaryPath}\dict.db;Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM `english`";
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var word = reader["word"] as string;

                            var pro = new WordPronunciation()
                            {
                                Text = reader["word"] as string,
                                UK = reader["uk"] as string,
                                US = reader["us"] as string,
                                USVoiceLink = reader["usvoice"] as string,
                                UKVoiceLink = reader["ukvoice"] as string,
                            };

                            DictRef.Add(word, pro);
                            _items.Add(pro);
                        }
                    }
                }
            }
            _dispatchService.Invoke(() => FilteredItems = new ObservableCollection<WordPronunciation>(_items));
            var time2 = DateTime.Now - time;
        }

        private bool _isShowPanel;

        public bool IsShowPanel
        {
            get
            {
                return _isShowPanel;
            }
            set
            {
                Set(ref _isShowPanel, value);
            }
        }

        private string _textFilter;

        public string TextFilter
        {
            get
            {
                return _textFilter;
            }
            set
            {
                Set(ref _textFilter, value);
                FilteredItems.Clear();
                if (string.IsNullOrWhiteSpace(value))
                {
                    _dispatchService.Invoke(() => FilteredItems = new ObservableCollection<WordPronunciation>(_items));
                    return;
                }

                var itemList = _items.Where(x => x.Text.ToLower().Contains(_textFilter.ToLower()));
                _dispatchService.Invoke(() => FilteredItems = new ObservableCollection<WordPronunciation>(itemList));
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
                    WordPronunciation wordPronunciation;
                    if (DictRef.TryGetValue(word.Trim().ToLower(), out wordPronunciation) && wordPronunciation != null)
                        pronce.Add(wordPronunciation);
                }
            }

            return pronce;
        }
    }
}
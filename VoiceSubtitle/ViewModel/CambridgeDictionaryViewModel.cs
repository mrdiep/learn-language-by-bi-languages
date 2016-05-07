using GalaSoft.MvvmLight;
using HtmlAgilityPack;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using VoiceSubtitle.Model;
using System.Collections.Generic;
using System.Web;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace VoiceSubtitle.ViewModel
{
    public class CambridgeDictionaryViewModel : ViewModelBase
    {
        public CambridgeDictionaryViewModel()
        {
            Task.Factory.StartNew(FetchData);
        }

        private void FetchData()
        {

            using (SQLiteConnection connection  = new SQLiteConnection())
            {
                using ( var command  = connection.CreateCommand())
                {
                    command.CommandText = "Select * from english";
                }
            }
        }

        protected Dictionary<string, WordPronunciation>   DataRef { get; }

        public async Task<WordPronunciation[]> Prononce(string sentence)
        {
            var tasks = new List<Task<WordPronunciation>>();
            var words = sentence.Split(@" ,/.-&!@#$%^&*()~`?<>;{}[],./\|".ToCharArray());

            foreach (var word in words)
            {
                if (word.Trim().Length > 0)
                {
                    tasks.Add(DownloadPrononciation(word));
                }
            }

            var listItems = await Task.WhenAll(tasks);

            return listItems;
        }

       
    }
}
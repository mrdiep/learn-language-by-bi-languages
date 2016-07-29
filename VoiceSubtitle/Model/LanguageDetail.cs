using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace VoiceSubtitle.Model
{
    public class LanguageDetail
    {
        public LanguageDetail(string text, List<WordPronunciation> Prononciations)
        {
            text = text.ToLower();
            PrononciationSentence = text;
            
            this.Prononciations = Prononciations.Where(x=>x!=null).Distinct().ToList();

            var words = text.Split(@" ,/.-&!@#$%^&*()~`?<>;{}[],./\|".ToCharArray());
            PrononciationSentence = string.Join(" ", words.Select(x => Prononciations.Where(t => t.Text == x)?.FirstOrDefault()?.US?? AddSpace(x.Length)));
        }

        private string AddSpace(int length)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                builder.Append(" ");
            }

            return builder.ToString();
        }

        public string PrononciationSentence { get; set; }
        public List<WordPronunciation> Prononciations { get; set; }
    }
}
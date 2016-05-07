using System.Collections.Generic;
using System.Text;

namespace VoiceSubtitle.Model
{
    public class LanguageDetail
    {
        public LanguageDetail(string text, List<WordPronunciation> Prononciations)
        {
            PrononciationSentence = text;
            this.Prononciations = Prononciations;

            foreach(var e in Prononciations)
            {
                text = text.ToLower();
                PrononciationSentence  = PrononciationSentence.Replace(e.Text, $"{e.Text} {e.US}");
            }
        }

        string AddSpace(int length)
        {
            StringBuilder builder = new StringBuilder();
                for(int i=0;i< length;i++)
            {
                builder.Append(" ");
            }

            return builder.ToString();
        }

        public string PrononciationSentence { get; set; }
        public List<WordPronunciation> Prononciations { get; set; }
    }
}
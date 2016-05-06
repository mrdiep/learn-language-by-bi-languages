using System.Collections.Generic;

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
                PrononciationSentence  = PrononciationSentence.Replace(e.Text, $"{e.Text} {e.US}");
            }
        }

        public string PrononciationSentence { get; set; }
        public List<WordPronunciation> Prononciations { get; set; }
    }
}
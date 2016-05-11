using System;

namespace VoiceSubtitle.Model
{
    public class WordPronunciation : IEquatable<WordPronunciation>
    {
        public string Text { get; set; }
        public string UK { get; set; }
        public string US { get; set; }
        public string UKVoiceLink { get; set; }
        public string USVoiceLink { get; set; }

        public bool Equals(WordPronunciation other)
        {
            return Text == other.Text;
        }
    }
}
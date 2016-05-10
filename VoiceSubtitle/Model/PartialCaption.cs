using System;

namespace VoiceSubtitle.Model
{
    public class PartialCaption
    {
        public PartialCaption(int index, TimeSpan from, TimeSpan to, string text)
        {
            Index = index;
            From = from;
            To = to;
            Text = text;
        }

        public int Index { get; }
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public string Text { get; }

        public override string ToString()
        {
            return $"{Text}";
        }
    }
}
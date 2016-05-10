using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VoiceSubtitle.Model;

namespace VoiceSubtitle.Helper
{
    public static class ConverterExtensions
    {
        private static Regex pattern_1 = new Regex(
       @"(?<sequence>\d+)\r\n(?<start>\d{2}\:\d{2}\:\d{2},\d{3}) --\> (?<end>\d{2}\:\d{2}\:\d{2},\d{3})\r\n(?<text>[\s\S]*?\r\n\r\n)",
       RegexOptions.Compiled | RegexOptions.ECMAScript);

        private static Regex pattern_2 = new Regex(
            @"(?<sequence>\d+)\n(?<start>\d{2}\:\d{2}\:\d{2},\d{3}) --\> (?<end>\d{2}\:\d{2}\:\d{2},\d{3})\n(?<text>[\s\S]*?\n\n)",
            RegexOptions.Compiled | RegexOptions.ECMAScript);

        private static readonly string FormatTime = @"hh\:mm\:ss\,fff";
        public static List<PartialCaption> LoadSubFormFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<PartialCaption>();
            }
            
            return LoadSourceFromText(File.ReadAllText(filePath));
            
        }

        public static List<PartialCaption> LoadSourceFromText(string textSource)
        {
            var captions = new List<PartialCaption>();
            var matches = pattern_1.Matches(textSource);
            if (matches.Count == 0)
                matches = pattern_2.Matches(textSource);
            foreach (Match e in matches)
            {
                var index = Convert.ToInt32(e.Groups[1].Value);
                var from = TimeSpan.ParseExact(e.Groups[2].Value, FormatTime, CultureInfo.InvariantCulture);
                var to = TimeSpan.ParseExact(e.Groups[3].Value, FormatTime, CultureInfo.InvariantCulture);
                var text = e.Groups[4].Value.Replace("\r\n", " ").Replace("  ", " ");
                text = text.Replace("\n", " ").Replace("  ", " ");
                text = text.Trim();

                text = WebHelper.InnerHtmlText(text);
                var caption = new PartialCaption(index, from, to, text);
                captions.Add(caption);
            }

            return captions;
        }

        public static double Intersects(TimeSpan f1, TimeSpan t1, TimeSpan f2, TimeSpan t2)
        {
            if (f1 > t1 || f2 > t2)
                return 0;

            if (f1 == t1 || f2 == t2)
                return 0; // No actual date range

            if (f1 == f2 || t1 == t2)
                return (t1 - f1).TotalMilliseconds; // If any set is the same time, then by default there must be some overlap.

            if (f1 < f2)
            {
                if (t1 > f2 && t1 < t2)
                    return (t1 - f2).TotalMilliseconds; // Condition 1

                if (t1 > t2)
                    return (t2 - f2).TotalMilliseconds; // Condition 3
            }
            else
            {
                if (t2 > f1 && t2 < t1)
                    return (t2 - f1).TotalMilliseconds; // Condition 2

                if (t2 > t1)
                    return (t1 - f1).TotalMilliseconds; // Condition 4
            }

            return 0;
        }

    }
}

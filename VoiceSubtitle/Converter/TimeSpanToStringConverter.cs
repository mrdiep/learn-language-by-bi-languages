using System;
using System.Globalization;
using System.Windows.Data;

namespace VoiceSubtitle.Converter
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
                return ((TimeSpan)value).ToString(@"hh\:mm\:ss");

            return "--:--";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeSpan.ParseExact(value as string, @"hh\:mm\:ss", CultureInfo.InvariantCulture);
        }
    }
}
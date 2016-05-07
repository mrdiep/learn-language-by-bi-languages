using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VoiceSubtitle.Converter
{
    public class TimeSpanToLongConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan)
                return ((TimeSpan)value).Ticks;

            if(value is TimeSpan? && !((TimeSpan?)value).HasValue)
                return ((TimeSpan?)value).Value.Ticks;

            if(value is Duration)
            {
                Duration duration = (Duration)value;
                if (duration.HasTimeSpan)
                    return duration.TimeSpan.Ticks;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return TimeSpan.FromSeconds(0);

            long tick = System.Convert.ToInt64(value);
            return TimeSpan.FromTicks(tick);
        }
    }
}
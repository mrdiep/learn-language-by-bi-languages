using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VoiceSubtitle.Converter
{
    public class IntGreaterZeroToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                int v = (int)value;
                if (v > 0)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
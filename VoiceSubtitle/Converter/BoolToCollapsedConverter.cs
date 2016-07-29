using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VoiceSubtitle.Converter
{
    public class BoolToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                var v = (bool)value;
                if (!v) return Visibility.Visible; else return Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
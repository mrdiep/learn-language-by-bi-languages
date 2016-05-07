using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VoiceSubtitle.Converter
{
    public class VisibleInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
                return ((Visibility)value) == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
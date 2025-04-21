using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Autoinstaller.Converters
{
    public class HighlightIfNotEqualConverter : IValueConverter
    {
        // Параметр ConverterParameter задаёт ожидаемое значение.
        // Если фактическое значение не совпадает, возвращается красный Brush, иначе — черный.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string expected = parameter as string;
            string actual = value as string;
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(actual))
                return Brushes.Black;
            return actual == expected ? Brushes.Black : Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

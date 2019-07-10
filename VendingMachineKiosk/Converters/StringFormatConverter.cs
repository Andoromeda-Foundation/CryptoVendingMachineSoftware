using System;
using Windows.UI.Xaml.Data;

namespace VendingMachineKiosk.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (parameter)
            {
                case null:
                    return value;
                case string format:
                    return string.Format(format, value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
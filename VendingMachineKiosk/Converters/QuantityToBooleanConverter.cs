using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace VendingMachineKiosk.Converters
{
    public class QuantityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int quantity)
            {
                return quantity > 0;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"expect int but got {value.GetType().Name}");
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

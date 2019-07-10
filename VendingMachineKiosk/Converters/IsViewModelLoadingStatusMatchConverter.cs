using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VendingMachineKiosk.ViewModels;

namespace VendingMachineKiosk.Converters
{
    public class IsViewModelLoadingStatusMatchToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ViewModelLoadingStatus status && parameter != null)
            {
                var targetStatus = (ViewModelLoadingStatus)parameter;
                return status == targetStatus ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IsViewModelLoadingStatusMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ViewModelLoadingStatus status && parameter != null)
            {
                var targetStatus = (ViewModelLoadingStatus)parameter;
                return status == targetStatus;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

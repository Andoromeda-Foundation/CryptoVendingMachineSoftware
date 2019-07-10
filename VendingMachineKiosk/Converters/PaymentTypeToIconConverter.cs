using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using XiaoTianQuanProtocols;

namespace VendingMachineKiosk.Converters
{
    public class PaymentTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PaymentType type)
            {
                switch (type)
                {
                    case PaymentType.LightningNetwork:
                        return new BitmapImage(new Uri("ms-appx:///Assets/lightning-network.png"));
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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

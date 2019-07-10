using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace VendingMachineKiosk.Extensions
{
    public static class FrameExtensions
    {
        public static void NavigateWithoutHistory<T>(this Page page, object parameter = null) where T : class
        {
            page.Frame.NavigateToType(typeof(T), parameter, new Windows.UI.Xaml.Navigation.FrameNavigationOptions
            {
                IsNavigationStackEnabled = false
            });
        }

        public static void NavigateWithoutHistory<T>(this Frame frame, object parameter = null) where T : class
        { 
            frame.NavigateToType(typeof(T), parameter, new Windows.UI.Xaml.Navigation.FrameNavigationOptions
            {
                IsNavigationStackEnabled = false
            });
        }


        public static void Navigate<T>(this Page page, object parameter = null) where T : class
        {
            page.Frame.Navigate(typeof(T), parameter);
        }

        public static void Navigate<T>(this Frame frame, object parameter = null) where T : class
        {
            frame.Navigate(typeof(T), parameter);
        }
    }
}

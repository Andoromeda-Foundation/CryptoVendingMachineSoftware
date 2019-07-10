using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using VendingMachineKiosk.ViewModels;
using XiaoTianQuanProtocols.DataObjects;
using VendingMachineKiosk.Extensions;
using XiaoTianQuanProtocols;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VendingMachineKiosk.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductPayment : Page
    {
        public ProductPayment()
        {
            this.InitializeComponent();
            Messenger.Default.Register<(ProductInformation, PaymentType)>(this, ProductPaymentViewModel.MessageChannelId,
                DisplayPaymentInstruction);
        }

        private ProductPaymentViewModel ViewModel => (ProductPaymentViewModel)DataContext;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var product = (ProductInformation)e.Parameter;
            ViewModel.ProductInformation = product;
        }

        private async void DisplayPaymentInstruction((ProductInformation product, PaymentType paymentType) param)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                () => FramePaymentInstruction.Navigate<PaymentInstruction>(param));
        }

        public ICommand CommandGoBack => new RelayCommand(() =>
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        });

        public ICommand CommandGoHome => new RelayCommand(() => { this.Navigate<MainPage>(); });
    }
}

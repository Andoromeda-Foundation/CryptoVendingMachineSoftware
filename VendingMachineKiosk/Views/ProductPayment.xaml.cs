using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using VendingMachineKiosk.Services;
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
            Messenger.Default.Register<Messages>(this, ProcessMessage);
        }

        private void ProcessMessage(Messages msg)
        {
            switch (msg)
            {
                case Messages.InvalidatePaymentSession:
                    break;
                case Messages.LoadProductPaymentViewModel:
                    break;
                case Messages.LoadPaymentInstructionPage:
                    FramePaymentInstruction.Navigate<PaymentInstruction>();
                    break;
                case Messages.LoadPaymentInstructionViewModel:
                    break;
                case Messages.UnloadProductPaymentViewModel:
                    break;
                case Messages.UnloadPaymentInstructionViewModel:
                    break;
                case Messages.LoadProductSelectionViewModel:
                    break;
                case Messages.UnloadProductSelectionViewModel:
                    break;
                case Messages.ProductReleasing:
                    FramePaymentInstruction.Navigate<ProductReleasing>();
                    break;
                case Messages.ProductReleased:
                    FramePaymentInstruction.Navigate<ProductReleased>();
                    break;
                default:
                    break;
            }
        }

        private ProductPaymentViewModel ViewModel => (ProductPaymentViewModel)DataContext;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Messenger.Default.Send(Messages.LoadProductPaymentViewModel);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Messenger.Default.Send(Messages.UnloadProductPaymentViewModel);
        }

        public ICommand CommandGoBack => new RelayCommand(() =>
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        });

        public ICommand CommandGoHome => new RelayCommand(() => this.Navigate<MainPage>());

        public ICommand CommandRetryLoad => new RelayCommand(async () => await ViewModel.LoadAsync());
    }
}

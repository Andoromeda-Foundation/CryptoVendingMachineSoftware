using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using VendingMachineKiosk.Exceptions;
using VendingMachineKiosk.Services;
using XiaoTianQuanProtocols.DataObjects;
using XiaoTianQuanProtocols.Extensions;

namespace VendingMachineKiosk.ViewModels
{
    public class ProductSelectionViewModel : AsyncLoadingViewModelBase
    {
        public VendingStateViewModelService VendingStateViewModelService { get; }
        private readonly ServerRequester _requester;
        private readonly LoggingChannel _logging;
        private readonly INavigationService _navigationService;
        private ObservableCollection<ProductInformation> _products = new ObservableCollection<ProductInformation>();

        public ObservableCollection<ProductInformation> Products
        {
            get => _products;
            set
            {
                _products = value;
                RaisePropertyChanged();
            }
        }

        public ProductSelectionViewModel(ServerRequester requester, LoggingChannel logging,
            VendingStateViewModelService vendingStateViewModelService, INavigationService navigationService)
        {
            VendingStateViewModelService = vendingStateViewModelService;
            _requester = requester;
            _logging = logging;
            _navigationService = navigationService;
        }

        public ICommand CommandSelectedProduct => new RelayCommand(SelectProduct);

        private void SelectProduct()
        {
            _navigationService.NavigateTo("ProductPayment");
        }

        public override async Task LoadAsync()
        {
            VendingStateViewModelService.ResetVendingState();

            ViewModelLoadingStatus = ViewModelLoadingStatus.Loading;
            try
            {
                var products = await _requester.GetProductList();
                Products = new ObservableCollection<ProductInformation>(products.Where(p => p.Prices.Count > 0));
                ViewModelLoadingStatus = ViewModelLoadingStatus.Loaded;
            }
            catch (VendingMachineKioskException e)
            {
                _logging.LogMessage($"Product selection viewmodel err: {e.GetInnerMessages()}", LoggingLevel.Error);
                ErrorMessage = e.GetInnerMessages();
                ViewModelLoadingStatus = ViewModelLoadingStatus.Error;
            }
        }

        protected override async Task ProcessMessageAsync(Messages msg)
        {
            switch (msg)
            {
                case Messages.InvalidatePaymentSession:
                    break;
                case Messages.LoadProductPaymentViewModel:
                    break;
                case Messages.CeaseProductPaymentViewModel:
                    break;
                case Messages.LoadPaymentInstructionPage:
                    break;
                case Messages.LoadPaymentInstructionViewModel:
                    break;
                case Messages.CeasePaymentInstructionViewModel:
                    break;
                case Messages.LoadProductSelectionViewModel:
                    await LoadAsync();
                    break;
                case Messages.CeaseProductSelectionViewModel:
                    IsCeased = true;
                    break;
                default:
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using GalaSoft.MvvmLight;
using VendingMachineKiosk.Exceptions;
using VendingMachineKiosk.Services;
using XiaoTianQuanProtocols.DataObjects;

namespace VendingMachineKiosk.ViewModels
{
    public class ProductSelectionViewModel : AsyncLoadingViewModelBase
    {
        private readonly ServerRequester _requester;
        private readonly LoggingChannel _logging;
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

        public ProductSelectionViewModel(ServerRequester requester, LoggingChannel logging)
        {
            _requester = requester;
            _logging = logging;
        }

        public override async Task LoadAsync()
        {
            ViewModelLoadingStatus = ViewModelLoadingStatus.Loading;
            try
            {
                var products = await _requester.GetProductList();
                Products = new ObservableCollection<ProductInformation>(products.Where(p => p.Prices.Count > 0));
                ViewModelLoadingStatus = ViewModelLoadingStatus.Loaded;
            }
            catch (VendingMachineKioskException e)
            {
                _logging.LogMessage($"Product selection viewmodel err: {e.Message}", LoggingLevel.Error);
                ErrorMessage = e.Message;
                ViewModelLoadingStatus = ViewModelLoadingStatus.Error;
            }
        }
    }
}

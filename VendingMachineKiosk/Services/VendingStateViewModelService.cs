using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using XiaoTianQuanProtocols;
using XiaoTianQuanProtocols.DataObjects;

namespace VendingMachineKiosk.Services
{
    public class VendingStateViewModelService : ViewModelBase
    {
        private ProductInformation _selectedProduct;

        public ProductInformation SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                RaisePropertyChanged();
            }
        }

        private PaymentType _paymentType;

        public PaymentType PaymentType
        {
            get => _paymentType;
            set
            {
                _paymentType = value;
                RaisePropertyChanged();
            }
        }

        private Guid _transactionId;

        public Guid TransactionId
        {
            get => _transactionId;
            set
            {
                _transactionId = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TransactionValid));
            }
        }

        public bool TransactionValid => TransactionId != Guid.Empty;

        public void ResetVendingState()
        {
            SelectedProduct = null;
            TransactionId = Guid.Empty;
        }

    }
}

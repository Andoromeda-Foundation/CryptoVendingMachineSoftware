using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using XiaoTianQuanProtocols;
using XiaoTianQuanProtocols.DataObjects;
using XiaoTianQuanProtocols.VendingMachineRequests;

namespace VendingMachineKiosk.ViewModels
{
    public class ProductPaymentViewModel : ViewModelBase
    {
        public static readonly Guid MessageChannelId = Guid.NewGuid();

        public Guid LockToken { get; set; }

        private ProductInformation _productInformation;

        // This should be set
        public ProductInformation ProductInformation
        {
            get => _productInformation;
            set
            {
                _productInformation = value;
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

        public ProductPaymentViewModel()
        {
            if (IsInDesignModeStatic)
            {
                ProductInformation = new ProductInformation
                {
                    Name = "Weed",
                    Description = "Super strong super nice",
                    Image = new Uri(
                        "https://xiaotianquandev.blob.core.windows.net/images/cfe0b1b0-abbe-4ffb-9e7b-e54667a4b5d0.jpg"),
                    Quantity = 6,
                    Prices = new Dictionary<PaymentType, double>
                    {
                        { XiaoTianQuanProtocols.PaymentType.LightningNetwork, 6000 }
                    },
                    Slot = "1"
                };
            }
        }

        public ICommand CommandSelectPaymentType => new RelayCommand(CreateTransaction);

        private void CreateTransaction()
        {
            if (ProductInformation == null)
            {
                throw new InvalidOperationException("ProductInformation is null");
            }

            Messenger.Default.Send<(ProductInformation, PaymentType)>((ProductInformation, PaymentType), MessageChannelId);
        }
    }
}

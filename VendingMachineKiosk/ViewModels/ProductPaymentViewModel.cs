using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using VendingMachineKiosk.Exceptions;
using VendingMachineKiosk.Services;
using XiaoTianQuanProtocols;
using XiaoTianQuanProtocols.DataObjects;
using XiaoTianQuanProtocols.Extensions;
using XiaoTianQuanProtocols.VendingMachineRequests;

namespace VendingMachineKiosk.ViewModels
{
    public class ProductPaymentViewModel : AsyncLoadingViewModelBase
    {
        public VendingStateViewModelService VendingStateViewModelService { get; }

        private readonly Timer _displayTimer = new Timer(100);
        private readonly ServerRequester _requester;
        private readonly IVendingMachineControlService _vendingMachineControlService;

        private int _displayTimeRemaining;
        private bool _isDisplayTimerVisible;
        private PaymentType _paymentType;
        private DateTime _transactionExpiry;

        public ProductPaymentViewModel(ServerRequester requester,
            VendingStateViewModelService vendingStateViewModelService,
            IVendingMachineControlService vendingMachineControlService)
        {
            VendingStateViewModelService = vendingStateViewModelService;
            _requester = requester;
            _vendingMachineControlService = vendingMachineControlService;
            _displayTimer.Elapsed += DisplayTimer_Elapsed;

            if (IsInDesignModeStatic)
                VendingStateViewModelService.SelectedProduct = new ProductInformation
                {
                    Name = "Weed",
                    Description = "Super strong super nice",
                    Image = new Uri(
                        "https://xiaotianquandev.blob.core.windows.net/images/cfe0b1b0-abbe-4ffb-9e7b-e54667a4b5d0.jpg"),
                    Quantity = 6,
                    Prices = new Dictionary<PaymentType, double>
                    {
                        {PaymentType.LightningNetwork, 6000}
                    },
                    Slot = "1"
                };
        }

        protected override async Task ProcessMessageAsync(Messages msg)
        {
            switch (msg)
            {
                case Messages.InvalidatePaymentSession:
                    IsPayable = false;
                    break;
                case Messages.LoadProductPaymentViewModel:
                    await LoadAsync();
                    break;
                case Messages.LoadPaymentInstructionPage:
                    break;
                case Messages.LoadPaymentInstructionViewModel:
                    break;
                case Messages.UnloadProductPaymentViewModel:
                    CeaseViewModel();
                    _vendingMachineControlService.RemovePendingTransaction(VendingStateViewModelService.TransactionId);
                    VendingStateViewModelService.ResetVendingState();
                    break;
                case Messages.UnloadPaymentInstructionViewModel:
                    break;
                case Messages.LoadProductSelectionViewModel:
                    break;
                case Messages.UnloadProductSelectionViewModel:
                    break;
                case Messages.ProductReleasing:
                    StopDisplayTimer();
                    IsPayable = false;
                    break;
                case Messages.ProductReleased:
                    break;
                default:
                    break;
            }
        }

        // This should be set
        public ProductInformation ProductInformation => VendingStateViewModelService.SelectedProduct;

        public PaymentType PaymentType
        {
            get => _paymentType;
            set
            {
                _paymentType = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDisplayTimerVisible
        {
            get => _isDisplayTimerVisible;
            set
            {
                _isDisplayTimerVisible = value;
                RaisePropertyChangedMarshaled();
            }
        }

        public ICommand CommandSelectPaymentType => new RelayCommand(SelectPaymentType);

        public DateTime TransactionExpiry
        {
            get => _transactionExpiry;
            set
            {
                _transactionExpiry = value;
                RaisePropertyChanged();
            }
        }

        public int DisplayTimeRemaining
        {
            get => _displayTimeRemaining;
            set
            {
                _displayTimeRemaining = value;
                RaisePropertyChangedMarshaled();
            }
        }

        private bool _isPayable = true;

        public bool IsPayable
        {
            get => _isPayable;
            set
            {
                _isPayable = value;
                RaisePropertyChanged();
            }
        }


        public override async Task LoadAsync()
        {
            ViewModelLoadingStatus = ViewModelLoadingStatus.Loading;

            try
            {
                ViewModelLoadingStatus = ViewModelLoadingStatus.Loaded;

                await CreateTransaction();

                if (ProductInformation.Prices.Count > 0)
                {
                    PaymentType = ProductInformation.Prices.First().Key;
                    SelectPaymentType();
                }
            }
            catch (VendingMachineKioskException e)
            {
                ViewModelLoadingStatus = ViewModelLoadingStatus.Error;
                ErrorMessage = e.GetInnerMessages();
            }
        }

        private async Task CreateTransaction()
        {
            if (ProductInformation == null) throw new InvalidOperationException("ProductInformation is null");

            CreateTransactionResponse result = await _requester.CreateTransaction(new CreateTransactionRequest
            {
                Slot = ProductInformation.Slot
            });
            TransactionExpiry = result.PaymentDisplayTimeout.ToLocalTime();
            DisplayTimeRemaining = (int)(result.PaymentDisplayTimeout - DateTime.UtcNow).TotalSeconds;
            StartDisplayTimer();
            VendingStateViewModelService.TransactionId = result.TransactionId;
            _vendingMachineControlService.AddPendingTransaction(result.TransactionId);
        }

        private void DisplayTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsCeased)
            {
                StopDisplayTimer();
                return;
            }

            var timeRemaining = (int)Math.Floor((TransactionExpiry - DateTime.Now).TotalSeconds);
            DisplayTimeRemaining = timeRemaining;
            if (timeRemaining <= 0)
            {
                StopDisplayTimer();
                RunInUiThread(() => MessengerInstance.Send(Messages.InvalidatePaymentSession));
            }
        }

        private void SelectPaymentType()
        {
            if (ProductInformation == null)
                throw new InvalidOperationException("ProductInformation is null");

            Messenger.Default.Send(Messages.LoadPaymentInstructionPage);
        }

        private void StartDisplayTimer()
        {
            IsDisplayTimerVisible = true;
            _displayTimer.Start();
        }

        private void StopDisplayTimer()
        {
            _displayTimer.Stop();
            IsDisplayTimerVisible = false;
        }
    }
}
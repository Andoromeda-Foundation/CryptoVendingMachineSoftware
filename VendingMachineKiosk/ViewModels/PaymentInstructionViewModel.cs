using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using QRCoder;
using VendingMachineKiosk.Exceptions;
using VendingMachineKiosk.Extensions;
using VendingMachineKiosk.Services;
using XiaoTianQuanProtocols;
using XiaoTianQuanProtocols.VendingMachineRequests;

namespace VendingMachineKiosk.ViewModels
{
    public class PaymentInstructionViewModel : AsyncLoadingViewModelBase
    {
        public VendingStateViewModelService VendingStateViewModelService { get; }
        private readonly ServerRequester _requester;
        private ImageSource _paymentQrCode;

        private bool _paymentQrCodeNotValid;

        public PaymentInstructionViewModel(ServerRequester requester, VendingStateViewModelService vendingStateViewModelService)
        {
            VendingStateViewModelService = vendingStateViewModelService;
            _requester = requester;
            MessengerInstance.Register<Messages>(this, ProcessMessages);
        }

        public PaymentType PaymentType => VendingStateViewModelService.PaymentType;

        public ImageSource PaymentQrCode
        {
            get => _paymentQrCode;
            set
            {
                _paymentQrCode = value;
                RaisePropertyChanged();
            }
        }

        public bool PaymentQrCodeNotValid
        {
            get => _paymentQrCodeNotValid;
            set
            {
                _paymentQrCodeNotValid = value;
                RaisePropertyChanged();
            }
        }

        public Guid TransactionId => VendingStateViewModelService.TransactionId;

        public ICommand CommandRetryLoad => new RelayCommand(async () => await LoadAsync());

        public override async Task LoadAsync()
        {
            if (ViewModelLoadingStatus == ViewModelLoadingStatus.Loading)
                return;

            ViewModelLoadingStatus = ViewModelLoadingStatus.Loading;
            try
            {
                GetPaymentInstructionResponse response = await _requester.GetPaymentInstruction(
                    new GetPaymentInstructionRequest
                    {
                        PaymentType = PaymentType,
                        TransactionId = TransactionId
                    });

                switch (PaymentType)
                {
                    case PaymentType.LightningNetwork:
                        PaymentQrCode = await GenerateQr(response.PaymentCode);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ViewModelLoadingStatus = ViewModelLoadingStatus.Loaded;
            }
            catch (VendingMachineKioskException e)
            {
                ViewModelLoadingStatus = ViewModelLoadingStatus.Error;
                ErrorMessage = e.Message;
            }
        }

        private async Task<WriteableBitmap> GenerateQr(string source)
        {
            var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData =
                qrGenerator.CreateQrCode(Encoding.UTF8.GetBytes(source), QRCodeGenerator.ECCLevel.L);
            var qrCode = new BitmapByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            using (var stream = new InMemoryRandomAccessStream())
            {
                using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(qrCodeImage);
                    await writer.StoreAsync();
                }

                var bitmap = new WriteableBitmap(200, 200);
                await bitmap.SetSourceAsync(stream);
                return bitmap;
            }
        }

        private async void ProcessMessages(Messages msg)
        {
            switch (msg)
            {
                case Messages.InvalidatePaymentSession:
                    PaymentQrCodeNotValid = true;
                    break;
                case Messages.LoadProductPaymentViewModel:
                    break;
                case Messages.LoadPaymentInstructionPage:
                    break;
                case Messages.LoadPaymentInstructionViewModel:
                    await LoadAsync();
                    break;
            }
        }
    }
}
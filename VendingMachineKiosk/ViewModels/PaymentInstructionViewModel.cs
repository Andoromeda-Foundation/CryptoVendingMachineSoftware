using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using QRCoder;
using VendingMachineKiosk.Exceptions;
using VendingMachineKiosk.Extensions;
using VendingMachineKiosk.Services;
using XiaoTianQuanProtocols;
using XiaoTianQuanProtocols.DataObjects;
using XiaoTianQuanProtocols.VendingMachineRequests;

namespace VendingMachineKiosk.ViewModels
{
    public class PaymentInstructionViewModel : AsyncLoadingViewModelBase
    {
        private readonly ServerRequester _requester;

        public PaymentType PaymentType { get; set; }
        public string Slot { get; set; }
        private ImageSource _paymentQrCode = new WriteableBitmap(500, 500);
        private readonly Timer _paymentTimer = new Timer(1000);

        private int _idleTimerCount;

        private int IdleTimerCount
        {
            get => _idleTimerCount;
            set
            {
                _idleTimerCount = value;
                RaisePropertyChanged(nameof(IdleTimerRemaining));
            }
        }

        private int _displayTimeout = -1;

        public int IdleTimerRemaining
        {
            get
            {
                if (_displayTimeout == -1)
                    return 0;

                return _displayTimeout - _idleTimerCount;
            }
        }


        public ImageSource PaymentQrCode
        {
            get => _paymentQrCode;
            set
            {
                _paymentQrCode = value;
                RaisePropertyChanged();
            }
        }

        private bool _paymentQrCodeNotValid = true;

        public bool PaymentQrCodeNotValid
        {
            get => _paymentQrCodeNotValid;
            set
            {
                _paymentQrCodeNotValid = value;
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
            }
        }

        public ICommand CommandRetryLoad => new RelayCommand(async () => await LoadAsync());

        public PaymentInstructionViewModel(ServerRequester requester)
        {
            _requester = requester;
            _paymentTimer.Elapsed += PaymentTimerElapsed;
        }

        private async void PaymentTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_displayTimeout == -1)
            {
                return;
            }

            ++_idleTimerCount;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.High,
                () =>
                {
                    RaisePropertyChanged(nameof(IdleTimerRemaining));

                    if (IdleTimerCount >= _displayTimeout)
                    {
                        StopPaymentTimer();
                        PaymentQrCodeNotValid = true;
                    }
                });
        }

        private void StartPaymentTimer()
        {
            _paymentTimer.Start();
        }

        private void StopPaymentTimer()
        {
            _paymentTimer.Stop();
        }

        public override async Task LoadAsync()
        {
            if (ViewModelLoadingStatus == ViewModelLoadingStatus.Loading)
                return;

            ViewModelLoadingStatus = ViewModelLoadingStatus.Loading;
            try
            {
                var response = await _requester.CreateTransaction(new CreateTransactionRequest
                {
                    PaymentType = PaymentType,
                    Slot = Slot,
                });

                _displayTimeout = response.PaymentDisplayTimeout;
                TransactionId = response.TransactionId;

                switch (PaymentType)
                {
                    case PaymentType.LightningNetwork:
                        var qrCode = await GenerateQr(response.PaymentCode);
                        await qrCode.SaveAsync($"{TransactionId}.png");
                        PaymentQrCode = qrCode;
                        PaymentQrCodeNotValid = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ViewModelLoadingStatus = ViewModelLoadingStatus.Loaded;
                StartPaymentTimer();
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
            var qrCodeData = qrGenerator.CreateQrCode(Encoding.UTF8.GetBytes(source), QRCodeGenerator.ECCLevel.H);
            var qrCode = new BitmapByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            using (var stream = new InMemoryRandomAccessStream())
            {
                using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(qrCodeImage);
                    await writer.StoreAsync();
                }
                var bitmap = new WriteableBitmap(500, 500);
                await bitmap.SetSourceAsync(stream);
                return bitmap;
            }
        }
    }
}

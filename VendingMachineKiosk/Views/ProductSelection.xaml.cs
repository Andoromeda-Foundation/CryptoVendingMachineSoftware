using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using VendingMachineKiosk.Annotations;
using VendingMachineKiosk.Extensions;
using VendingMachineKiosk.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VendingMachineKiosk.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductSelection : Page, INotifyPropertyChanged
    {
        private readonly Timer _timer = new Timer(1000);
        private int _idleCounter;

        private int IdleCounter
        {
            get => _idleCounter;
            set
            {
                _idleCounter = value;
                OnPropertyChanged(nameof(RemainingTime));
            }
        }

        private int RemainingTime => Config.ProductSelectionIdleTimeout - IdleCounter;

        public ICommand CommandGoBack => new RelayCommand(() => { this.NavigateWithoutHistory<MainPage>(); });

        public ICommand CommandRetry => new RelayCommand(async () => await ((ViewModels.ProductSelectionViewModel)DataContext).LoadAsync());

        public ProductSelection()
        {
            this.InitializeComponent();
            _timer.Elapsed += IdleTimerElapsed;
        }

        private async void IdleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ++IdleCounter;
            if (IdleCounter == Config.ProductSelectionIdleTimeout)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    this.NavigateWithoutHistory<MainPage>();
                });
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Load and display, then start timer
            Messenger.Default.Send(Messages.LoadProductSelectionViewModel);

            _timer.Start();
            IdleCounter = 0;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Messenger.Default.Send(Messages.CeaseProductSelectionViewModel);
            _timer.Stop();
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            IdleCounter = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

    }
}

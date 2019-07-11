using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using GalaSoft.MvvmLight;

namespace VendingMachineKiosk.ViewModels
{
    public enum ViewModelLoadingStatus
    {
        NotLoaded,
        Loading,
        Loaded,
        Error,
    }

    public abstract class AsyncLoadingViewModelBase : MessagingViewModelBase
    {
        private ViewModelLoadingStatus _viewModelViewModelLoadingStatus = ViewModelLoadingStatus.NotLoaded;

        public ViewModelLoadingStatus ViewModelLoadingStatus
        {
            get => _viewModelViewModelLoadingStatus;
            protected set
            {
                _viewModelViewModelLoadingStatus = value;
                RaisePropertyChanged();
            }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                RaisePropertyChanged();
            }
        }

        public abstract Task LoadAsync();

        public virtual Task UnloadAsync()
        {
            return Task.CompletedTask;
        }

        protected void RunInUiThread(DispatchedHandler action, CoreDispatcherPriority priority = CoreDispatcherPriority.High)
        {
            Task.Run(async () => await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(priority, action));
        }

        public virtual void RaisePropertyChangedMarshaled([CallerMemberName] string propertyName = null)
        {
            RunInUiThread(() => RaisePropertyChanged(propertyName));
        }
    }
}

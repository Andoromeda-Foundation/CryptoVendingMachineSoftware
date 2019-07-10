using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public abstract class AsyncLoadingViewModelBase : ViewModelBase
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
    }
}

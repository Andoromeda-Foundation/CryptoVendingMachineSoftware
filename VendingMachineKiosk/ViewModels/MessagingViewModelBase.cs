using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace VendingMachineKiosk.ViewModels
{
    public abstract class MessagingViewModelBase : ViewModelBase
    {
        protected MessagingViewModelBase()
        {
            MessengerInstance.Register<Messages>(this, ProcessMessage);
        }

        private async void ProcessMessage(Messages msg)
        {
            if (IsCeased)
                return;

            await ProcessMessageAsync(msg);
        }

        protected abstract Task ProcessMessageAsync(Messages msg);

        protected bool IsCeased { get; set; }
    }
}

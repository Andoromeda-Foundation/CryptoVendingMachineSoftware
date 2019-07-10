using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Extensions.DependencyInjection;
using VendingMachineKiosk.ViewModels;
using VendingMachineKiosk.Views;

namespace VendingMachineKiosk.Helpers
{
    public class ViewModelLocator
    {
        public T GetService<T>()
        {
            return ViewModelBase.IsInDesignModeStatic ? SimpleIoc.Default.GetInstance<T>() : App.ServiceProvider.GetService<T>();
        }

        public ViewModelLocator()
        {
            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<ProductSelectionViewModel>();
                SimpleIoc.Default.Register<ProductPaymentViewModel>();
                SimpleIoc.Default.Register<PaymentInstructionViewModel>();
            }
            else
            {
                App.ConfigureDi(services =>
                {
                    services.AddTransient<ProductSelectionViewModel>();
                    services.AddTransient<ProductPaymentViewModel>();
                    services.AddTransient<PaymentInstructionViewModel>();

                    var navigationService = new NavigationService();
                    navigationService.Configure("MainPage", typeof(MainPage));
                    navigationService.Configure("ProductSelection", typeof(ProductSelection));
                    navigationService.Configure("ProductPayment", typeof(ProductPayment));
                    navigationService.Configure("PaymentInstruction", typeof(PaymentInstruction));
                    services.AddSingleton<INavigationService>(navigationService);
                });
            }
        }

        public ProductSelectionViewModel ProductSelectionViewModel => GetService<ProductSelectionViewModel>();
        public ProductPaymentViewModel ProductPaymentViewModel => GetService<ProductPaymentViewModel>();
        public PaymentInstructionViewModel PaymentInstructionViewModel => GetService<PaymentInstructionViewModel>();
    }
}

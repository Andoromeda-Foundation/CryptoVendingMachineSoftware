using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using VendingMachineKiosk.Helpers;
using VendingMachineKiosk.Services;

namespace VendingMachineKiosk
{
    sealed partial class App : Application
    {
        private static readonly IServiceCollection ServiceCollection = new ServiceCollection();

        public static IServiceProvider ServiceProvider { get; private set; } =
            ServiceCollection.BuildServiceProvider();

        public static void ConfigureDi(Action<IServiceCollection> configure)
        {
            configure(ServiceCollection);
            ServiceProvider = ServiceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(sp => new LoggingChannel("VendingMachineKiosk", null));
            services.AddSingleton<ServerRequester>();
            services.AddSingleton<VendingStateViewModelService>();
            services.AddSingleton<IVendingMachineControlService, VendingMachineControlService>();
            services.AddSingleton<PreloadSingletonServiceHelper>();
        }
    }
}

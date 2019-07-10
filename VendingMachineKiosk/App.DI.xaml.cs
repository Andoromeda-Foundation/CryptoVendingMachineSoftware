using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

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
    }
}

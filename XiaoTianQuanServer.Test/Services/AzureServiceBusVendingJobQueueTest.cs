using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using XiaoTianQuanServer.Data;
using XiaoTianQuanServer.DataModels;
using XiaoTianQuanServer.Services;
using XiaoTianQuanServer.Services.Implementations;
using Xunit;

namespace XiaoTianQuanServer.Test.Services
{
    //public class AzureServiceBusVendingJobQueueTest : DbContextTest
    //{
        //private readonly AzureServiceBusVendingJobQueue _queue;
        //private readonly IKvCacheManager cacheManager = Substitute.For<IKvCacheManager>();
        //private Guid M1Guid = Guid.NewGuid();

        //public AzureServiceBusVendingJobQueueTest()
        //{
        //    var serviceProvider = Substitute.For<IServiceProvider>();
        //    serviceProvider.GetService<ApplicationDbContext>().Returns(DbContext);

        //    _queue = new AzureServiceBusVendingJobQueue(new Logger<AzureServiceBusVendingJobQueue>(), serviceProvider,
        //        new Option<Settings.ServiceBus>(new Settings.ServiceBus
        //        {
        //            ConnectionString =
        //                "Endpoint=sb://xiaotianquan-dev.servicebus.windows.net/;SharedAccessKeyName=ServerKey;SharedAccessKey=G5TqH1Znl0x+jA00ShhmZQd/0tQbA+oVeCvYBvPMcKI=",
        //            PaymentExpiryQueueName = "PaymentExpiry",
        //            ProductUnreleasedRefundQueueName = "ProductUnreleasedRefund",
        //            VendingMachineUnlockQueueName = "VendingMachineUnlock"
        //        }), cacheManager);
        //}

        //public override async Task InitializeAsync()
        //{
        //    await base.InitializeAsync();
        //    await _queue.StartAsync(new System.Threading.CancellationToken());
        //}

        //protected override void CreateDatabaseItems(ApplicationDbContext context)
        //{
        //    var m1 = new VendingMachine
        //    {
        //        ExclusiveUseLock = Guid.NewGuid(),
        //        MachineId = M1Guid
        //    };
        //    context.Add(m1);
        //}

        //[Fact]
        //public async Task AzureServiceBusVendingJobQueue_ShouldEnqueueVendingMachineUnlockAndExecute()
        //{
        //    await _queue.EnqueueVendingMachineUnlockMessageAsync(M1Guid, 0);
        //    await Task.Delay(2);
        //    var m = await DbContext.VendingMachines.SingleAsync();
        //    Assert.Equal(m.ExclusiveUseLock, Guid.Empty);
        //}
    //}
}

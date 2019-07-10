using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XiaoTianQuanServer.Data;
using XiaoTianQuanServer.Settings;

namespace XiaoTianQuanServer.Services.Implementations
{
    public class AzureServiceBusVendingJobQueue : IVendingJobQueue, IHostedService
    {
        private readonly ILogger<AzureServiceBusVendingJobQueue> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IKvCacheManager _cacheManager;
        private readonly IQueueClient _paymentExpiryQueue;
        private readonly IQueueClient _vendingMachineUnlockQueue;
        private readonly IQueueClient _productUnreleasedRefundQueue;
        private readonly ServiceBus _settings;

        private (IServiceScope, ApplicationDbContext) GetDbContext()
        {
            var scope = _serviceProvider.CreateScope();
            return (scope, scope.ServiceProvider.GetService<ApplicationDbContext>());
        }

        public AzureServiceBusVendingJobQueue(ILogger<AzureServiceBusVendingJobQueue> logger,
            IServiceProvider serviceProvider, IOptions<ServiceBus> options, IKvCacheManager cacheManager)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cacheManager = cacheManager;
            _settings = options.Value;
            _vendingMachineUnlockQueue = new QueueClient(_settings.ConnectionString, _settings.VendingMachineUnlockQueueName);
            _productUnreleasedRefundQueue = new QueueClient(_settings.ConnectionString, _settings.ProductUnreleasedRefundQueueName);
            _paymentExpiryQueue = new QueueClient(_settings.ConnectionString, _settings.PaymentExpiryQueueName);
        }

        public async Task EnqueueVendingMachineUnlockMessageAsync(Guid machineId, int delay)
        {
            var message = new Message
            {
                Body = Encoding.UTF8.GetBytes(machineId.ToString())
            };

            var seqNo = await _vendingMachineUnlockQueue.ScheduleMessageAsync(message, DateTime.UtcNow.AddSeconds(delay));
            _logger.LogTrace($"Enqueued vending machine unlock message for machine {machineId}, seq no. {seqNo}");
            await _cacheManager.SetAsync(_settings.VendingMachineUnlockQueueName, machineId.ToString(), seqNo);
        }

        public async Task<bool> RemoveVendingMachineUnlockMessageAsync(Guid machineId)
        {
            var seqNo = await _cacheManager.GetDeleteLongAsync(_settings.VendingMachineUnlockQueueName, machineId.ToString());
            try
            {
                await _vendingMachineUnlockQueue.CancelScheduledMessageAsync(seqNo);
                _logger.LogTrace($"Cancelled vending machine unlock message for machine {machineId}, seq no. {seqNo}");
                return true;
            }
            catch (MessageNotFoundException)
            {
                _logger.LogError(
                    $"Failed to remove machine unlock message for machine {machineId}, retrieved seq no. {seqNo} not found");
                return false;
            }
        }

        public async Task EnqueuePaymentExpiryMessageAsync(Guid transactionId, int delay)
        {
            var message = new Message
            {
                Body = Encoding.UTF8.GetBytes(transactionId.ToString())
            };

            var seqNo = await _paymentExpiryQueue.ScheduleMessageAsync(message, DateTime.UtcNow.AddSeconds(delay));
            await _cacheManager.SetAsync(_settings.PaymentExpiryQueueName, transactionId.ToString(), seqNo);
        }

        public async Task<bool> RemovePaymentExpiryMessageAsync(Guid transactionId)
        {
            var seqNo = await _cacheManager.GetDeleteLongAsync(_settings.PaymentExpiryQueueName,
                transactionId.ToString());
            try
            {
                await _paymentExpiryQueue.CancelScheduledMessageAsync(seqNo);
                return true;
            }
            catch (MessageNotFoundException)
            {
                _logger.LogError(
                    $"Failed to remove payment expiry message for transaction {transactionId}, retrieved seq no {seqNo} not found");
                return false;
            }
        }

        public Task EnqueueProductUnreleasedRefundMessageAsync(Guid transactionId, int delay)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveProductUnreleasedRefundMessageAsync(Guid transactionId)
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var options = new MessageHandlerOptions(ExceptionHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            };

            _vendingMachineUnlockQueue.RegisterMessageHandler(HandleVendingMachineUnlockQueueMessage, options);
            _paymentExpiryQueue.RegisterMessageHandler(HandlePaymentExpiryQueueMessage, options);
            _productUnreleasedRefundQueue.RegisterMessageHandler(HandleProductUnreleasedRefundQueueMessage, options);

            return Task.CompletedTask;
        }

        private Task HandleProductUnreleasedRefundQueueMessage(Message message, CancellationToken arg2)
        {
            throw new NotImplementedException();
        }

        private Task HandlePaymentExpiryQueueMessage(Message arg1, CancellationToken arg2)
        {
            // It seems that you don't need to do anything
            // TODO: it can actually revoke the invoice to avoid miss payment
            return Task.CompletedTask;
        }

        private async Task HandleVendingMachineUnlockQueueMessage(Message message, CancellationToken dummy)
        {
            bool ok = Guid.TryParse(Encoding.UTF8.GetString(message.Body), out var machineId);
            if (!ok)
            {
                _logger.LogError(
                    $"VendingMachineUnlockQueue received trash data: {BitConverter.ToString(message.Body.Take(32).ToArray())}");
                await _vendingMachineUnlockQueue.DeadLetterAsync(message.SystemProperties.LockToken);
                return;
            }

            (IServiceScope scope, ApplicationDbContext context) = GetDbContext();
            using IServiceScope serviceScope = scope;

            var machine = await context.VendingMachines.SingleAsync(vm => vm.MachineId == machineId);

            if (machine.ExclusiveUseLock == Guid.Empty)
            {
                _logger.LogError(
                    $"VendingMachineUnlockQueue failed to unlock: machine {machineId} is already unlocked");
                await _vendingMachineUnlockQueue.CompleteAsync(message.SystemProperties.LockToken);
                return;
            }

            machine.ExclusiveUseLock = Guid.Empty;
            context.Entry(machine).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
                await _vendingMachineUnlockQueue.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _vendingMachineUnlockQueue.AbandonAsync(message.SystemProperties.LockToken);
            }
        }

        private Task ExceptionHandler(ExceptionReceivedEventArgs arg)
        {
            _logger.LogError(arg.Exception,
                $"Exception received during process queue {arg.ExceptionReceivedContext.EntityPath}@{arg.ExceptionReceivedContext.Endpoint}");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();    // This is not happening
        }
    }
}

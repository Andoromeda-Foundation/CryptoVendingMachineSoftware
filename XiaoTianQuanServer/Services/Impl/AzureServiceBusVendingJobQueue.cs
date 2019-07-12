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
using Newtonsoft.Json;
using XiaoTianQuanServer.Data;
using XiaoTianQuanServer.Settings;

namespace XiaoTianQuanServer.Services.Impl
{
    public class AzureServiceBusVendingJobQueue : IVendingJobQueue, IHostedService
    {
        private readonly ILogger<AzureServiceBusVendingJobQueue> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IKvCacheManager _cacheManager;
        private readonly IRefundService _refundService;
        private readonly IQueueClient _paymentExpiryQueue;
        private readonly IQueueClient _productUnfulfilledRefundQueue;
        private readonly ServiceBus _settings;

        public AzureServiceBusVendingJobQueue(ILogger<AzureServiceBusVendingJobQueue> logger,
            IServiceProvider serviceProvider, IOptions<ServiceBus> options,
            IKvCacheManager cacheManager, IRefundService refundService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cacheManager = cacheManager;
            _refundService = refundService;
            _settings = options.Value;
            _productUnfulfilledRefundQueue = new QueueClient(_settings.ConnectionString, _settings.ProductUnfulfilledRefundQueueName);
            _paymentExpiryQueue = new QueueClient(_settings.ConnectionString, _settings.PaymentExpiryQueueName);
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

            if (seqNo.HasValue)
            {
                try
                {
                    await _paymentExpiryQueue.CancelScheduledMessageAsync(seqNo.Value);
                    return true;
                }
                catch (MessageNotFoundException)
                {
                    _logger.LogError(
                        $"Failed to remove payment expiry message for transaction {transactionId}, retrieved seq no {seqNo} not found");
                    return false;
                }
            }
            else
            {
                _logger.LogInformation($"Payment expiry message seq No. was not found for transaction {transactionId}");
                return false;
            }
        }

        public async Task EnqueueProductUnfulfilledRefundMessageAsync(Guid transactionId, int inventoryId, int timeoutRelease)
        {
            var message = new ProductUnfulfilledRefundMessage
            {
                InventoryId = inventoryId,
                TransactionId = transactionId,
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            var seqNo = await _productUnfulfilledRefundQueue.ScheduleMessageAsync(msg, DateTime.UtcNow.AddSeconds(timeoutRelease));
            await _cacheManager.SetAsync(_settings.ProductUnfulfilledRefundQueueName, transactionId.ToString(), seqNo);
        }

        public class ProductUnfulfilledRefundMessage
        {
            public Guid TransactionId { get; set; }
            public int InventoryId { get; set; }
        }


        public async Task<bool> RemoveProductUnfulfilledRefundMessageAsync(Guid transactionId)
        {
            var seqNo = await _cacheManager.GetLongAsync(_settings.ProductUnfulfilledRefundQueueName,
                transactionId.ToString());

            if (seqNo.HasValue)
            {
                try
                {
                    await _productUnfulfilledRefundQueue.CancelScheduledMessageAsync(seqNo.Value);
                    return true;
                }
                catch (MessageNotFoundException)
                {
                    _logger.LogError(
                        $"Failed to remove product unfulfilled message for transaction {transactionId}, retrieved seq no {seqNo} not found");
                    return false;
                }
            }
            else
            {
                _logger.LogInformation($"Unfulfilled message seq No. was not found for transaction {transactionId}");
                return false;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var options = new MessageHandlerOptions(ExceptionHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            };

            _paymentExpiryQueue.RegisterMessageHandler(HandlePaymentExpiryQueueMessage, options);
            _productUnfulfilledRefundQueue.RegisterMessageHandler(HandleProductUnfulfilledRefundQueueMessage, options);

            return Task.CompletedTask;
        }

        private async Task HandleProductUnfulfilledRefundQueueMessage(Message message, CancellationToken _)
        {
            using var scope = _serviceProvider.CreateScope();

            var jsonString = Encoding.UTF8.GetString(message.Body);

            bool processed = false;
            try
            {
                var msg = JsonConvert.DeserializeObject<ProductUnfulfilledRefundMessage>(jsonString);

                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var vendingMachineDataService = scope.ServiceProvider.GetService<IVendingMachineDataService>();

                var transaction = await context.Transactions.FindAsync(msg.TransactionId);
                if (transaction == null)
                {
                    _logger.LogError($"unknown transaction {msg.TransactionId} in unfulfilled refund queue");
                    return;
                }

                if (transaction.Fulfilled)
                {
                    _logger.LogError(
                        $"transaction {msg.TransactionId} in unfulfilled refund queue was actually fulfilled");
                    processed = true;
                    return;
                }

                var invOk = await vendingMachineDataService.IncreaseVendingMachineSlotInventoryQuantityAsync(
                    msg.InventoryId,
                    1);
                if (!invOk)
                {
                    _logger.LogError($"failed to increase inventory for unfulfilled item {msg.InventoryId}");
                    return;
                }

                await _refundService.AppendToRefundQueueAsync(msg.TransactionId);

                _logger.LogInformation(
                    $"inventory item {msg.InventoryId} was returned to stock due to unfulfillment");
                processed = true;
            }
            catch (JsonReaderException)
            {
                _logger.LogError($"product unfulfilled queue got unknown message {jsonString}");
            }
            finally
            {
                if (processed)
                {
                    await _productUnfulfilledRefundQueue.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    await _productUnfulfilledRefundQueue.AbandonAsync(message.SystemProperties.LockToken);
                }
            }

        }

        private Task HandlePaymentExpiryQueueMessage(Message arg1, CancellationToken _)
        {
            // It seems that you don't need to do anything
            // TODO: it can actually revoke the invoice to avoid miss payment
            return Task.CompletedTask;
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

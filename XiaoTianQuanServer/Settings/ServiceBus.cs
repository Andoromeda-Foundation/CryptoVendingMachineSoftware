namespace XiaoTianQuanServer.Settings
{
    public class ServiceBus
    {
        public string ConnectionString { get; set; }
        public string PaymentExpiryQueueName { get; set; }
        public string ProductUnfulfilledRefundQueueName { get; set; }
        public string PendingLightningNetworkInvoiceQueue { get; set; }
    }
}
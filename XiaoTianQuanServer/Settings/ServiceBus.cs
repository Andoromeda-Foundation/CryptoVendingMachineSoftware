namespace XiaoTianQuanServer.Settings
{
    public class ServiceBus
    {
        public string ConnectionString { get; set; }
        public string VendingMachineUnlockQueueName { get; set; }
        public string PaymentExpiryQueueName { get; set; }
        public string ProductUnreleasedRefundQueueName { get; set; }
    }
}
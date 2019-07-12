using System;

namespace XiaoTianQuanProtocols.VendingMachineRequests
{
    public class TransactionCompleteRequest
    {
        public Guid TransactionId { get; set; }
    }

    public class TransactionCompleteResponse : ResponseBase
    {

    }
}
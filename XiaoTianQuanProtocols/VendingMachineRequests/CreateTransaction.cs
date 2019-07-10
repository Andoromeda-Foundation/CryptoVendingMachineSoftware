using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace XiaoTianQuanProtocols.VendingMachineRequests
{
    public class CreateTransactionRequest
    {
        [Required]
        public string Slot { get; set; }
        public PaymentType PaymentType { get; set; }
        public Guid LockToken { get; set; }
    }

    public class CreateTransactionResponse : ResponseBase
    {
        [Required]
        public string PaymentCode { get; set; }
        public int PaymentDisplayTimeout { get; set; }
        public Guid TransactionId { get; set; }
    }
}

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
    }

    public class CreateTransactionResponse : ResponseBase
    {
        public DateTime PaymentDisplayTimeout { get; set; }
        public Guid TransactionId { get; set; }
    }
}

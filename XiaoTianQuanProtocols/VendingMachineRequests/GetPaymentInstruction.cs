using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace XiaoTianQuanProtocols.VendingMachineRequests
{
    public class GetPaymentInstructionRequest
    {
        public Guid TransactionId { get; set; }
        public PaymentType PaymentType { get; set; }
    }

    public class GetPaymentInstructionResponse : ResponseBase
    {
        [Required]
        public string PaymentCode { get; set; }
    }
}

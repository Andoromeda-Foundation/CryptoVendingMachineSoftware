using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XiaoTianQuanServer.Services.LightningNetwork.Models
{
    public class AddInvoiceRequest
    {
        public string Memo { get; set; }
        public string RPreimage { get; set; }
        public string RHash { get; set; }
        public string Value { get; set; }
        public bool Settled { get; set; }
        public string CreationDate { get; set; }
        public string SettleDate { get; set; }
        public string PaymentRequest { get; set; }
        public string DescriptionHash { get; set; }
        public string Expiry { get; set; }
        public string FallbackAddr { get; set; }
        public string CltvExpiry { get; set; }
        public List<RouteHint> RouteHints { get; set; }
        public bool Private { get; set; }
        public string AddIndex { get; set; }
        public string SettleIndex { get; set; }
        public string AmtPaidSat { get; set; }
        public string AmtPaidMsat { get; set; }
        public InvoiceInvoiceState State { get; set; }
    }

    public class AddInvoiceResponse
    {
        public string RHash { get; set; }
        public string PaymentRequest { get; set; }
        public string AddIndex { get; set; }
    }
}

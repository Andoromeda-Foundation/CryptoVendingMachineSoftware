using System;
using System.Collections.Generic;
using System.Text;

namespace XiaoTianQuanProtocols.VendingMachineRequests
{
    public class ResponseBase
    {
        public ResponseStatus Status { get; set; }

    }

    public enum ResponseStatus
    {
        Ok = 0,

        // Vending Machine related
        InvalidLockToken = 1000,
        VendingMachineNotFound = 1001,

        // Inventory related
        OutOfOrder = 2000,

        // Internal errors
        // 50xx: local server
        // 51xx: lnd
        LndGenerateInvoiceFailed = 5100,
    }
}

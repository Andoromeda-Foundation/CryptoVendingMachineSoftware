﻿namespace XiaoTianQuanProtocols.VendingMachineRequests
{
    public static class Endpoints
    {
        public const string GetProductList = "api/vendingmachine/products";
        public const string CreateTransaction = "api/transaction/create";
        public const string GetPaymentInstruction = "api/transaction/getpaymentinstruction";
        public const string VendingMachineHub = "hub/vendingmachine";
        public const string TransactionComplete = "api/transaction/transactioncomplete";
    }
}

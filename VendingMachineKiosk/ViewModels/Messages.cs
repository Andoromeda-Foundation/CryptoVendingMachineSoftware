using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineKiosk.ViewModels
{
    public enum Messages
    {
        InvalidatePaymentSession,
        LoadProductPaymentViewModel,
        UnloadProductPaymentViewModel,
        LoadPaymentInstructionPage,
        LoadPaymentInstructionViewModel,
        UnloadPaymentInstructionViewModel,
        LoadProductSelectionViewModel,
        UnloadProductSelectionViewModel,
        ProductReleasing,
        ProductReleased
    }
}

using GalaSoft.MvvmLight.Views;

namespace VendingMachineKiosk.Services
{
    public interface IConcealableNavigationService : INavigationService
    {
        void NavigateToWithoutHistory(string pageKey);
        
        void NavigateToWithoutHistory(string pageKey, object parameter);
    }
}
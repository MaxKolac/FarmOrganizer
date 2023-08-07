using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Views;

namespace FarmOrganizer.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        [RelayCommand]
        static async Task OpenPage(string s) => 
            await Shell.Current.GoToAsync(s);

        [RelayCommand]
        async Task OpenDatabasePage() => 
            await Shell.Current.GoToAsync(nameof(LedgerPage));
    }
}

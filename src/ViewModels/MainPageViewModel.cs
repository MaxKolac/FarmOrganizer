using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FarmOrganizer.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        [RelayCommand]
        static async Task OpenPage(string s) =>
            await Shell.Current.GoToAsync(s);
    }
}

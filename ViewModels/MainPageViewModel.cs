using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FarmOrganizer.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public string someText;

        [RelayCommand]
        void ChangeText()
        {
            SomeText = String.Empty;
        }

        [RelayCommand]
        static async Task OpenPage(string s) => await Shell.Current.GoToAsync(s);
    }
}

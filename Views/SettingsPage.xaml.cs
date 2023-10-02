using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class DebugPage : ContentPage
{
    public DebugPage(DebugPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class LedgerFilterPage : ContentPage
{
    public LedgerFilterPage(LedgerFilterPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
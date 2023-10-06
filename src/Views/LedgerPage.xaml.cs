using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class LedgerPage : ContentPage
{
    public LedgerPage(LedgerPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override bool OnBackButtonPressed()
    {
        //And here I am, foolishly thinking I'd never have to have any code on page's modal class...
        base.OnBackButtonPressed();
        _ = ((LedgerPageViewModel)BindingContext).ReturnToPreviousPage();
        return true;
    }
}
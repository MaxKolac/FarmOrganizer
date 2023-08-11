using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class LedgerPage : ContentPage
{
	public LedgerPage(LedgerPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
		base.OnNavigatedTo(args);
		//View should NOT be "aware" of viewmodel, but this is kinda necessary
		((LedgerPageViewModel)BindingContext).QueryLedgerEntries();
    }
}
using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class LedgerPage : ContentPage
{
	public LedgerPage(LedgerPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
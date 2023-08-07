using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class LedgerRecordPage : ContentPage
{
	public LedgerRecordPage(LedgerRecordPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
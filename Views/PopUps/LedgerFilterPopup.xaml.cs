using FarmOrganizer.ViewModels;
using Mopups.Pages;

namespace FarmOrganizer.PopUps;

public partial class LedgerFilterPopup : PopupPage
{
	public LedgerFilterPopup(LedgerFilterPopupViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
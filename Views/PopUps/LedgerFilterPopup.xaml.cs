using FarmOrganizer.ViewModels.PopUps;
using Mopups.Pages;

namespace FarmOrganizer.Views.PopUps;

public partial class LedgerFilterPopup : PopupPage
{
	public LedgerFilterPopup(LedgerFilterPopupViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
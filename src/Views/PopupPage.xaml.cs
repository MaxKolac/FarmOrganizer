using CommunityToolkit.Maui.Views;
using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class PopupPage : Popup
{
	public PopupPage(PopupPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}
}
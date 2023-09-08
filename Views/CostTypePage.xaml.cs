using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class CostTypePage : ContentPage
{
	public CostTypePage(CostTypePageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
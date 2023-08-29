using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class SeasonsPage : ContentPage
{
	public SeasonsPage(SeasonsPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
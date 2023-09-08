using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class CropFieldPage : ContentPage
{
	public CropFieldPage(CropFieldPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class DatabasePage : ContentPage
{
	public DatabasePage(DatabasePageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
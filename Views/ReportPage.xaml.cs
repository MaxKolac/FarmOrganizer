using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class ReportPage : ContentPage
{
	public ReportPage(ReportPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
using FarmOrganizer.Views;

namespace FarmOrganizer;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		//Route registries for different pages
		Routing.RegisterRoute(nameof(QuickCalculator), typeof(QuickCalculator));
		Routing.RegisterRoute(nameof(DatabasePage), typeof(DatabasePage));
	}
}

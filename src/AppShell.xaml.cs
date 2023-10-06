using FarmOrganizer.Views;

namespace FarmOrganizer;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        //Route registries for different pages
        Routing.RegisterRoute(nameof(QuickCalculator), typeof(QuickCalculator));
        Routing.RegisterRoute(nameof(LedgerPage), typeof(LedgerPage));
        Routing.RegisterRoute(nameof(LedgerFilterPage), typeof(LedgerFilterPage));
        Routing.RegisterRoute(nameof(LedgerRecordPage), typeof(LedgerRecordPage));
        Routing.RegisterRoute(nameof(ReportPage), typeof(ReportPage));
        Routing.RegisterRoute(nameof(DebugPage), typeof(DebugPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(SeasonsPage), typeof(SeasonsPage));
        Routing.RegisterRoute(nameof(CostTypePage), typeof(CostTypePage));
        Routing.RegisterRoute(nameof(CropFieldPage), typeof(CropFieldPage));
    }
}

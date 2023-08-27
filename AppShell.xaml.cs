﻿using FarmOrganizer.Views;

namespace FarmOrganizer;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		//Route registries for different pages
		Routing.RegisterRoute(nameof(QuickCalculator), typeof(QuickCalculator));
		Routing.RegisterRoute(nameof(LedgerPage), typeof(LedgerPage));
		Routing.RegisterRoute(nameof(LedgerRecordPage), typeof(LedgerRecordPage));
		Routing.RegisterRoute(nameof(ReportPage), typeof(ReportPage));
		Routing.RegisterRoute(nameof(DebugPage), typeof(DebugPage));
		Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
	}
}

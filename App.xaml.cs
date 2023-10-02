using FarmOrganizer.Services;
using FarmOrganizer.ViewModels;

namespace FarmOrganizer;

public partial class App : Application
{
    private static IServiceProvider services;
    private static IAlertService alertSvc;

    public static IServiceProvider Services { get => services; set => services = value; }
    public static IAlertService AlertSvc { get => alertSvc; set => alertSvc = value; }

    public App(IServiceProvider provider)
    {
        InitializeComponent();

        Services = provider;
        AlertSvc = Services.GetService<IAlertService>();
        SettingsPageViewModel.ApplyPreferences();

        MainPage = new AppShell();
    }
}

using FarmOrganizer.IO.Exporting.PDF;
using FarmOrganizer.Services;
using FarmOrganizer.ViewModels;
using PdfSharpCore.Fonts;
using System.Globalization;

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
        CultureInfo.CurrentCulture = new CultureInfo("pl-PL", false);

        Services = provider;
        AlertSvc = Services.GetService<IAlertService>();
        SettingsPageViewModel.ApplyPreferences();

        //Required by PdfSharpCore and MigraDocCore in PdfBuilder class
        GlobalFontSettings.FontResolver = new GenericFontResolver();

        MainPage = new AppShell();
    }
}

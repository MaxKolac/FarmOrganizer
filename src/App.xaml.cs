using FarmOrganizer.IO.Exporting.PDF;
using FarmOrganizer.ViewModels;
using PdfSharpCore.Fonts;
using System.Globalization;

namespace FarmOrganizer;

public partial class App : Application
{
    public static IServiceProvider Services { get; set; }

    public App(IServiceProvider provider)
    {
        InitializeComponent();
        CultureInfo.CurrentCulture = new CultureInfo("pl-PL", false);

        Services = provider;
        SettingsPageViewModel.ApplyPreferences();

        GlobalFontSettings.FontResolver = new GenericFontResolver();

        MainPage = new AppShell();
    }
}

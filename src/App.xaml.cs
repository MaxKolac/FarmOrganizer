using CommunityToolkit.Maui.Core;
using FarmOrganizer.IO.Exporting.PDF;
using FarmOrganizer.ViewModels;
using PdfSharpCore.Fonts;
using System.Globalization;

namespace FarmOrganizer;

public partial class App : Application
{
    /// <summary>
    /// Static property exposing the collection of all services registered in the app.
    /// </summary>
    public static IServiceProvider Services { get; set; }

    /// <summary>
    /// An <see cref="IPopupService"/> property for classes which aren't ViewModels or can't benefit from dependency injection.
    /// </summary>
    public static IPopupService PopupService { get; set; }

    public App(IServiceProvider provider)
    {
        InitializeComponent();
        CultureInfo.CurrentCulture = new CultureInfo("pl-PL", false);

        Services = provider;
        PopupService = provider.GetService<IPopupService>();

        SettingsPageViewModel.ApplyPreferences();
        GlobalFontSettings.FontResolver = new GenericFontResolver();
        MainPage = new AppShell();
    }
}

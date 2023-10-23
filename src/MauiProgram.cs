using CommunityToolkit.Maui;
using FarmOrganizer.Database;
using FarmOrganizer.IO;
using FarmOrganizer.Services;
using FarmOrganizer.ViewModels;
using FarmOrganizer.Views;
using Microsoft.Extensions.Logging;

namespace FarmOrganizer;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-BoldItalic.ttf", "OpenSansBoldItalic");
                fonts.AddFont("OpenSans-Italic.ttf", "OpenSansItalic");
                fonts.AddFont("OpenSans-Bold.ttf", "OpenSansBold");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        //Register singletons of view and VM types to dependency inject into XAML code-behinds and VM constructors
        //Singletons are created only once and remain through app's lifetime
        //Transients are created and disposed repeatedly
        //Use CommunityMauiToolkit extensions to add Pages automatically with ViewModels and automatically register a new shell route for them
        builder.Services.AddSingletonWithShellRoute<MainPage, MainPageViewModel>(nameof(MainPage));
        builder.Services.AddTransientWithShellRoute<QuickCalculator, QuickCalculatorViewModel>(nameof(QuickCalculator));
        builder.Services.AddTransientWithShellRoute<LedgerPage, LedgerPageViewModel>(nameof(LedgerPage));
        builder.Services.AddTransientWithShellRoute<LedgerRecordPage, LedgerRecordPageViewModel>(nameof(LedgerRecordPage));
        builder.Services.AddTransientWithShellRoute<LedgerFilterPage, LedgerFilterPageViewModel>(nameof(LedgerFilterPage));
        builder.Services.AddTransientWithShellRoute<ReportPage, ReportPageViewModel>(nameof(ReportPage));
        builder.Services.AddTransientWithShellRoute<SettingsPage, SettingsPageViewModel>(nameof(SettingsPage));
        builder.Services.AddTransientWithShellRoute<SeasonsPage, SeasonsPageViewModel>(nameof(SeasonsPage));
        builder.Services.AddTransientWithShellRoute<CostTypePage, CostTypePageViewModel>(nameof(CostTypePage));
        builder.Services.AddTransientWithShellRoute<CropFieldPage, CropFieldPageViewModel>(nameof(CropFieldPage));
        builder.Services.AddDbContext<DatabaseContext>();
        builder.Services.AddSingleton<IAlertService, AlertService>();

#if DEBUG
        builder.Services.AddTransientWithShellRoute<DebugPage, DebugPageViewModel>(nameof(DebugPage));
#endif

        if (!DatabaseFile.Exists())
            MainThread.InvokeOnMainThreadAsync(DatabaseFile.Create);

        return builder.Build();
    }
}
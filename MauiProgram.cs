using FarmOrganizer.Database;
using FarmOrganizer.Services;
using FarmOrganizer.ViewModels;
using FarmOrganizer.Views;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

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
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        #if DEBUG
        builder.Logging.AddDebug();
        #endif

        //Register singletons of view and VM types to dependency inject into XAML code-behinds
        //Singletons are created only once and remain through app's lifetime
        //Transients are created and disposed repeatedly
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddTransient<QuickCalculator>();
        builder.Services.AddTransient<QuickCalculatorViewModel>();
        builder.Services.AddTransient<LedgerPage>();
        builder.Services.AddTransient<LedgerPageViewModel>();
        builder.Services.AddTransient<LedgerRecordPage>();
        builder.Services.AddTransient<LedgerRecordPageViewModel>();
        builder.Services.AddTransient<ReportPage>();
        builder.Services.AddTransient<ReportPageViewModel>();
        builder.Services.AddTransient<DebugPage>();
        builder.Services.AddTransient<DebugPageViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SettingsPageViewModel>();
        builder.Services.AddTransient<SeasonsPage>();
        builder.Services.AddTransient<SeasonsPageViewModel>();
        builder.Services.AddTransient<CostTypePage>();
        builder.Services.AddTransient<CostTypePageViewModel>();
        builder.Services.AddDbContext<DatabaseContext>();
        builder.Services.AddSingleton<IAlertService, AlertService>();

        if (!DatabaseFile.Exists())
            MainThread.InvokeOnMainThreadAsync(DatabaseFile.Create);

        return builder.Build();
    }
}
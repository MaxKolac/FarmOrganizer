using FarmOrganizer.Models;
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
        builder.Services.AddTransient<DebugPage>();
        builder.Services.AddTransient<DebugPageViewModel>();
        builder.Services.AddDbContext<DatabaseContext>();
        builder.Services.AddSingleton<IAlertService, AlertService>();
        CopyDatabaseToAppDirectory("database.sqlite3");

        return builder.Build();
    }

    /// <summary>
    /// Copy the read-only database file bundled with the APK to app's local storage to be editable.
    /// <para>
    /// <see href="https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers?tabs=android"/>
    /// </para>
    /// </summary>
    /// <param name="databaseFilename">The name of the database file.</param>
    public static async Task CopyDatabaseToAppDirectory(string databaseFilename)
    {
        using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(databaseFilename);
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, databaseFilename);

        using FileStream outputStream = File.Create(targetFile);
            await inputStream.CopyToAsync(outputStream);
    }
}

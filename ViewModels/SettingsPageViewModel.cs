using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.ViewModels.Converters;

namespace FarmOrganizer.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        private const string appThemeKey = "appTheme";

        [ObservableProperty]
        private List<string> appThemes;
        [ObservableProperty]
        private AppTheme selectedTheme = Enum.Parse<AppTheme>(Preferences.Get(appThemeKey, Enum.GetName(AppTheme.Unspecified)));

        public SettingsPageViewModel()
        {
            AppThemes = new()
            {
                AppThemeToStringConverter.Default,
                AppThemeToStringConverter.Light,
                AppThemeToStringConverter.Dark
            };
        }

        public static void ApplyPreferences()
        {
            Application.Current.UserAppTheme = Enum.Parse<AppTheme>(Preferences.Get(appThemeKey, Enum.GetName(AppTheme.Unspecified)));
        }

        partial void OnSelectedThemeChanging(AppTheme oldValue, AppTheme newValue)
        {
            Preferences.Set("appTheme", Enum.GetName(newValue));
            ApplyPreferences();
        }

        [RelayCommand]
        private async static Task ResetDatabase()
        {
            if (await App.AlertSvc.ShowConfirmationAsync(
                "Uwaga!",
                "Za chwilę bezpowrotnie wyczyścisz wszystkie dane z bazy danych. " +
                "Tej akcji nie można odwrócić. Czy jesteś pewny aby kontynuować?", 
                "Tak", "Nie"))
            {
                await DatabaseFile.Delete();
                await MainThread.InvokeOnMainThreadAsync(DatabaseFile.Create);
            }
        }

        [RelayCommand]
        private async static Task ExportDatabase()
        {
            try
            {
                FolderPickerResult folder = await FolderPicker.PickAsync(default);
                if (!folder.IsSuccessful)
                    return;
                await DatabaseFile.ExportTo(folder.Folder.Path);
                App.AlertSvc.ShowAlert(
                    "Sukces",
                    $"Baza danych została pomyślnie wyeksportowana do lokalizacji: {folder.Folder.Path}"
                    );
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        [RelayCommand]
        private async static Task ImportDatabase()
        {
            try
            {
                await DatabaseFile.CreateBackup();
                FileResult file = await FilePicker.PickAsync();
                if (file == null)
                {
                    DatabaseFile.DeleteBackup();
                    return;
                }
                await DatabaseFile.ImportFrom(file.FullPath);
                App.AlertSvc.ShowAlert(
                    "Sukces",
                    $"Baza danych została pomyślnie importowana z pliku {file.FileName}"
                    );
                DatabaseFile.DeleteBackup();
            }
            catch (Exception ex)
            {
                await DatabaseFile.RestoreBackup();
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }
    }
}

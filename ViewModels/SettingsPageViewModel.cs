using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.Converters;
using Microsoft.Data.Sqlite;

namespace FarmOrganizer.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        public const string AppThemeKey = "appTheme";
        public const string LedgerPage_DefaultCropField = "ledger_defaultCropFieldKey";

        [ObservableProperty]
        private List<string> appThemes;
        [ObservableProperty]
        private AppTheme selectedTheme;

        [ObservableProperty]
        private List<CropField> cropFields;
        [ObservableProperty]
        private CropField defaultCropField;

        public SettingsPageViewModel()
        {
            using var context = new DatabaseContext();
            AppThemes = new()
                {
                    AppThemeToStringConverter.Default,
                    AppThemeToStringConverter.Light,
                    AppThemeToStringConverter.Dark
                };
            SelectedTheme = Enum.Parse<AppTheme>(
                Preferences.Get(
                    AppThemeKey,
                    Enum.GetName(AppTheme.Unspecified)
                    )
                );

            CropFields = context.CropFields.ToList();
            DefaultCropField = context.CropFields.Find(
                Preferences.Get(
                    LedgerPage_DefaultCropField,
                    context.CropFields.First().Id
                    )
                );
            DefaultCropField ??= CropFields.First();
        }

        public static void ApplyPreferences()
        {
            Application.Current.UserAppTheme = Enum.Parse<AppTheme>(Preferences.Get(AppThemeKey, Enum.GetName(AppTheme.Unspecified)));
        }

        partial void OnSelectedThemeChanging(AppTheme oldValue, AppTheme newValue)
        {
            Preferences.Set(AppThemeKey, Enum.GetName(newValue));
            ApplyPreferences();
        }

        partial void OnDefaultCropFieldChanged(CropField oldValue, CropField newValue)
        {
            Preferences.Set(LedgerPage_DefaultCropField, newValue.Id);
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
            catch (IOException ex)
            {
                ExceptionHandler.Handle(ex, false);
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
            catch (IOException ex)
            {
                await DatabaseFile.RestoreBackup();
                ExceptionHandler.Handle(ex, false);
            }
        }
    }
}

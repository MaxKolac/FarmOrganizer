using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.Converters;

namespace FarmOrganizer.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        #region Preference Keys
        public const string AppThemeKey = "appTheme";
        public const string LedgerPage_DefaultCropField = "ledger_defaultCropFieldKey";
        #endregion

        [ObservableProperty]
        private List<string> appThemes;
        [ObservableProperty]
        private AppTheme selectedTheme;

        [ObservableProperty]
        private List<CropField> cropFields = new();
        [ObservableProperty]
        private CropField defaultCropField;
        [ObservableProperty]
        private bool cropFieldPickerEnabled = true;

        private const string _IOAlertNoPermissions = "Aby aplikacja mogła zresetować bazę danych, potrzebne są odpowiednie uprawnienia.\nJeżeli ten komunikat pokazuje się po zrestartowaniu aplikacji, możliwe że wymagane jest zresetowanie odmówionych uprawnień. Przejdź do ustawień swojego telefonu, a następnie w sekcji 'Aplikacje', odnajdź FarmOrganizer i nadaj mu uprawnienia do zapisu i odczytu plików.";

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

            try
            {
                CropFields = CropField.RetrieveAll(null);
                DefaultCropField = CropFields.FirstOrDefault(field => field.Id == Preferences.Get(LedgerPage_DefaultCropField, CropFields.First().Id));
                DefaultCropField ??= CropFields.First();
            }
            catch (TableValidationException ex)
            {
                CropFieldPickerEnabled = false;
                ExceptionHandler.Handle(ex, false);
            }
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
            if (CropFieldPickerEnabled)
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
                if (!await DatabaseFile.RequestPermissions())
                {
                    App.AlertSvc.ShowAlert("Błąd", _IOAlertNoPermissions);
                    return;
                }
                await DatabaseFile.Delete();
                await DatabaseFile.Create();
            }
        }

        [RelayCommand]
        private async static Task ExportDatabase()
        {
            try
            {
                if (!await DatabaseFile.RequestPermissions())
                {
                    App.AlertSvc.ShowAlert("Błąd", _IOAlertNoPermissions);
                    return;
                }
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
                if (!await DatabaseFile.RequestPermissions())
                {
                    App.AlertSvc.ShowAlert("Błąd", _IOAlertNoPermissions);
                    return;
                }
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

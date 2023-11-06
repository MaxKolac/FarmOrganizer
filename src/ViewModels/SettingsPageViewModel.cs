using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.IO;
using FarmOrganizer.Models;
using FarmOrganizer.Services;
using FarmOrganizer.ViewModels.Converters;

namespace FarmOrganizer.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        readonly IPopupService popupService;

        [ObservableProperty]
        List<string> appThemes;
        [ObservableProperty]
        AppTheme selectedTheme;

        [ObservableProperty]
        List<CropField> cropFields = new();
        [ObservableProperty]
        CropField defaultCropField;
        [ObservableProperty]
        bool cropFieldPickerEnabled = true;

        #region Preference Keys
        public const string AppThemeKey = "appTheme";
        public const string LedgerPage_DefaultCropField = "ledger_defaultCropFieldKey";
        #endregion

        public SettingsPageViewModel(IPopupService popupService)
        {
            this.popupService = popupService;
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
        async Task ResetDatabase()
        {
            if (await PopupExtensions.ShowConfirmationAsync(
                    popupService,
                    "Uwaga!",
                    "Za chwilę bezpowrotnie wyczyścisz wszystkie dane z bazy danych. " +
                    "Tej akcji nie można odwrócić. Czy jesteś pewny aby kontynuować?"
                    )
                )
            {
                if (!await PermissionManager.RequestPermissionsAsync())
                    return;

                await DatabaseFile.Delete();
                await DatabaseFile.Create();
            }
        }

        [RelayCommand]
        async Task ExportDatabase()
        {
            try
            {
                if (!await PermissionManager.RequestPermissionsAsync())
                    return;
                FolderPickerResult folder = await FolderPicker.PickAsync(default);
                if (!folder.IsSuccessful)
                    return;
                await DatabaseFile.ExportTo(folder.Folder.Path);
                PopupExtensions.ShowAlert(
                    popupService,
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
        async Task ImportDatabase()
        {
            try
            {
                if (!await PermissionManager.RequestPermissionsAsync())
                    return;
                await DatabaseFile.CreateBackup();
                FileResult file = await FilePicker.PickAsync();
                if (file == null)
                {
                    DatabaseFile.DeleteBackup();
                    return;
                }
                await DatabaseFile.ImportFrom(file.FullPath);
                PopupExtensions.ShowAlert(
                    popupService,
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

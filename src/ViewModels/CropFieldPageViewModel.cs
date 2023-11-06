using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.Services;
using Microsoft.Data.Sqlite;

namespace FarmOrganizer.ViewModels
{
    public partial class CropFieldPageViewModel : ObservableObject
    {
        readonly IPopupService popupService;

        [ObservableProperty]
        List<CropField> cropFields = new();

        [ObservableProperty]
        bool showCreatorFrame = false;
        [ObservableProperty]
        string saveButtonText = _buttonTextAddingEntry;
        const string _buttonTextAddingEntry = "Dodaj pole i zapisz";
        const string _buttonTextEditingEntry = "Zapisz zmiany";

        bool addingEntry = false;
        bool editingEntry = false;
        int editedEntryId;

        [ObservableProperty]
        string cropFieldName = "Nowe pole";
        [ObservableProperty]
        string cropFieldHectares;

        public CropFieldPageViewModel(IPopupService popupService)
        {
            this.popupService = popupService;
            try
            {
                CropFields = CropField.RetrieveAll(null);
            }
            catch (TableValidationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        [RelayCommand]
        private void AddOrSave()
        {
            try
            {
                decimal hectares = Utils.CastToValue(CropFieldHectares);
                var cropField = new CropField()
                {
                    Name = CropFieldName,
                    Hectares = hectares
                };
                if (addingEntry)
                {
                    CropField.AddEntry(cropField, null);
                }
                else if (editingEntry)
                {
                    cropField.Id = editedEntryId;
                    CropField.EditEntry(cropField, null);
                }

                CropFields = new DatabaseContext().CropFields.ToList();
                ToggleAdding();
            }
            catch (InvalidRecordPropertyException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
            catch (NoRecordFoundException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
            catch (SqliteException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
        }

        [RelayCommand]
        private void Edit(CropField cropFieldToEdit)
        {
            editedEntryId = cropFieldToEdit.Id;
            CropFieldName = cropFieldToEdit.Name;
            CropFieldHectares = cropFieldToEdit.Hectares.ToString();
            editingEntry = true;
            addingEntry = false;
            SaveButtonText = _buttonTextEditingEntry;
            ShowCreatorFrame = true;
        }

        [RelayCommand]
        private async Task Remove(CropField cropFieldToRemove)
        {
            if (!await PopupExtensions.ShowConfirmationAsync(
                    popupService,
                    "Uwaga!",
                    "Usunięcie pola uprawnego usunie również WSZYSTKIE wpisy, które były podpięte pod usuwane pole. " +
                    "Tej operacji nie można cofnąć. Czy chcesz kontynuować?"
                    )
                )
                return;
            try
            {
                CropField.DeleteEntry(cropFieldToRemove.Id, null);
                CropFields = new DatabaseContext().CropFields.ToList();
            }
            catch (RecordDeletionException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
            finally
            {
                ToggleAdding();
                ShowCreatorFrame = false;
            }
        }

        [RelayCommand]
        private void ToggleAdding()
        {
            editingEntry = false;
            addingEntry = true;
            SaveButtonText = _buttonTextAddingEntry;
            ShowCreatorFrame = !ShowCreatorFrame;
        }
    }
}

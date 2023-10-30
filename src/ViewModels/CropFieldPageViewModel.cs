using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using Microsoft.Data.Sqlite;

namespace FarmOrganizer.ViewModels
{
    public partial class CropFieldPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<CropField> cropFields = new();

        [ObservableProperty]
        private bool showCreatorFrame = false;
        [ObservableProperty]
        private string saveButtonText = _buttonTextAddingEntry;
        private const string _buttonTextAddingEntry = "Dodaj pole i zapisz";
        private const string _buttonTextEditingEntry = "Zapisz zmiany";

        private bool addingEntry = false;
        private bool editingEntry = false;
        private int editedEntryId;

        [ObservableProperty]
        private string cropFieldName = "Nowe pole";
        [ObservableProperty]
        private string cropFieldHectares;

        public CropFieldPageViewModel()
        {
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
            if (!await App.AlertSvc.ShowConfirmationAsync(
                "Uwaga!",
                "Usunięcie pola uprawnego usunie również WSZYSTKIE wpisy, które były podpięte pod usuwane pole. Tej operacji nie można cofnąć. Czy chcesz kontynuować?",
                "Tak, usuń",
                "Anuluj"))
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels
{
    public partial class CropFieldPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<CropField> cropFields;

        [ObservableProperty]
        private bool showCreatorFrame = false;
        [ObservableProperty]
        private string saveButtonText = "Dodaj pole i zapisz";

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
                using var context = new DatabaseContext();
                CropFields = context.CropFields.ToList();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        [RelayCommand]
        private void AddOrSave()
        {
            try
            {
                using var context = new DatabaseContext();
                decimal hectares = Utils.CastToValue(CropFieldHectares);
                if (hectares <= 0)
                    throw new InvalidRecordException("Pole powierzchni", hectares.ToString());

                if (addingEntry)
                {
                    CropField newField = new()
                    {
                        Name = CropFieldName,
                        Hectares = hectares
                    };
                    context.CropFields.Add(newField); 
                }
                else if (editingEntry)
                {
                    CropField existingField = context.CropFields.Find(editedEntryId);
                    existingField.Name = CropFieldName;
                    existingField.Hectares = hectares;
                }

                context.SaveChanges();
                CropFields = context.CropFields.ToList();
                ToggleAdding();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
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
            SaveButtonText = "Zapisz zmiany";
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
                using var context = new DatabaseContext();
                context.CropFields.Remove(cropFieldToRemove);
                context.SaveChanges();
                CropFields = context.CropFields.ToList();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        [RelayCommand]
        private void ToggleAdding()
        {
            editingEntry = false;
            addingEntry = true;
            SaveButtonText = "Dodaj pole i zapisz";
            ShowCreatorFrame = !ShowCreatorFrame;
        }
    }
}

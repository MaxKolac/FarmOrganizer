using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using Microsoft.Data.Sqlite;

namespace FarmOrganizer.ViewModels
{
    public partial class CostTypePageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<CostType> costTypes = new();
        [ObservableProperty]
        private bool showCreatorFrame = false;
        [ObservableProperty]
        private string saveButtonText = "Dodaj rodzaj i zapisz";

        private bool addingEntry = false;
        private bool editingEntry = false;
        private int editedEntryId;

        [ObservableProperty]
        private string costTypeName = "Nowy rodzaj";
        [ObservableProperty]
        private bool costTypeIsExpense;

        public CostTypePageViewModel()
        {
            try
            {
                CostType.Validate(out List<CostType> allEntries);
                CostTypes.AddRange(allEntries);
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
                if (addingEntry)
                {
                    CostType newCostType = new()
                    {
                        Name = CostTypeName,
                        IsExpense = CostTypeIsExpense
                    };
                    CostType.AddEntry(newCostType);
                }
                else if (editingEntry)
                {
                    CostType costTypeToEdit = new()
                    {
                        Id = editedEntryId,
                        Name = CostTypeName,
                        IsExpense = CostTypeIsExpense
                    };
                    CostType.EditEntry(costTypeToEdit);
                }

                CostTypes = new DatabaseContext().CostTypes.ToList();
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
        private void Edit(CostType costToEdit)
        {
            editedEntryId = costToEdit.Id;
            CostTypeName = costToEdit.Name;
            CostTypeIsExpense = costToEdit.IsExpense;
            editingEntry = true;
            addingEntry = false;
            SaveButtonText = "Zapisz zmiany";
            ShowCreatorFrame = true;
        }

        [RelayCommand]
        private async Task Remove(CostType costToRemove)
        {
            try
            {
                if (!await App.AlertSvc.ShowConfirmationAsync(
                "Uwaga!",
                "Usunięcie rodzaju wpisu usunie również WSZYSTKIE wpisy z tego rodzaju. Tej operacji nie można cofnąć. Czy chcesz kontynuować?",
                "Tak, usuń",
                "Anuluj"))
                    return;
                CostType.DeleteEntry(costToRemove);
                CostTypes = new DatabaseContext().CostTypes.ToList();
            }
            catch (RecordDeletionException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
        }

        [RelayCommand]
        private void ToggleAdding() 
        {
            editingEntry = false;
            addingEntry = true;
            SaveButtonText = "Dodaj rodzaj i zapisz";
            ShowCreatorFrame = !ShowCreatorFrame;
        }
    }
}

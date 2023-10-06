using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.HelperClasses;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;

namespace FarmOrganizer.ViewModels
{
    public partial class CostTypePageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CostTypeGroup> costTypeGroups;
        [ObservableProperty]
        private bool showCreatorFrame = false;

        [ObservableProperty]
        private string costTypeLabel = _costTypeLabelExpense;
        private const string _costTypeLabelExpense = "Wydatek";
        private const string _costTypeLabelProfit = "Przychód";
        [ObservableProperty]
        private string saveButtonText = _saveButtonAddText;
        private const string _saveButtonAddText = "Dodaj rodzaj i zapisz";
        private const string _saveButtonEditText = "Zapisz zmiany";

        private bool addingEntry = false;
        private bool editingEntry = false;
        private int editedEntryId;

        [ObservableProperty]
        private string costTypeName = "Nowy rodzaj";
        [ObservableProperty]
        private bool costTypeIsExpense = true;

        public CostTypePageViewModel()
        {
            try
            {
                using var context = new DatabaseContext();
                CostType.Validate(context);
                CostTypeGroups = CostType.BuildCostTypeGroups(context);
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
                var costType = new CostType()
                {
                    Name = CostTypeName,
                    IsExpense = CostTypeIsExpense
                };
                if (addingEntry)
                {
                    CostType.AddEntry(costType, null);
                }
                else if (editingEntry)
                {
                    costType.Id = editedEntryId;
                    CostType.EditEntry(costType, null);
                }
                CostTypeGroups = CostType.BuildCostTypeGroups(null);
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
            SaveButtonText = _saveButtonEditText;
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
                CostType.DeleteEntry(costToRemove.Id, null);
                CostTypeGroups = CostType.BuildCostTypeGroups(null);
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
            SaveButtonText = _saveButtonAddText;
            ShowCreatorFrame = !ShowCreatorFrame;
        }

        partial void OnCostTypeIsExpenseChanged(bool value) =>
            CostTypeLabel = value ? _costTypeLabelExpense : _costTypeLabelProfit;
    }
}

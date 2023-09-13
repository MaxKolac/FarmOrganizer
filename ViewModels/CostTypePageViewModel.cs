using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels
{
    public partial class CostTypePageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<CostType> costTypes;
        [ObservableProperty]
        private bool showCreatorFrame = false;
        [ObservableProperty]
        private string saveButtonText = "Dodaj koszt i zapisz";

        private bool addingEntry = false;
        private bool editingEntry = false;
        private int editedEntryId;

        [ObservableProperty]
        private string costTypeName = "Nowy rodzaj kosztu";
        [ObservableProperty]
        private bool costTypeIsExpense;

        public CostTypePageViewModel()
        {
            try
            {
                CostType.Validate();
                CostTypes = new DatabaseContext().CostTypes.ToList();
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
                    //TODO: IValidatable.EditEntry
                    CostType existingCostType = context.CostTypes.Find(editedEntryId);
                    existingCostType.Name = CostTypeName;
                    existingCostType.IsExpense = CostTypeIsExpense;
                }
                context.SaveChanges();
                CostTypes = context.CostTypes.ToList();
                ToggleAdding();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
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
            using var context = new DatabaseContext();
            List<CostType> allExpenses = new();
            List<CostType> allProfits = new();
            foreach (CostType cost in CostTypes)
            {
                if (cost.IsExpense)
                    allExpenses.Add(cost);
                else
                    allProfits.Add(cost);
            }
            if (allExpenses.Count == 1 && costToRemove.IsExpense)
            {
                App.AlertSvc.ShowAlert("Wystąpił błąd", "Nie można usunąć ostatniego rodzaju kosztu, który jest wydatkiem. Aby aplikacja działała poprawnie, musi istnieć przynajmniej jeden taki rodzaj kosztu.");
                return;
            }
            if (allProfits.Count == 1 && !costToRemove.IsExpense)
            {
                App.AlertSvc.ShowAlert("Wystąpił błąd", "Nie można usunąć ostatniego rodzaju kosztu, który jest przychodem. Aby aplikacja działała poprawnie, musi istnieć przynajmniej jeden taki rodzaj kosztu.");
                return;
            }

            if (!await App.AlertSvc.ShowConfirmationAsync(
                "Uwaga!",
                "Usunięcie rodzaju kosztu usunie również WSZYSTKIE wpisy z kosztami, które były podpięte pod usuwany rodzaj. Tej operacji nie można cofnąć. Czy chcesz kontynuować?",
                "Tak, usuń",
                "Anuluj"))
                return;
            try
            {
                context.CostTypes.Remove(costToRemove);
                context.SaveChanges();
                CostTypes = context.CostTypes.ToList();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        [RelayCommand]
        private void ToggleAdding() 
        {
            editingEntry = false;
            addingEntry = true;
            SaveButtonText = "Dodaj koszt i zapisz";
            ShowCreatorFrame = !ShowCreatorFrame;
        }
    }
}

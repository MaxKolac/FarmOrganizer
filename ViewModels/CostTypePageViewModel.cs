using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using System.Globalization;

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
                using var context = new DatabaseContext();
                CostTypes = context.CostTypes.ToList();
                //There needs to be at least 1 expense and 1 income category
                bool expenseFound = false;
                bool incomeFound = false;
                foreach (CostType type in CostTypes)
                {
                    if (type.IsExpense)
                        expenseFound = true;
                    else
                        incomeFound = true;
                    if (expenseFound && incomeFound)
                        break;
                }
                if (!expenseFound || !incomeFound)
                    throw new NoRecordFoundException(nameof(DatabaseContext.CostTypes), "W tabeli nie znaleziono przynajmniej jednego kosztu traktowanego jako wydatek lub przynajmniej jednego kosztu traktowanego jako zysk.");
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
                    context.CostTypes.Add(newCostType); 
                }
                else if (editingEntry)
                {
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

    internal class IsExpenseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
                return string.Empty;
            bool isExpense = (bool)value;
            return isExpense ? "Wydatek" : "Przychód";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

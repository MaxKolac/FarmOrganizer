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
        private bool addingNewCostType;

        #region Cost Type Properties
        [ObservableProperty]
        private string newCostName;
        [ObservableProperty]
        private bool newCostIsExpense;
        #endregion

        public CostTypePageViewModel()
        {
            try
            {
                using var context = new DatabaseContext();
                CostTypes = context.CostTypes.ToList();
                //There needs to be at least 1 costToRemove and 1 income category
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
        private void AddNewCostType()
        {
            try
            {
                using var context = new DatabaseContext();
                CostType newCostType = new()
                {
                    Name = NewCostName,
                    IsExpense = NewCostIsExpense
                };
                context.CostTypes.Add(newCostType);
                context.SaveChanges();
                CostTypes = context.CostTypes.ToList();
                ToggleAddingNewCostType();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        [RelayCommand]
        private async Task RemoveCostType(CostType costToRemove)
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
        private void ToggleAddingNewCostType() =>
            AddingNewCostType = !AddingNewCostType;
    }

    internal class IsExpenseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
                return string.Empty;
            bool isExpense = (bool)value;
            return isExpense ? "Wydatek" : "Zarobek";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

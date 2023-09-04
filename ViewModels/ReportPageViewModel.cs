using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels
{
    [QueryProperty(nameof(passedLedgerEntries), "entries")]
    [QueryProperty(nameof(PassedCropField), "cropfield")]
    [QueryProperty(nameof(PassedSeason), "season")]
    public partial class ReportPageViewModel : QuickCalculatorViewModel, IQueryAttributable
    {
        #region Query Properties
        private List<BalanceLedger> passedLedgerEntries;
        [ObservableProperty]
        private CropField passedCropField;
        [ObservableProperty]
        private Season passedSeason;
        #endregion

        #region Money Related Properties and Lists
        [ObservableProperty]
        private List<CostTypeReportEntry> expenseEntries = new();
        [ObservableProperty]
        private List<CostTypeReportEntry> profitEntries = new();
        [ObservableProperty]
        private decimal totalExpense = 0.0m;
        [ObservableProperty]
        private decimal totalProfit = 0.0m;
        [ObservableProperty]
        private decimal totalChange = 0.0m;
        [ObservableProperty]
        private decimal profitAfterExpenses = 0.0m;
        #endregion

        [ObservableProperty]
        private bool addNewSeasonAfterSaving = false;
        [ObservableProperty]
        private string newSeasonName;
        [ObservableProperty]
        private List<CostType> costTypes;
        [ObservableProperty]
        private CostType selectedCostType;

        public ReportPageViewModel()
        {
            try
            {
                using var context = new DatabaseContext();
                CostTypes = context.CostTypes.Where(cost => !cost.IsExpense).ToList();
                if (CostTypes.Count == 0)
                    throw new NoRecordFoundException(nameof(DatabaseContext.CostTypes), 
                        "Nie znaleziono żadnych rodzajów kosztów, które nie są wydatkami.");
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            passedLedgerEntries = query["entries"] as List<BalanceLedger>;
            PassedCropField = query["cropfield"] as CropField;
            PassedSeason = query["season"] as Season;

            var costDictionary = new Dictionary<CostType, decimal>();
            foreach (BalanceLedger entry in passedLedgerEntries)
            {
                CostType cost = entry.IdCostTypeNavigation;
                if (cost.IsExpense)
                    TotalExpense += entry.BalanceChange;
                else
                    TotalProfit += entry.BalanceChange;

                if (costDictionary.ContainsKey(cost))
                    costDictionary[cost] += entry.BalanceChange;
                else
                    costDictionary.Add(cost, entry.BalanceChange);
            }

            foreach (KeyValuePair<CostType, decimal> kvp in costDictionary)
            {
                CostTypeReportEntry entry = new()
                {
                    Name = kvp.Key.Name,
                    Amount = kvp.Value
                };
                if (kvp.Key.IsExpense)
                    ExpenseEntries.Add(entry);
                else
                    ProfitEntries.Add(entry);
            }
            ExpenseEntries = ExpenseEntries.OrderBy(entry => entry.Name).ToList();
            ProfitEntries = ProfitEntries.OrderBy(entry => entry.Name).ToList();
            TotalChange = TotalProfit - TotalExpense;
            SelectedCostType = CostTypes.First();
        }

        [RelayCommand]
        private async Task AddNewLedgerEntry()
        {
            try
            {
                using var context = new DatabaseContext();
                BalanceLedger newEntry = new()
                {
                    IdCostType = SelectedCostType.Id,
                    IdCropField = PassedCropField.Id,
                    IdSeason = PassedSeason.Id,
                    DateAdded = DateTime.Now,
                    BalanceChange = ProfitAfterExpenses
                };
                context.BalanceLedgers.Add(newEntry);
                context.SaveChanges();
                if (AddNewSeasonAfterSaving)
                    SeasonsPageViewModel.StartNewSeason(NewSeasonName, DateTime.Now);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        protected override void OnIncomeChanged(string value)
        {
            decimal pureIncome = Utils.CastToValue(value);
            ProfitAfterExpenses = pureIncome + TotalChange;
        }
    }

    public class CostTypeReportEntry
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}
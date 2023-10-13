using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.IO;
using FarmOrganizer.IO.Exporting.PDF;
using FarmOrganizer.Models;
using Microsoft.Data.Sqlite;
using PdfSharpCore.Pdf;

namespace FarmOrganizer.ViewModels
{
    [QueryProperty(nameof(passedLedgerEntries), "entries")]
    [QueryProperty(nameof(cropFieldIds), "cropfields")]
    [QueryProperty(nameof(seasonIds), "seasons")]
    public partial class ReportPageViewModel : QuickCalculatorViewModel, IQueryAttributable
    {
        #region Query Properties
        private List<BalanceLedger> passedLedgerEntries;
        private List<int> cropFieldIds;
        private List<int> seasonIds;
        #endregion

        [ObservableProperty]
        private List<CropField> passedCropFields = new();
        [ObservableProperty]
        private List<Season> passedSeasons = new();

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

        #region Properties related to options at view's bottom
        [ObservableProperty]
        private bool addNewSeasonAfterSaving = false;
        [ObservableProperty]
        private string newSeasonName;
        [ObservableProperty]
        private List<CostType> costTypes;
        [ObservableProperty]
        private CostType selectedCostType;
        [ObservableProperty]
        private bool exportPdfWithPureIncome = false;
        #endregion

        #region Dynamic text
        [ObservableProperty]
        private string seasonsLabel = _seasonLabelSingle;
        private const string _seasonLabelSingle = "Sezon:";
        private const string _seasonLabelMultiple = "Sezony:";

        [ObservableProperty]
        private string cropFieldsLabel = _cropFieldLabelSingle;
        private const string _cropFieldLabelSingle = "Pole uprawne:";
        private const string _cropFieldLabelMultiple = "Pola uprawne:";

        [ObservableProperty]
        private string totalChangeText = _labelProfit;
        [ObservableProperty]
        private string profitAfterExpensesText = _labelProfit;
        private const string _labelProfit = "Zysk";
        private const string _labelLoss = "Straty";
        #endregion

        public static event EventHandler OnPageQuit;

        public ReportPageViewModel()
        {
            try
            {
                CostType.Validate(null);
                CostTypes = new DatabaseContext().CostTypes.Where(cost => !cost.IsExpense).ToList();
            }
            catch (TableValidationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            passedLedgerEntries = query["entries"] as List<BalanceLedger>;
            cropFieldIds = query["cropfields"] as List<int>;
            seasonIds = query["seasons"] as List<int>;

            using var context = new DatabaseContext();
            foreach (int id in cropFieldIds)
                PassedCropFields.Add(context.CropFields.FirstOrDefault(e => e.Id == id));
            foreach (int id in seasonIds)
                PassedSeasons.Add(context.Seasons.FirstOrDefault(e => e.Id == id));

            if (PassedSeasons.Count > 1)
                SeasonsLabel = _seasonLabelMultiple;
            if (PassedCropFields.Count > 1)
                CropFieldsLabel = _cropFieldLabelMultiple;

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
                var entry = new CostTypeReportEntry(kvp.Key.Name, kvp.Value);
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
            if (PassedCropFields.Count == 0 || PassedSeasons.Count == 0)
            {
                App.AlertSvc.ShowAlert("Nie można dodać nowego rekordu",
                    "Program nie jest pewien pod jaki sezon i pod jakie pole uprawne powinien być podpięty wpis ze sprzedaży, ponieważ wygenerowano raport bez zaznaczonego przynajmniej jednego sezonu i/lub jednego pola uprawnego.");
                return;
            }
            if (PassedCropFields.Count != 1 || PassedSeasons.Count != 1)
            {
                App.AlertSvc.ShowAlert("Nie można dodać nowego rekordu",
                    "Program nie jest pewien pod jaki sezon i pod jakie pole uprawne powinien być podpięty wpis ze sprzedaży, ponieważ wygenerowano raport zawierający więcej niż jeden sezon i/lub więcej niż jedno pole uprawne.");
                return;
            }

            try
            {
                using var context = new DatabaseContext();
                BalanceLedger newEntry = new()
                {
                    IdCostType = SelectedCostType.Id,
                    IdCropField = PassedCropFields[0].Id,
                    IdSeason = PassedSeasons[0].Id,
                    DateAdded = DateTime.Now,
                    BalanceChange = Utils.CastToValue(PureIncomeValue),
                    Notes = $"Sprzedaż {Utils.CastToValue(CropAmountValue)} kg przy stawce {Utils.CastToValue(SellRateValue)} zł za kilo."
                };
                if (AddNewSeasonAfterSaving)
                {
                    Season newSeason = new()
                    {
                        Name = NewSeasonName,
                        DateStart = DateTime.Now,
                        DateEnd = Season.MaximumDate
                        //HasConcluded = false
                    };
                    Season.AddEntry(newSeason, null);
                }
                context.BalanceLedgers.Add(newEntry);
                context.SaveChanges();
                OnPageQuit?.Invoke(this, null);
                await Shell.Current.GoToAsync("..");
            }
            catch (InvalidRecordPropertyException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
            catch (SqliteException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
        }

        [RelayCommand]
        private async Task ExportReportAsPDF()
        {
            var builder = new PdfBuilder(PassedCropFields, PassedSeasons, ExpenseEntries, ProfitEntries);
            if (ExportPdfWithPureIncome)
                builder.AddProfitEntry(new("Zysk ze sprzedaży (prognozowany)", Utils.CastToValue(PureIncomeValue)));
            var document = builder.Build();
            await PdfBuilder.Export(document);
        }

        protected override void OnIncomeChanged(string value) =>
            ProfitAfterExpenses = Utils.CastToValue(value) + TotalChange;

        partial void OnTotalChangeChanged(decimal value) =>
            TotalChangeText = value >= 0 ? _labelProfit : _labelLoss;

        partial void OnProfitAfterExpensesChanged(decimal value) =>
            ProfitAfterExpensesText = value >= 0 ? _labelProfit : _labelLoss;
    }

    /// <summary>
    /// Used by <see cref="ReportPageViewModel"/> to show individual cost types and the sum of their expenses.
    /// </summary>
    public record class CostTypeReportEntry(string Name, decimal Amount);
}
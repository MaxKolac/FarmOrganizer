using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.IO;
using FarmOrganizer.IO.Exporting.PDF;
using FarmOrganizer.Models;
using FarmOrganizer.Services;
using Microsoft.Data.Sqlite;

namespace FarmOrganizer.ViewModels
{
    [QueryProperty(nameof(passedLedgerEntries), "entries")]
    [QueryProperty(nameof(cropFieldIds), "cropfields")]
    [QueryProperty(nameof(seasonIds), "seasons")]
    public partial class ReportPageViewModel : ObservableObject, IQueryAttributable
    {
        readonly IPopupService popupService;

        #region Query Properties
        List<BalanceLedger> passedLedgerEntries;
        List<int> cropFieldIds;
        List<int> seasonIds;
        #endregion

        [ObservableProperty]
        List<CropField> passedCropFields = new();
        [ObservableProperty]
        List<Season> passedSeasons = new();

        [ObservableProperty]
        decimal cropAmountValue;
        [ObservableProperty]
        decimal sellRateValue;
        [ObservableProperty]
        decimal pureIncomeValue;

        #region Money Related Properties and Lists
        [ObservableProperty]
        List<CostTypeReportEntry> expenseEntries = new();
        [ObservableProperty]
        List<CostTypeReportEntry> profitEntries = new();
        [ObservableProperty]
        decimal totalExpense = 0.0m;
        [ObservableProperty]
        decimal totalProfit = 0.0m;
        [ObservableProperty]
        decimal totalChange = 0.0m;
        [ObservableProperty]
        decimal profitAfterExpenses = 0.0m;
        #endregion

        #region Properties related to options at view's bottom
        [ObservableProperty]
        bool addNewSeasonAfterSaving = false;
        [ObservableProperty]
        string newSeasonName;
        [ObservableProperty]
        List<CostType> costTypes;
        [ObservableProperty]
        CostType selectedCostType;
        [ObservableProperty]
        bool exportPdfWithPureIncome = false;
        #endregion

        #region Dynamic text
        [ObservableProperty]
        string seasonsLabel = _seasonLabelSingle;
        const string _seasonLabelSingle = "Sezon:";
        const string _seasonLabelMultiple = "Sezony:";

        [ObservableProperty]
        string cropFieldsLabel = _cropFieldLabelSingle;
        const string _cropFieldLabelSingle = "Pole uprawne:";
        const string _cropFieldLabelMultiple = "Pola uprawne:";

        [ObservableProperty]
        string totalChangeText = _labelProfit;
        [ObservableProperty]
        string profitAfterExpensesText = _labelProfit;
        const string _labelProfit = "Zysk";
        const string _labelLoss = "Straty";
        #endregion

        public static event EventHandler OnPageQuit;

        public ReportPageViewModel(IPopupService popupService)
        {
            this.popupService = popupService;
            try
            {
                CostType.Validate(null);
                CostTypes = new DatabaseContext().CostTypes.Where(cost => !cost.IsExpense).ToList();
            }
            catch (TableValidationException ex)
            {
                ExceptionHandler.Handle(popupService, ex, true);
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
            ProfitAfterExpenses = TotalChange - PureIncomeValue;
            SelectedCostType = CostTypes.First();
        }

        [RelayCommand]
        private async Task AddNewLedgerEntry()
        {
            if (PassedCropFields.Count == 0 || PassedSeasons.Count == 0)
            {
                PopupExtensions.ShowAlert(
                    popupService,
                    "Nie można dodać nowego rekordu",
                    "Program nie jest pewien pod jaki sezon i pod jakie pole uprawne powinien być podpięty wpis ze sprzedaży, ponieważ wygenerowano raport bez zaznaczonego przynajmniej jednego sezonu i/lub jednego pola uprawnego."
                    );
                return;
            }
            if (PassedCropFields.Count != 1 || PassedSeasons.Count != 1)
            {
                PopupExtensions.ShowAlert(
                    popupService,
                    "Nie można dodać nowego rekordu",
                    "Program nie jest pewien pod jaki sezon i pod jakie pole uprawne powinien być podpięty wpis ze sprzedaży, ponieważ wygenerowano raport zawierający więcej niż jeden sezon i/lub więcej niż jedno pole uprawne."
                    );
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
                    BalanceChange = PureIncomeValue,
                    Notes = $"Sprzedaż {CropAmountValue} kg przy stawce {SellRateValue} zł za kilo."
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
                ExceptionHandler.Handle(popupService, ex, false);
            }
            catch (SqliteException ex)
            {
                ExceptionHandler.Handle(popupService, ex, false);
            }
        }

        [RelayCommand]
        private async Task ExportReportAsPDF()
        {
            try
            {
                var builder = new PdfBuilder(AppInfo.Current.Name,
                                             AppInfo.Current.VersionString,
                                             PassedCropFields,
                                             PassedSeasons,
                                             ExpenseEntries,
                                             ProfitEntries);
                if (ExportPdfWithPureIncome)
                    builder.AddProfitEntry(new("Zysk ze sprzedaży (prognozowany)", PureIncomeValue));
                var document = builder.Build();
                if (!await PermissionManager.RequestPermissionsAsync(popupService))
                    return;
                FolderPickerResult folder = await FolderPicker.PickAsync(default);
                if (!folder.IsSuccessful)
                    return;
                document.Save(Path.Combine(folder.Folder.Path, PdfBuilder.Filename));
                PopupExtensions.ShowAlert(
                    popupService,
                    "Sukces",
                    $"Raport wyeksportowano do folderu {folder.Folder.Path} pod nazwą {PdfBuilder.Filename}.");
            }
            catch (Exception ex)
            {
                PopupExtensions.ShowAlert(popupService, "Błąd", ex.ToString());
            }
        }

        partial void OnTotalChangeChanged(decimal value) =>
            TotalChangeText = value >= 0 ? _labelProfit : _labelLoss;

        partial void OnPureIncomeValueChanged(decimal value) =>
            ProfitAfterExpenses = TotalChange + value;

        partial void OnProfitAfterExpensesChanged(decimal value) =>
            ProfitAfterExpensesText = value >= 0 ? _labelProfit : _labelLoss;
    }

    /// <summary>
    /// Used by <see cref="ReportPageViewModel"/> to show individual cost types and the sum of their expenses.
    /// </summary>
    public record class CostTypeReportEntry(string Name, decimal Amount);
}
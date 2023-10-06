using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.HelperClasses;
using FarmOrganizer.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FarmOrganizer.ViewModels
{
    public partial class LedgerPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<BalanceLedger> ledgerEntries = new();
        [ObservableProperty]
        private bool showLedgerEmptyInfo = false;
        [ObservableProperty]
        private bool showLedger = false;
        [ObservableProperty]
        private bool isBusy = true;

        #region Preferences
        private CropField preferredCropField;
        #endregion

        private LedgerFilterSet _filterSet;

        public LedgerPageViewModel()
        {
            LedgerRecordPageViewModel.OnPageQuit += QueryLedgerEntries;
            ReportPageViewModel.OnPageQuit += QueryLedgerEntries;
            LedgerFilterPageViewModel.OnFilterSetCreated += ApplyFilters;
            _filterSet = GetDefaultFilterSet();
            Task.Run(LoadDatabaseDataAsync);
        }

        private async Task LoadDatabaseDataAsync()
        {
            try
            {
                Task costsValidation = Task.Run(() => CostType.Validate(null));
                Task seasonValidation = Task.Run(() => Season.Validate(null));
                Task<List<CropField>> cropFieldValidation = Task.Run(() => CropField.RetrieveAll(null));

                //Load preferences
                List<CropField> allCropFields = cropFieldValidation.Result;
                preferredCropField = allCropFields.FirstOrDefault(field =>
                    field.Id == Preferences.Get(
                        SettingsPageViewModel.LedgerPage_DefaultCropField,
                        allCropFields.First().Id
                        )
                    );
                preferredCropField ??= allCropFields.First();

                await Task.WhenAll(costsValidation, cropFieldValidation, seasonValidation);
                //await Task.Delay(1000);
                QueryLedgerEntries(this, null);
            }
            catch (TableValidationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        [RelayCommand]
        public async Task ReturnToPreviousPage()
        {
            LedgerRecordPageViewModel.OnPageQuit -= QueryLedgerEntries;
            ReportPageViewModel.OnPageQuit -= QueryLedgerEntries;
            LedgerFilterPageViewModel.OnFilterSetCreated -= ApplyFilters;
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task AddRecord()
        {
            var query = new Dictionary<string, object>
            {
                { "mode", "add" },
                { "id", 0 },
                { "cropFieldId", preferredCropField.Id }
            };
            await Shell.Current.GoToAsync(nameof(LedgerRecordPage), query);
        }

        [RelayCommand]
        private async Task EditRecord(BalanceLedger record)
        {
            var query = new Dictionary<string, object>
            {
                { "mode", "edit" },
                { "id", record.Id },
                { "cropFieldId", preferredCropField.Id }
            };
            await Shell.Current.GoToAsync(nameof(LedgerRecordPage), query);
        }

        [RelayCommand]
        private void DeleteRecord(BalanceLedger record)
        {
            using var context = new DatabaseContext();
            context.BalanceLedgers.Remove(record);
            context.SaveChanges();
            QueryLedgerEntries(this, null);
        }

        [RelayCommand]
        private async Task GenerateReport()
        {
            var query = new Dictionary<string, object>()
            {
                { "entries", LedgerEntries.ToList() },
                { "cropfields", _filterSet.SelectedCropFieldIds },
                { "seasons", _filterSet.SelectedSeasonIds }
            };
            await Shell.Current.GoToAsync(nameof(ReportPage), query);
        }

        [RelayCommand]
        private async Task FilterAndSortRecords()
        {
            var query = new Dictionary<string, object>()
            {
                { "filterSet", _filterSet }
            };
            await Shell.Current.GoToAsync(nameof(LedgerFilterPage), query);
        }

        private void ApplyFilters(object sender, LedgerFilterSet newSet)
        {
            _filterSet = newSet;
            QueryLedgerEntries(this, null);
        }

        public static LedgerFilterSet GetDefaultFilterSet()
        {
            LedgerFilterSet set = new()
            {
                //TODO: load default filters from Preferences
                SmallestBalanceChange = 0,
                LargestBalanceChange = 999_999m,
                EarliestDate = DateTime.Now.AddMonths(-1),
                LatestDate = DateTime.Now.Date.AddDays(1).AddMicroseconds(-1),
                SelectedSeasonIds = new() { Season.GetCurrentSeason().Id }
            };
            return set;
        }

        public void QueryLedgerEntries(object sender, EventArgs e)
        {
            IsBusy = true;
            ShowLedger = false;
            IEnumerable<BalanceLedger> query =
                from entry in new DatabaseContext().BalanceLedgers
                .Include(entry => entry.IdCostTypeNavigation)
                .Include(entry => entry.IdSeasonNavigation)
                .Include(entry => entry.IdCropFieldNavigation)
                where _filterSet.SelectedCropFieldIds.Contains(entry.IdCropField)
                && _filterSet.SelectedCostTypeIds.Contains(entry.IdCostType)
                && _filterSet.SelectedSeasonIds.Contains(entry.IdSeason)
                && _filterSet.EarliestDate <= entry.DateAdded
                && entry.DateAdded <= _filterSet.LatestDate
                && _filterSet.SmallestBalanceChange <= entry.BalanceChange
                && entry.BalanceChange <= _filterSet.LargestBalanceChange
                select entry;
            //await Task.Delay(500);

            List<BalanceLedger> retrievedEntries = query.ToList();
            switch (_filterSet.SortingMethod)
            {
                case LedgerFilterSet.SortingCriteria.CostTypes:
                    retrievedEntries = retrievedEntries.OrderBy(entry => entry.IdCostTypeNavigation.Name).ToList();
                    break;
                case LedgerFilterSet.SortingCriteria.DateAdded:
                    retrievedEntries = retrievedEntries.OrderBy(entry => entry.DateAdded).ToList();
                    break;
                case LedgerFilterSet.SortingCriteria.SeasonStartDate:
                    retrievedEntries = retrievedEntries.OrderBy(entry => entry.IdSeasonNavigation.DateStart).ToList();
                    break;
                case LedgerFilterSet.SortingCriteria.BalanceChange:
                    retrievedEntries = retrievedEntries.OrderBy(entry => entry.BalanceChange).ToList();
                    break;
            }
            //await Task.Delay(500);

            LedgerEntries.Clear();
            if (_filterSet.DescendingSort)
            {
                for (int i = retrievedEntries.Count - 1; i >= 0; i--)
                    LedgerEntries.Add(retrievedEntries[i]);
            }
            else
            {
                for (int i = 0; i < retrievedEntries.Count; i++)
                    LedgerEntries.Add(retrievedEntries[i]);
            }
            ShowLedger = LedgerEntries.Any();
            ShowLedgerEmptyInfo = !ShowLedger;
            IsBusy = false;
        }
    }
}

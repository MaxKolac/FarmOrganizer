using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.HelperClasses;
using FarmOrganizer.Views;
using Microsoft.EntityFrameworkCore;

namespace FarmOrganizer.ViewModels
{
    public partial class LedgerPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<BalanceLedger> ledgerEntries = new();
        [ObservableProperty]

        #region Preferences
        private readonly CropField preferredCropField;
        #endregion

        private LedgerFilterSet _filterSet;

        public LedgerPageViewModel()
        {
            LedgerRecordPageViewModel.OnPageQuit += QueryLedgerEntries;
            ReportPageViewModel.OnPageQuit += QueryLedgerEntries;
            LedgerFilterPageViewModel.OnFilterSetCreated += ApplyFilters;
            _filterSet = new()
            {
                //TODO: load default filters from Preferences
                SmallestBalanceChange = 0,
                LargestBalanceChange = 999_999m,
                EarliestDate = DateTime.Now.AddMonths(-1),
                LatestDate = DateTime.Now.Date.AddDays(1).AddMicroseconds(-1)
            };
            //TODO: select the season that is going through as of right now
            _filterSet.SelectedSeasonIds = new() { _filterSet.SelectedSeasonIds.Last() };
            try
            {
                Season.Validate();
                CostType.Validate();
                CropField.Validate(out List<CropField> allCropFields);
                QueryLedgerEntries(this, null);

                preferredCropField = allCropFields.Find(field =>
                    field.Id == Preferences.Get(
                        SettingsPageViewModel.LedgerPage_DefaultCropField,
                        allCropFields.First().Id
                        )
                    );
                preferredCropField ??= allCropFields.First();
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
                //TODO: multiple cropfields, multiple seasons
                { "entries", LedgerEntries },
                { "cropfield", preferredCropField },
                { "season", Season.GetCurrentSeason() }
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

        public void QueryLedgerEntries(object sender, EventArgs e)
        {
            using var context = new DatabaseContext();
            IEnumerable<BalanceLedger> query =
                from entry in context.BalanceLedgers.Include(entry => entry.IdCostTypeNavigation)
                                                    .Include(entry => entry.IdSeasonNavigation)
                where _filterSet.SelectedCropFieldIds.Contains(entry.IdCropField)
                && _filterSet.SelectedCostTypeIds.Contains(entry.IdCostType)
                && _filterSet.SelectedSeasonIds.Contains(entry.IdSeason)
                && _filterSet.EarliestDate <= entry.DateAdded
                && entry.DateAdded <= _filterSet.LatestDate
                && _filterSet.SmallestBalanceChange <= entry.BalanceChange
                && entry.BalanceChange <= _filterSet.LargestBalanceChange
                select entry;
            LedgerEntries = query.ToList();

            switch (_filterSet.SortingMethod)
            {
                case LedgerFilterSet.SortingCriteria.CostTypes:
                    LedgerEntries = LedgerEntries.OrderBy(entry => entry.IdCostTypeNavigation.Name).ToList();
                    break;
                case LedgerFilterSet.SortingCriteria.DateAdded:
                    LedgerEntries = LedgerEntries.OrderBy(entry => entry.DateAdded).ToList();
                    break;
                case LedgerFilterSet.SortingCriteria.SeasonStartDate:
                    LedgerEntries = LedgerEntries.OrderBy(entry => entry.IdSeasonNavigation.DateStart).ToList();
                    break;
                case LedgerFilterSet.SortingCriteria.BalanceChange:
                    LedgerEntries = LedgerEntries.OrderBy(entry => entry.BalanceChange).ToList();
                    break;
            }
            if (_filterSet.DescendingSort)
                LedgerEntries.Reverse();
        }
    }
}

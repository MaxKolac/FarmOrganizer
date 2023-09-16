using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.HelperClasses;
using FarmOrganizer.ViewModels.PopUps;
using FarmOrganizer.Views;
using FarmOrganizer.Views.PopUps;
using Microsoft.EntityFrameworkCore;
using Mopups.Interfaces;

namespace FarmOrganizer.ViewModels
{
    public partial class LedgerPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<BalanceLedger> ledgerEntries = new();
        [ObservableProperty]
        private List<CostType> costTypes = new();
        [ObservableProperty]
        private List<CropField> cropFields = new();
        [ObservableProperty]
        private CropField selectedCropField;

        [ObservableProperty]
        private bool filterPopupButtonEnabled = true;

        private LedgerFilterSet _filterSet;
        private readonly IPopupNavigation _popUpSvc;

        public LedgerPageViewModel(IPopupNavigation popupNavigation)
        {
            LedgerRecordPageViewModel.OnPageQuit += QueryLedgerEntries;
            ReportPageViewModel.OnPageQuit += QueryLedgerEntries;
            LedgerFilterPopupViewModel.OnPageQuit += EnableFilterPopupButton;
            LedgerFilterPopupViewModel.OnFilterSetCreated += ApplyFilters;
            _popUpSvc = popupNavigation;
            _filterSet = new()
            {
                //TODO: load default filters from Preferences
                SmallestBalanceChange = 0,
                LargestBalanceChange = 999_999m,
                EarliestDate = DateTime.Now.AddMonths(-1),
                LatestDate = DateTime.Now
            };
            try
            {
                CostType.Validate(out List<CostType> allCostTypes);
                CostTypes.AddRange(allCostTypes);
                CropField.Validate(out List<CropField> allCropFields);
                CropFields.AddRange(allCropFields);
                //OnSelectedCropField already triggers QueryLedgerTables(); no need to call it twice
                SelectedCropField = CropFields.Find(field =>
                    field.Id == Preferences.Get(
                        SettingsPageViewModel.LedgerPage_DefaultCropField,
                        CropFields.First().Id
                        )
                    );
                SelectedCropField ??= CropFields.First();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        [RelayCommand]
        public async Task ReturnToPreviousPage()
        {
            LedgerRecordPageViewModel.OnPageQuit -= QueryLedgerEntries;
            ReportPageViewModel.OnPageQuit -= QueryLedgerEntries;
            LedgerFilterPopupViewModel.OnFilterSetCreated -= ApplyFilters;
            await Shell.Current.GoToAsync("..");

        }

        [RelayCommand]
        private async Task AddRecord()
        {
            var query = new Dictionary<string, object>
            {
                { "mode", "add" },
                { "id", 0 },
                { "cropFieldId", SelectedCropField.Id }
            };
            await Shell.Current.GoToAsync(nameof(LedgerRecordPage), query);
        }

        [RelayCommand]
        private static async Task EditRecord(BalanceLedger record)
        {
            var query = new Dictionary<string, object>
            {
                { "mode", "edit" },
                { "id", record.Id },
                { "cropFieldId", 0 }
            };
            await Shell.Current.GoToAsync(nameof(LedgerRecordPage), query);
        }

        [RelayCommand]
        private void DeleteRecord(BalanceLedger record)
        {
            try
            { 
                using var context = new DatabaseContext();
                context.BalanceLedgers.Remove(record);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
            finally
            {
                QueryLedgerEntries(this, null);
            }
        }

        [RelayCommand]
        private async Task GenerateReport()
        {
            var query = new Dictionary<string, object>()
            {
                { "entries", LedgerEntries },
                { "cropfield", SelectedCropField },
                { "season", Season.GetCurrentSeason() }
            };
            await Shell.Current.GoToAsync(nameof(ReportPage), query);
        }

        [RelayCommand]
        private void FilterAndSortRecords()
        {
            FilterPopupButtonEnabled = false;
            _popUpSvc.PushAsync(new LedgerFilterPopup(new LedgerFilterPopupViewModel(_filterSet, _popUpSvc)));
        }

        private void ApplyFilters(object sender, LedgerFilterSet newSet)
        {
            _filterSet = newSet;
            QueryLedgerEntries(this, null);
            FilterPopupButtonEnabled = true;
        }

        public void QueryLedgerEntries(object sender, EventArgs e)
        {
            try
            {
                using var context = new DatabaseContext();
                IEnumerable<BalanceLedger> query =
                    from entry in context.BalanceLedgers.Include(entry => entry.IdCostTypeNavigation)
                                                        .Include(entry => entry.IdSeasonNavigation)
                    where entry.IdCropField == SelectedCropField.Id
                    && _filterSet.SelectedCostTypeIds.Contains(entry.IdCostType)
                    && _filterSet.EarliestDate <= entry.DateAdded 
                    && entry.DateAdded <= _filterSet.LatestDate
                    && _filterSet.SelectedSeasonIds.Contains(entry.IdSeason)
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
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        public void EnableFilterPopupButton(object sender, EventArgs e) =>
            FilterPopupButtonEnabled = true;

        partial void OnSelectedCropFieldChanged(CropField value) => 
            QueryLedgerEntries(this, null);
    }
}

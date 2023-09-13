using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.PopUps;
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

        private LedgerFilterSet _filterSet;
        private readonly IPopupNavigation _popUpSvc;

        public LedgerPageViewModel(IPopupNavigation popupNavigation)
        {
            _popUpSvc = popupNavigation;
            _filterSet = new();
            //{
            //    TODO: load default filters from Preferences
            //};
            try
            {
                CostType.Validate();
                using var context = new DatabaseContext();
                CostTypes = context.CostTypes.ToList();
                CropFields = context.CropFields.ToList();
                SelectedCropField = CropFields.First();
                LedgerEntries = context.BalanceLedgers
                    .Where(entry => entry.IdCropField == SelectedCropField.Id)
                    .ToList();
                LedgerEntries.Reverse();
                //TODO: filters
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
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
                QueryLedgerEntries();
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
        private void FilterAndSortRecords() => 
            _popUpSvc.PushAsync(new LedgerFilterPopup(new LedgerFilterPopupViewModel(_filterSet, _popUpSvc)));

        private void ApplyFilters(FilterSetEventArgs args)
        {
            _filterSet = args.FilterSet;
            QueryLedgerEntries();
        }

        public void QueryLedgerEntries()
        {
            using var context = new DatabaseContext();
            try
            {
                //TODO: filters
                LedgerEntries = context.BalanceLedgers
                    .Include(entry => entry.IdCostTypeNavigation)
                    .Where(entry => entry.IdCropField == SelectedCropField.Id)
                    .ToList();
                LedgerEntries.Reverse();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        partial void OnSelectedCropFieldChanged(CropField value)
            => QueryLedgerEntries();
    }
}

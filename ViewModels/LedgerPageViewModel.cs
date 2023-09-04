using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.Views;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FarmOrganizer.ViewModels
{
    public partial class LedgerPageViewModel : ObservableObject
    {
        [ObservableProperty]
        List<BalanceLedger> ledgerEntries = new();
        [ObservableProperty]
        List<CostType> costTypes = new();
        [ObservableProperty]
        List<CropField> cropFields = new();

        [ObservableProperty]
        CropField selectedCropField;

        public LedgerPageViewModel()
        {
            using var context = new DatabaseContext();
            try
            {
                CostTypes = context.CostTypes.ToList();
                CropFields = context.CropFields.ToList();
                SelectedCropField = CropFields.First();
                LedgerEntries = context.BalanceLedgers
                    .Where(entry => entry.IdCropField == SelectedCropField.Id)
                    .ToList();
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
            await Shell.Current.GoToAsync($"{nameof(LedgerRecordPage)}", query);
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
            await Shell.Current.GoToAsync($"{nameof(LedgerRecordPage)}", query);
        }

        [RelayCommand]
        private static void SortRecords()
        {

        }

        [RelayCommand]
        private static void FilterRecords()
        {

        }

        [RelayCommand]
        private async Task GenerateReport()
        {
            var query = new Dictionary<string, object>()
            {
                { "entries", FilteredLedgerEntries },
                { "cropfield", SelectedCropField },
                { "season", SeasonsPageViewModel.GetCurrentSeason() }
            };
            await Shell.Current.GoToAsync(nameof(ReportPage), query);
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

        public void QueryLedgerEntries()
        {
            using var context = new DatabaseContext();
            try
            {
                LedgerEntries = context.BalanceLedgers
                    .Include(entry => entry.IdCostTypeNavigation)
                    .Where(entry => entry.IdCropField == SelectedCropField.Id)
                    .ToList();
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

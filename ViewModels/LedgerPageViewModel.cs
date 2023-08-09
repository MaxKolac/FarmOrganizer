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
                CostTypes = context.CostType.ToList();
                CropFields = context.CropField.ToList();
                SelectedCropField = CropFields.First();
                LedgerEntries = context.BalanceLedger
                    .Where(entry => entry.IdCropField == SelectedCropField.Id)
                    .ToList();
            }
            catch (SqliteException ex)
            {
                App.AlertSvc.ShowAlert(ErrorMessages.GenericTitle, ErrorMessages.GenericMessage(ex));
                Application.Current.MainPage.Dispatcher.Dispatch(async () =>
                {
                    await Shell.Current.GoToAsync("..");
                });
            }
        }

        [RelayCommand]
        private static async Task AddRecord(BalanceLedger record) => 
            await Shell.Current.GoToAsync($"{nameof(LedgerRecordPage)}?mode=add&id=0&cropFieldId=" +
                $"{record.IdCropField}");
        //TODO: record object is obviously empty, because this isnt a CollectionItem, its a Toolbar... bruh
        //VM needs to have a property which keeps track of which CropField has been selected in the Picker
        //Also it needs to be set to the first field or smthing like that, so theres no need to select the same field
        //everytime the ledgerPage is opened that'd be annoying

        [RelayCommand]
        private static async Task EditRecord(BalanceLedger record) =>
            await Shell.Current.GoToAsync($"{nameof(LedgerRecordPage)}?mode=edit&id={record.Id}&cropFieldId=0");

        [RelayCommand]
        private void SortRecords()
        {

        }

        [RelayCommand]
        private void FilterRecords()
        {

        }

        [RelayCommand]
        private void GenerateAndCalculate()
        {

        }


        [RelayCommand]
        private void DeleteRecord(BalanceLedger record) 
        {
            try
            { 
                using var context = new DatabaseContext();
                context.BalanceLedger.Remove(record);
                context.SaveChanges();
            }
            catch (SqliteException ex)
            {
                App.AlertSvc.ShowAlert(ErrorMessages.GenericTitle, ErrorMessages.GenericMessage(ex));
            }
            finally
            {
                QueryLedgerEntries();
            }
        }

        private void QueryLedgerEntries()
        {
            using var context = new DatabaseContext();
            try
            {
                LedgerEntries = context.BalanceLedger
                    .Include(entry => entry.IdCostTypeNavigation)
                    .Where(entry => entry.IdCropField == SelectedCropField.Id)
                    .ToList();
            }
            catch (SqliteException ex)
            {
                App.AlertSvc.ShowAlert(ErrorMessages.GenericTitle, ErrorMessages.GenericMessage(ex));
                Application.Current.MainPage.Dispatcher.Dispatch(async () =>
                {
                    await Shell.Current.GoToAsync("..");
                });
            }
        }

        partial void OnSelectedCropFieldChanged(CropField value)
            => QueryLedgerEntries();
    }
}

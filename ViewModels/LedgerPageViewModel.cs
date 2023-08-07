using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Models;
using FarmOrganizer.Views;
using Microsoft.Data.Sqlite;

namespace FarmOrganizer.ViewModels
{
    //[QueryProperty(nameof(PassedQuery), "query")]
    public partial class LedgerPageViewModel : ObservableObject
    {
        //[ObservableProperty]
        //string passedQuery;
        [ObservableProperty]
        List<BalanceLedger> ledgerList = new();

        public LedgerPageViewModel()
        {
            using var context = new DatabaseContext();
            try
            {
                ledgerList = context.BalanceLedger.ToList();
            } 
            catch (SqliteException ex)
            {
                App.AlertSvc.ShowAlert(
                    "Coś poszło nie tak",
                    $"Program zwrócił błąd związany z bazą danych. Nie udało się załadować rekordów z tabeli BalanceLedger. Kod błędu SQLite: {ex.SqliteErrorCode}, {ex.SqliteExtendedErrorCode}.");
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
        private void GenerateAndCalculate()
        {

        }

        [RelayCommand]
        private void EditRecord()
        {

        }

        [RelayCommand]
        private void DeleteRecord() { }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Models;
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
            }
        }

        [RelayCommand]
        private void AddRecord()
        {

        }

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

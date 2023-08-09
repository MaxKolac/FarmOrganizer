using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FarmOrganizer.ViewModels
{
    [QueryProperty(nameof(PageMode), "mode")]
    [QueryProperty(nameof(RecordId), "id")]
    [QueryProperty(nameof(QuerriedCropFieldId), "cropFieldId")]
    public partial class LedgerRecordPageViewModel : ObservableObject, IQueryAttributable
    {
        #region Queryable Properties
        /// <summary>
        /// The mode in which this page has been opened. It could be used for either adding a new record, or editing an existing one. Use one of those modes by passing a string <c>add</c> or <c>edit</c>, all lowercase.
        /// </summary>
        [ObservableProperty]
        string pageMode;
        /// <summary>
        /// If the page was openned in <c>edit</c> mode, this is the unique ID of the record to edit. Otherwise, this property is ignored.
        /// </summary>
        [ObservableProperty]
        int recordId;
        /// <summary>
        /// If the page is opened in <c>add</c> mode, this is a value that is automatically assigned to the new record's <c>IdCropField</c> property. In <c>edit</c> mode, it is ignored.
        /// </summary>
        [ObservableProperty]
        int querriedCropFieldId;
        #endregion

        #region Record Properties
        [ObservableProperty]
        int idCostType;
        [ObservableProperty]
        int idCropField;
        [ObservableProperty]
        DateTime dateAdded;
        [ObservableProperty]
        double balanceChange;
        [ObservableProperty]
        string notes;
        #endregion

        #region Mode-specific Strings
        [ObservableProperty]
        string titleText;
        [ObservableProperty]
        string saveButtonText;
        #endregion

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            PageMode = query["mode"] as string;
            RecordId = (int)query["id"];
            QuerriedCropFieldId = (int)query["cropFieldId"];
            switch (PageMode)
            {
                case "add":
                    TitleText = "Dodawanie nowego wpisu";
                    SaveButtonText = "Dodaj i zapisz";
                    IdCostType = 0;
                    IdCropField = QuerriedCropFieldId;
                    DateAdded = DateTime.Now;
                    BalanceChange = 0;
                    Notes = string.Empty;
                    break;
                case "edit":
                    TitleText = "Edytowanie wpisu";
                    SaveButtonText = "Zapisz zmiany";
                    try
                    {
                        using var context = new DatabaseContext();
                        BalanceLedger result = context.BalanceLedger.Find(RecordId);
                        IdCostType = result.IdCostType;
                        IdCropField = result.IdCropField;
                        DateAdded = result.DateAdded;
                        BalanceChange = result.BalanceChange;
                        Notes = result.Notes;
                    }
                    catch (NullReferenceException)
                    {
                        App.AlertSvc.ShowAlert(
                            ErrorMessages.GenericTitle,
                            ErrorMessages.RecordNotFound(nameof(BalanceLedger), RecordId));
                        ReturnToPreviousPage();
                    }
                    catch (SqliteException ex)
                    {
                        App.AlertSvc.ShowAlert(
                            ErrorMessages.GenericTitle,
                            ErrorMessages.GenericSqliteMessage(ex));
                        ReturnToPreviousPage();
                    }
                    break;
                default:
                    App.AlertSvc.ShowAlert(
                        ErrorMessages.GenericTitle,
                        ErrorMessages.InvalidPropertyQueries(nameof(PageMode), PageMode));
                    ReturnToPreviousPage();
                    break;
            }
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                using var context = new DatabaseContext();
                switch (PageMode)
                {
                    case "add":
                        BalanceLedger newRecord = new()
                        {
                            IdCostType = this.IdCostType,
                            IdCropField = this.IdCropField,
                            DateAdded = this.DateAdded,
                            BalanceChange = this.BalanceChange,
                            Notes = this.Notes
                        };
                        context.BalanceLedger.Add(newRecord);
                        context.SaveChanges();
                        ReturnToPreviousPage();
                        break;
                    case "edit":
                        BalanceLedger existingRecord = context.BalanceLedger.Find(RecordId);
                        existingRecord.IdCostType = IdCostType;
                        existingRecord.IdCropField = IdCropField;
                        existingRecord.DateAdded = DateAdded;
                        existingRecord.BalanceChange = BalanceChange;
                        existingRecord.Notes = Notes;
                        context.SaveChanges();
                        ReturnToPreviousPage();
                        break;
                }
            }
            catch (DbUpdateException ex)
            {
                //Thrown when:
                //Constraint is failed - Base 19
                App.AlertSvc.ShowAlert(
                    ErrorMessages.GenericTitle,
                    ErrorMessages.GenericSqliteMessage((SqliteException)ex.InnerException)
                    );
            }
            catch (SqliteException ex)
            {
                App.AlertSvc.ShowAlert(
                    ErrorMessages.GenericTitle,
                    ErrorMessages.GenericSqliteMessage(ex));
            }
            catch (NullReferenceException)
            {
                App.AlertSvc.ShowAlert(
                    ErrorMessages.GenericTitle,
                    ErrorMessages.RecordNotFound(nameof(BalanceLedger), RecordId));
            }
        }

        [RelayCommand]
        private static void ReturnToPreviousPage() =>
            Application.Current.MainPage.Dispatcher.Dispatch(
                async () => { await Shell.Current.GoToAsync(".."); }
            );
    }
}

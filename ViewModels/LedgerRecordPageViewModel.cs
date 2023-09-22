using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.HelperClasses;
using Microsoft.Data.Sqlite;

//Passing query parameters through a single method and a Dictionary - https://learn.microsoft.com/pl-pl/dotnet/maui/fundamentals/shell/navigation#process-navigation-data-using-a-single-method

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
        private string pageMode;
        /// <summary>
        /// If the page was openned in <c>edit</c> mode, this is the unique ID of the record to edit. Otherwise, this property is ignored.
        /// </summary>
        [ObservableProperty]
        private int recordId;
        /// <summary>
        /// If the page is opened in <c>add</c> mode, this is a value that is automatically assigned to the new record's <c>IdCropField</c> property. In <c>edit</c> mode, it is ignored.
        /// </summary>
        [ObservableProperty]
        private int querriedCropFieldId;
        #endregion

        #region Record Properties
        [ObservableProperty]
        private DateTime dateAdded;
        private DateTime dateAddedCorrected;
        [ObservableProperty]
        private string balanceChange;
        [ObservableProperty]
        private string notes;
        #endregion

        #region Mode-specific Strings
        [ObservableProperty]
        private string titleText;
        [ObservableProperty]
        private string saveButtonText;
        #endregion

        #region Picker ItemSources
        [ObservableProperty]
        List<CropField> cropFields = new();
        [ObservableProperty]
        CropField selectedCropField;

        [ObservableProperty]
        List<CostType> costTypes = new();
        [ObservableProperty]
        CostType selectedCostType;

        [ObservableProperty]
        List<Season> seasons = new();
        [ObservableProperty]
        Season selectedSeason;
        #endregion

        public static event EventHandler OnPageQuit;

        public LedgerRecordPageViewModel()
        {
            try
            {
                CostType.Validate(out List<CostType> allCostTypes);
                CostTypes.AddRange(allCostTypes.OrderBy(cost => cost.Name).ToList());

                CropField.Validate(out var allCropFields);
                CropFields.AddRange(allCropFields);

                Season.Validate(out var allSeasons);
                Seasons.AddRange(allSeasons);

                SelectedSeason = Seasons.Find(season => season.Id == Season.GetCurrentSeason().Id);
            }
            catch (TableValidationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

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
                    SelectedCostType = CostTypes.First();
                    SelectedCropField = CropFields.Find(field => field.Id == QuerriedCropFieldId);
                    //SelectedSeason
                    dateAddedCorrected = DateTime.Now;
                    DateAdded = DateTime.Now;
                    BalanceChange = "0";
                    Notes = string.Empty;
                    break;
                case "edit":
                    TitleText = "Edytowanie wpisu";
                    SaveButtonText = "Zapisz zmiany";
                    using (var context = new DatabaseContext())
                    {
                        BalanceLedger result = context.BalanceLedgers.Find(RecordId);
                        SelectedCostType = CostTypes.Find(type => type.Id == result.IdCostType);
                        SelectedCropField = CropFields.Find(field => field.Id == result.IdCropField);
                        SelectedSeason = Seasons.Find(season => season.Id == result.IdSeason);
                        dateAddedCorrected = result.DateAdded;
                        DateAdded = result.DateAdded;
                        BalanceChange = result.BalanceChange.ToString();
                        Notes = result.Notes;
                    }
                    break;
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                using var context = new DatabaseContext();
                switch (PageMode)
                {
                    case "add":
                        var dateTime = new DateTime(
                            dateAddedCorrected.Year,
                            dateAddedCorrected.Month,
                            dateAddedCorrected.Day,
                            DateTime.Now.Hour,
                            DateTime.Now.Minute,
                            DateTime.Now.Second
                            );
                        BalanceLedger newRecord = new()
                        {
                            IdCostType = SelectedCostType.Id,
                            IdCropField = SelectedCropField.Id,
                            IdSeason = SelectedSeason.Id,
                            DateAdded = dateTime,
                            BalanceChange =
                            Math.Abs(
                                Math.Round(
                                    Utils.CastToValue(this.BalanceChange.ToString()), 2)),
                            Notes = this.Notes
                        };
                        context.BalanceLedgers.Add(newRecord);
                        context.SaveChanges();
                        await ReturnToPreviousPage();
                        break;
                    case "edit":
                        BalanceLedger existingRecord = context.BalanceLedgers.Find(RecordId);
                        existingRecord.IdCostType = SelectedCostType.Id;
                        existingRecord.IdCropField = SelectedCropField.Id;
                        existingRecord.IdSeason = SelectedSeason.Id;
                        existingRecord.DateAdded = dateAddedCorrected;
                        existingRecord.BalanceChange =
                            Math.Abs(
                                Math.Round(
                                    Utils.CastToValue(this.BalanceChange.ToString()), 2));
                        existingRecord.Notes = Notes;
                        context.SaveChanges();
                        await ReturnToPreviousPage();
                        break;
                }
            }
            catch (SqliteException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
        }

        [RelayCommand]
        private async Task ReturnToPreviousPage()
        {
            OnPageQuit?.Invoke(this, null);
            await Shell.Current.GoToAsync("..");
        }

        partial void OnDateAddedChanged(DateTime oldValue, DateTime newValue)
        {
            dateAddedCorrected = new DateTime(
                newValue.Year,
                newValue.Month,
                newValue.Day,
                dateAddedCorrected.Hour,
                dateAddedCorrected.Minute,
                dateAddedCorrected.Second,
                dateAddedCorrected.Millisecond,
                dateAddedCorrected.Microsecond
                );
        }
    }
}

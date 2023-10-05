using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.HelperClasses;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

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
        private List<CropField> cropFields = new();
        [ObservableProperty]
        private CropField selectedCropField;

        [ObservableProperty]
        private List<Season> seasons = new();
        [ObservableProperty]
        private Season selectedSeason;
        #endregion

        #region Cost Type Variables
        [ObservableProperty]
        private ObservableCollection<CostType> currentCostTypes = new();
        [ObservableProperty]
        private bool costIsExpense;
        [ObservableProperty]
        private string costTypeLabel = _costTypeLabelExpense;
        [ObservableProperty]
        private CostType selectedCostType;
        private const string _costTypeLabelExpense = "Wydatek";
        private const string _costTypeLabelProfit = "Przychód";
        private readonly List<CostTypeGroup> costTypeGroups = new();
        #endregion

        #region Balance Change Variables
        [ObservableProperty]
        private string balanceChangeLabel = _balanceLabelExpense;
        private const string _balanceLabelExpense = "Kwota wydatku w złotych";
        private const string _balanceLabelProfit = "Kwota przychodu w złotych";
        #endregion

        public static event EventHandler OnPageQuit;

        public LedgerRecordPageViewModel()
        {
            try
            {
                using var context = new DatabaseContext();
                CostType.Validate(context);
                costTypeGroups = CostType.BuildCostTypeGroups(context).ToList();
                //This triggers the partial method required to repopulate CurrentCostTypes
                CostIsExpense = true;

                CropFields = CropField.RetrieveAll(context);
                Seasons = Season.RetrieveAll(context);
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
                    SelectedCostType = CurrentCostTypes.First();
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
                        BalanceLedger result = context.BalanceLedgers.Include(entry => entry.IdCostTypeNavigation).ToList().Find(entry => entry.Id == RecordId);
                        //This triggers the partial method required to repopulate CurrentCostTypes
                        CostIsExpense = result.IdCostTypeNavigation.IsExpense;
                        SelectedCostType = CurrentCostTypes.ToList().Find(type => type.Id == result.IdCostType);
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
                BalanceLedger entry = new()
                {
                    IdCostType = SelectedCostType.Id,
                    IdCropField = SelectedCropField.Id,
                    IdSeason = SelectedSeason.Id,
                    BalanceChange = Utils.CastToValue(BalanceChange),
                    DateAdded = DateAdded,
                    Notes = Notes
                };
                switch (PageMode)
                {
                    case "add":
                        BalanceLedger.AddEntry(entry, null);
                        await ReturnToPreviousPage();
                        break;
                    case "edit":
                        entry.Id = RecordId;
                        BalanceLedger.EditEntry(entry, null);
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

        partial void OnCostIsExpenseChanged(bool value)
        {
            CostTypeLabel = value ? _costTypeLabelExpense : _costTypeLabelProfit;
            BalanceChangeLabel = value ? _balanceLabelExpense : _balanceLabelProfit;
            CurrentCostTypes.Clear();
            foreach (var cost in costTypeGroups[value ? 1 : 0])
                CurrentCostTypes.Add(cost);
            SelectedCostType = CurrentCostTypes.First();
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

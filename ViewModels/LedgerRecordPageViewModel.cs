﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.HelperClasses;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
        DateTime dateAdded;
        [ObservableProperty]
        string balanceChange;
        [ObservableProperty]
        string notes;
        #endregion

        #region Mode-specific Strings
        [ObservableProperty]
        string titleText;
        [ObservableProperty]
        string saveButtonText;
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
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
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
                    DateAdded = DateTime.Now;
                    BalanceChange = "0";
                    Notes = string.Empty;
                    break;
                case "edit":
                    TitleText = "Edytowanie wpisu";
                    SaveButtonText = "Zapisz zmiany";
                    try
                    {
                        using var context = new DatabaseContext();
                        BalanceLedger result = context.BalanceLedgers.Find(RecordId);
                        SelectedCostType = CostTypes.Find(type => type.Id == result.IdCostType);
                        SelectedCropField = CropFields.Find(field => field.Id == result.IdCropField);
                        SelectedSeason = Seasons.Find(season => season.Id == result.IdSeason);
                        DateAdded = result.DateAdded;
                        BalanceChange = result.BalanceChange.ToString();
                        Notes = result.Notes;
                    }
                    catch (Exception ex)
                    {
                        new ExceptionHandler(ex).ShowAlert();
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
                        BalanceLedger newRecord = new()
                        {
                            IdCostType = SelectedCostType.Id,
                            IdCropField = SelectedCropField.Id,
                            IdSeason = SelectedSeason.Id,
                            DateAdded = this.DateAdded,
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
                        existingRecord.DateAdded = DateAdded;
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
            catch (DbUpdateException ex)
            {
                //Thrown when:
                //Constraint is failed - basic code 19
                new ExceptionHandler((SqliteException)ex.InnerException).ShowAlert();
            }
            catch (NullReferenceException)
            {
                NoRecordFoundException ex = new(
                    nameof(DatabaseContext.BalanceLedgers),
                    $"If = {RecordId}"
                    );
                new ExceptionHandler(ex).ShowAlert();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        [RelayCommand]
        private async Task ReturnToPreviousPage()
        {
            OnPageQuit?.Invoke(this, null);
            await Shell.Current.GoToAsync("..");
        }
    }
}

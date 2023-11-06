using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using Microsoft.Data.Sqlite;

namespace FarmOrganizer.ViewModels
{
    public partial class SeasonsPageViewModel : ObservableObject
    {
        readonly IPopupService popupService;

        [ObservableProperty]
        List<Season> seasons = new();
        [ObservableProperty]
        bool showCreatorFrame = false;
        [ObservableProperty]
        bool dateEndPickerEnabled = false;
        [ObservableProperty]
        string saveButtonText = "Dodaj sezon i zapisz";

        bool addingSeason = false;
        bool editingSeason = false;
        int editedSeasonId;

        #region Season Details
        [ObservableProperty]
        string seasonName = "Nowy sezon " + DateTime.Now.AddMonths(1).Year.ToString();
        [ObservableProperty]
        DateTime seasonDateStart = DateTime.Now;
        [ObservableProperty]
        DateTime seasonDateEnd = DateTime.Now.AddYears(1);
        #endregion

        public SeasonsPageViewModel(IPopupService popupService)
        {
            this.popupService = popupService;
            try
            {
                Seasons = Season.RetrieveAll(null);
            }
            catch (TableValidationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        [RelayCommand]
        private void AddOrSave()
        {
            try
            {
                var season = new Season()
                {
                    Name = SeasonName,
                    DateStart = SeasonDateStart
                };
                if (addingSeason)
                {
                    season.DateEnd = Season.MaximumDate;
                    Season.AddEntry(season, null);
                }
                else if (editingSeason)
                {
                    season.Id = editedSeasonId;
                    season.DateEnd = SeasonDateEnd;
                    Season.EditEntry(season, null);
                }

                Seasons = new DatabaseContext().Seasons.ToList();
                ToggleAdding();
            }
            catch (InvalidRecordPropertyException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
            catch (NoRecordFoundException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
            catch (SqliteException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
        }

        [RelayCommand]
        private void Edit(Season seasonToEdit)
        {
            DateEndPickerEnabled = seasonToEdit.DateEnd < Season.MaximumDate;
            editedSeasonId = seasonToEdit.Id;
            SeasonName = seasonToEdit.Name;
            SeasonDateStart = seasonToEdit.DateStart;
            SeasonDateEnd = seasonToEdit.DateEnd;
            editingSeason = true;
            addingSeason = false;
            SaveButtonText = "Zapisz zmiany";
            ShowCreatorFrame = true;
        }

        [RelayCommand]
        private async Task Remove(Season seasonToRemove)
        {
            try
            {
                if (!await App.AlertSvc.ShowConfirmationAsync(
                    "Uwaga!",
                    "Usunięcie sezonu usunie również WSZYSTKIE wpisy z kosztami, które były podpięte pod usuwany sezon. Tej operacji nie można cofnąć. Czy chcesz kontynuować?",
                    "Tak, usuń",
                    "Anuluj"))
                    return;
                Season.DeleteEntry(seasonToRemove.Id, null);
                Seasons = new DatabaseContext().Seasons.ToList();
            }
            catch (RecordDeletionException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
            finally
            {
                ToggleAdding();
                ShowCreatorFrame = false;
            }
        }

        [RelayCommand]
        private void ToggleAdding()
        {
            editingSeason = false;
            addingSeason = true;
            SeasonDateEnd = Season.MaximumDate;
            DateEndPickerEnabled = false;
            SaveButtonText = "Dodaj sezon i zapisz";
            ShowCreatorFrame = !ShowCreatorFrame;
        }
    }
}

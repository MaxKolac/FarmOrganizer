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
        [ObservableProperty]
        private List<Season> seasons = new();
        [ObservableProperty]
        private bool showCreatorFrame = false;
        [ObservableProperty]
        private bool dateEndPickerEnabled = false;
        [ObservableProperty]
        private string saveButtonText = "Dodaj sezon i zapisz";

        private bool addingSeason = false;
        private bool editingSeason = false;
        private int editedSeasonId;

        #region Season Details
        [ObservableProperty]
        private string seasonName = "Nowy sezon " + DateTime.Now.AddMonths(1).Year.ToString();
        [ObservableProperty]
        private DateTime seasonDateStart = DateTime.Now;
        [ObservableProperty]
        private DateTime seasonDateEnd = DateTime.Now.AddYears(1);
        #endregion

        public SeasonsPageViewModel()
        {
            try
            {
                Seasons = Season.ValidateRetrieve();
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
                if (addingSeason)
                {
                    Season newSeason = new()
                    {
                        Name = SeasonName,
                        DateStart = SeasonDateStart,
                        DateEnd = Season.MaximumDate
                    };
                    Season.AddEntry(newSeason);
                }
                else if (editingSeason)
                {
                    Season seasonToEdit = new()
                    {
                        Id = editedSeasonId,
                        Name = SeasonName,
                        DateStart = SeasonDateStart,
                        DateEnd = SeasonDateEnd
                    };
                    Season.EditEntry(seasonToEdit);
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
                Season.DeleteEntry(seasonToRemove);
                Seasons = new DatabaseContext().Seasons.ToList();
            }
            catch (RecordDeletionException ex)
            {
                ExceptionHandler.Handle(ex, false);
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

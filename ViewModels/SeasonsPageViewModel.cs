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
        private bool addingNewSeason = false;

        #region New Season Details
        [ObservableProperty]
        private string newSeasonName;
        [ObservableProperty]
        private DateTime newSeasonDateStart;
        #endregion

        public SeasonsPageViewModel()
        {
            try
            {
                Season.Validate(out List<Season> allEntries);
                Seasons.AddRange(allEntries);
                NewSeasonName = "Nowy sezon " + DateTime.Now.Year.ToString();
                NewSeasonDateStart = DateTime.Now;
            }
            catch (TableValidationException ex)
            {
                ExceptionHandler.Handle(ex, true);
            }
        }

        [RelayCommand]
        private void StartNewSeason()
        {
            try
            {
                Season newSeason = new()
                {
                    Name = NewSeasonName,
                    DateStart = NewSeasonDateStart,
                    DateEnd = DateTime.MaxValue,
                    HasConcluded = false
                };
                Season.AddEntry(newSeason);
                Seasons = new DatabaseContext().Seasons.ToList();
                ToggleNewSeasonFrame();
            }
            catch (InvalidRecordPropertyException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
        }

        [RelayCommand]
        private static void ResumePreviousSeason()
        {

        }

        [RelayCommand]
        private void ToggleNewSeasonFrame() =>
            AddingNewSeason = !AddingNewSeason;
    }
}

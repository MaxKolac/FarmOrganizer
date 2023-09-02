using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace FarmOrganizer.ViewModels
{
    public partial class SeasonsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        List<Season> seasons;

        [ObservableProperty]
        bool addingNewSeason = false;

        [ObservableProperty]
        string editedSeasonName;
        [ObservableProperty]
        DateTime editedSeasonStartDate;
        [ObservableProperty]
        DateTime editedSeasonEndDate;
        [ObservableProperty]
        bool editedSeasonHasEnded;

        public SeasonsPageViewModel()
        {
            using var context = new DatabaseContext();
            try
            {
                Seasons = context.Seasons.ToList();
            }
            catch (SqliteException ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        [RelayCommand]
        private void StartNewSeason()
        {
                //New season cannot start before the previous one started
                if (seasonToEnd.DateStart >= newSeason.DateStart)
                    throw new InvalidRecordException("Data rozpoczęcia", newSeason.DateStart.ToShortDateString());

        }

        [RelayCommand]
        private void ResumePreviousSeason()
        {

        }

        [RelayCommand]
        private void ToggleNewSeasonFrame() =>
            AddingNewSeason = !AddingNewSeason;
    }

    internal class HasConcludedConverter : IValueConverter
    {
        private readonly string _done = "Zakończony";
        private readonly string _inProgress = "W trakcie";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
                return "ERROR";
            bool hasConcluded = (bool)value;
            return hasConcluded ? _done : _inProgress;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string)
                return false;
            string text = (string)value;
            return text.Equals(_done);
        }
    }
}

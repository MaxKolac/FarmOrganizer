using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using System.Globalization;

namespace FarmOrganizer.ViewModels
{
    public partial class SeasonsPageViewModel : ObservableObject
    {
        private Season currentlyOpenSeason;

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
                CheckForConsistency(out List<Season> allSeasons, out currentlyOpenSeason);
                Seasons.AddRange(allSeasons);
                NewSeasonName = "Nowy sezon " + DateTime.Now.Year.ToString();
                NewSeasonDateStart = DateTime.Now;
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        [RelayCommand]
        private void StartNewSeason()
        {
            using var context = new DatabaseContext();
            try
            {
                Season seasonToEnd = context.Seasons.Find(currentlyOpenSeason.Id);
                Season newSeason = new()
                {
                    Name = NewSeasonName,
                    DateStart = NewSeasonDateStart,
                    DateEnd = DateTime.MaxValue,
                    HasConcluded = false
                };

                //New season cannot start before the previous one started
                if (seasonToEnd.DateStart >= newSeason.DateStart)
                    throw new InvalidRecordException("Data rozpoczęcia", newSeason.DateStart.ToShortDateString());

                seasonToEnd.HasConcluded = true;
                seasonToEnd.DateEnd = NewSeasonDateStart;
                context.Seasons.Add(newSeason);
                currentlyOpenSeason = newSeason;
                context.SaveChanges();
                Seasons = context.Seasons.ToList();
                ToggleNewSeasonFrame();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        [RelayCommand]
        private static void ResumePreviousSeason()
        {

        }

        [RelayCommand]
        private void ToggleNewSeasonFrame() =>
            AddingNewSeason = !AddingNewSeason;

        /// <summary>
        /// Retrieves all Season records from the database and validates their consistency.
        /// <para>
        /// An exception will be thrown if:
        /// <list type="bullet">
        /// <item>No entries in the table exist</item>
        /// <item>There exists one Season, but it has concluded</item>
        /// <item>There exist more than 1 or no open Seasons in the table</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <exception cref="NoRecordFoundException"></exception>
        /// <exception cref="InvalidRecordException"></exception>
        private static void CheckForConsistency(out List<Season> allSeasons, out Season currentSeason)
        {
            using var context = new DatabaseContext();
            allSeasons = context.Seasons.ToList();
            //Check if there is at least 1 season; it needs to be open.
            if (allSeasons.Count == 0)
                throw new NoRecordFoundException(nameof(DatabaseContext.Seasons), "*");
            //Check if the only season is concluded
            if (allSeasons.Count == 1 && allSeasons[0].HasConcluded)
                throw new InvalidRecordException("W tabeli nie może znajdować się tylko jeden sezon, który jest zamknięty.", allSeasons[0].ToString());

            List<Season> openSeasons = allSeasons.FindAll(e => !e.HasConcluded);
            //Check if there are any open seasons
            if (openSeasons.Count == 0)
                throw new NoRecordFoundException(nameof(DatabaseContext.Seasons), "W tabeli nie odnaleziono żadnych otwartych sezonów.");
            //Check if there's more than 1 open season.
            if (openSeasons.Count > 1)
                throw new InvalidRecordException("W tabeli sezonów odnaleziono więcej niż jeden otwarty sezon.", openSeasons.ToString());

            currentSeason = openSeasons[0];
        }

        public static Season GetCurrentSeason()
        {
            CheckForConsistency(out _, out Season season);
            return season;
        }

        /// <summary>
        /// Attempts to start a new Season and end the currently open season.
        /// </summary>
        /// <param name="newSeasonName">The name of the new Season.</param>
        /// <param name="startDate">The start date to assign to the new Season.</param>
        /// <exception cref="InvalidRecordException"></exception>
        public static void StartNewSeason(string newSeasonName, DateTime startDate)
        {
            using var context = new DatabaseContext();
            Season seasonToEnd = context.Seasons.Find(GetCurrentSeason().Id);
            Season seasonToAdd = new()
            {
                Name = newSeasonName,
                DateStart = startDate,
                DateEnd = DateTime.MaxValue,
                HasConcluded = false
            };

            //New season cannot start before the previous one started
            if (seasonToEnd.DateStart >= seasonToAdd.DateStart)
                throw new InvalidRecordException("Data rozpoczęcia", seasonToAdd.DateStart.ToShortDateString());

            seasonToEnd.HasConcluded = true;
            seasonToEnd.DateEnd = seasonToAdd.DateStart;
            context.Seasons.Add(seasonToAdd);
            context.SaveChanges();
        }
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

    internal class DateEndConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime)
                return string.Empty;
            DateTime date = (DateTime)value;
            if (date > new DateTime(9999, 1, 1))
                return string.Empty;
            else
                return " - " + date.ToString("d MMMM yy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

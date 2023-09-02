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
        List<Season> seasons;
        [ObservableProperty]
        bool addingNewSeason = false;

        #region New Season Details
        [ObservableProperty]
        string newSeasonName;
        [ObservableProperty]
        DateTime newSeasonDateStart;
        #endregion

        public SeasonsPageViewModel()
        {
            using var context = new DatabaseContext();
            try
            {
                Seasons = context.Seasons.ToList();
                //Check if there is at least 1 season; it needs to be open.
                if (Seasons.Count == 0)
                    throw new NoRecordFoundException(nameof(DatabaseContext.Seasons), "*");
                //Check if the only season is concluded
                if (Seasons.Count == 1 && Seasons[0].HasConcluded)
                    throw new Exception("W tabeli nie może znajdować się tylko jeden sezon, który jest zamknięty.");
                //Check if there's more than 1 open season.
                if (Seasons.FindAll(e => !e.HasConcluded).Count > 1)
                    throw new Exception("W tabeli sezonów odnaleziono więcej niż jeden otwarty sezon.");

                currentlyOpenSeason = Seasons.Find(e => !e.HasConcluded);
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

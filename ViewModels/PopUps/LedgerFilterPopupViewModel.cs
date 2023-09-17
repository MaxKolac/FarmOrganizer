using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.Converters;
using FarmOrganizer.ViewModels.HelperClasses;
using Mopups.Interfaces;

namespace FarmOrganizer.ViewModels.PopUps
{
    public partial class LedgerFilterPopupViewModel : ObservableObject
    {
        public static event EventHandler<LedgerFilterSet> OnFilterSetCreated;
        public static event EventHandler OnPageQuit;

        private readonly LedgerFilterSet _filterSet;
        private readonly IPopupNavigation _popUpSvc;

        #region Collections and Choices Bindings
        [ObservableProperty]
        private List<CostType> allCostTypes = new();
        [ObservableProperty]
        private List<object> selectedCostTypes = new();

        [ObservableProperty]
        private List<Season> allSeasons = new();
        [ObservableProperty]
        private List<object> selectedSeasons = new();

        [ObservableProperty]
        private DateTime selectedEarliestDate;
        [ObservableProperty]
        private bool useCustomEarliestDate;

        [ObservableProperty]
        private DateTime selectedLatestDate;
        [ObservableProperty]
        private bool useCustomLatestDate;

        [ObservableProperty]
        private decimal smallestBalanceChange;
        [ObservableProperty]
        private bool useCustomSmallestChange;

        [ObservableProperty]
        private decimal largestBalanceChange;
        [ObservableProperty]
        private bool useCustomLargestChange;
        #endregion

        #region Sorting Related Fields
        [ObservableProperty]
        private List<string> sortMethods;
        [ObservableProperty]
        private LedgerFilterSet.SortingCriteria selectedSortMethod;
        [ObservableProperty]
        private bool useDescendingSortOrder;
        #endregion

        public LedgerFilterPopupViewModel(LedgerFilterSet filterSet, IPopupNavigation popupNavigation)
        {
            _filterSet = filterSet;
            _popUpSvc = popupNavigation;
            try
            {
                using var context = new DatabaseContext();

                AllCostTypes = context.CostTypes.OrderBy(cost => cost.Name).ToList();
                foreach (int id in _filterSet.SelectedCostTypeIds)
                    SelectedCostTypes.Add(context.CostTypes.Find(id));

                AllSeasons = context.Seasons.ToList();
                foreach (int id in _filterSet.SelectedSeasonIds)
                    SelectedSeasons.Add(context.Seasons.Find(id));

                UseCustomEarliestDate = _filterSet.EarliestDate != DateTime.MinValue;
                SelectedEarliestDate = UseCustomEarliestDate ? _filterSet.EarliestDate : DateTime.MinValue;

                UseCustomLatestDate = _filterSet.LatestDate != DateTime.MaxValue;
                SelectedLatestDate = UseCustomLatestDate ? _filterSet.LatestDate : DateTime.MaxValue;

                UseCustomSmallestChange = _filterSet.SmallestBalanceChange != decimal.MinValue;
                SmallestBalanceChange = UseCustomSmallestChange ? _filterSet.SmallestBalanceChange : decimal.MinValue;

                UseCustomLargestChange = _filterSet.LargestBalanceChange != decimal.MaxValue;
                LargestBalanceChange = UseCustomLargestChange ? _filterSet.LargestBalanceChange : decimal.MaxValue;

                SortMethods = new()
                {
                    SortingCriteriaToStringConverter.DateAdded,
                    SortingCriteriaToStringConverter.BalanceChange,
                    SortingCriteriaToStringConverter.CostTypes,
                    SortingCriteriaToStringConverter.SeasonStartDate
                };
                SelectedSortMethod = _filterSet.SortingMethod;
                UseDescendingSortOrder = _filterSet.DescendingSort;
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        [RelayCommand]
        private async Task Apply()
        {
            List<int> costTypeIds = new();
            foreach (CostType cost in SelectedCostTypes.Cast<CostType>())
                costTypeIds.Add(cost.Id);

            //Warn when no cost type was selected
            if (costTypeIds.Count == 0)
            {
                await App.AlertSvc.ShowAlertAsync(
                    "Brak wybranych kosztów",
                    "Nie wybrano żadnych rodzajów kosztów do uwzględnienia. Oznacza to, że nie zostanie pokazany żaden wpis. Zaznacz na zielono rodzaje kosztów, które mają posiadać wpisy aby zostały pokazane.");
                return;
            }

            List<int> seasons = new();
            foreach (Season season in SelectedSeasons.Cast<Season>())
                seasons.Add(season.Id);

            //Warn when no season was selected
            if (seasons.Count == 0)
            {
                await App.AlertSvc.ShowAlertAsync(
                    "Brak wybranych sezonów",
                    "Nie wybrano żadnego sezonu do uwzględnienia. Oznacza to, że nie zostanie pokazany żaden wpis. Zaznacz na zielono sezony, z których wpisy mają być pokazane.");
                return;
            }

            //Condition of LargestBalance >= SmallerBalance
            if (LargestBalanceChange < SmallestBalanceChange)
            {
                await App.AlertSvc.ShowAlertAsync(
                    "Zły zakres wartości kosztu",
                    "Najmniejszy koszt jest większy od największego kosztu, przez co zakres wartości kosztów jest nie poprawny. Zamień je miejscami, lub wyłącz jeden z nich aby ustawić jednostronny zakres.");
                return;
            }

            //Condition EarliestDate >= LatestDate is enforced in View
            var newFilterSet = new LedgerFilterSet(costTypeIds, seasons)
            {
                EarliestDate = UseCustomEarliestDate ? SelectedEarliestDate.Date : DateTime.MinValue,
                LatestDate = UseCustomLatestDate ? SelectedLatestDate.Date.AddDays(1).AddMicroseconds(-1) : DateTime.MaxValue,
                SmallestBalanceChange = UseCustomSmallestChange ? SmallestBalanceChange : decimal.MinValue,
                LargestBalanceChange = UseCustomLargestChange ? LargestBalanceChange : decimal.MaxValue,
                SortingMethod = SelectedSortMethod,
                DescendingSort = UseDescendingSortOrder
            };
            OnFilterSetCreated?.Invoke(this, newFilterSet);
            Pop();
        }

        [RelayCommand]
        private void Pop()
        {
            OnPageQuit?.Invoke(this, null);
            _popUpSvc.PopAsync();
        }

        //TODO: applies to all partial methods below - load default timespan from Preferences
        partial void OnUseCustomEarliestDateChanged(bool oldValue, bool newValue) => 
            SelectedEarliestDate = newValue ? DateTime.Now.AddMonths(-1) : DateTime.MinValue;

        partial void OnUseCustomLatestDateChanged(bool oldValue, bool newValue) => 
            SelectedLatestDate = newValue ? DateTime.Now : DateTime.MaxValue;

        partial void OnUseCustomSmallestChangeChanged(bool value) => 
            SmallestBalanceChange = 0.00m;

        partial void OnUseCustomLargestChangeChanged(bool value) => 
            LargestBalanceChange = 999_999m;
    }
}

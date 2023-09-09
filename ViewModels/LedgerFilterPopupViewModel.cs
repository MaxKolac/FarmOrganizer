using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.Views.PopUps;
using Mopups.Interfaces;

namespace FarmOrganizer.ViewModels
{
    public partial class LedgerFilterPopupViewModel : ObservableObject
    {
        private LedgerFilterSet _filterSet;
        private IPopupNavigation _popUpSvc;

        #region Collections and Choices Bindings
        [ObservableProperty]
        private List<CostType> allCostTypes;
        [ObservableProperty]
        private List<CostType> selectedCostTypes;

        [ObservableProperty]
        private List<Season> allSeasons;
        [ObservableProperty]
        private List<Season> selectedSeasons;

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
        //private List<string> sortMethods; //TODO: converter
        private List<LedgerFilterSet.SortBy> sortMethods; //TODO: converter
        [ObservableProperty]
        private LedgerFilterSet.SortBy selectedSortMethod; //TODO: converter
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

                AllCostTypes = context.CostTypes.ToList();
                SelectedCostTypes = new();
                foreach (int id in _filterSet.SelectedCostTypeIds)
                    SelectedCostTypes.Add(context.CostTypes.Find(id));

                AllSeasons = context.Seasons.ToList();
                SelectedSeasons = new();
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

                SortMethods = new();
                SortMethods.AddRange(Enum.GetValues<LedgerFilterSet.SortBy>());
                SelectedSortMethod = _filterSet.SortingMethod;
                UseDescendingSortOrder = _filterSet.DescendingSort;

                //what happens when ShowALert proceeds with true?
                //throw new Exception("please break stuff");
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        private LedgerFilterSet PackFieldValuesToFilterSet()
        {
            List<int> costTypeIds = new();
            foreach (CostType cost in SelectedCostTypes)
                costTypeIds.Add(cost.Id);

            List<int> seasons = new();
            foreach (Season season in SelectedSeasons)
                seasons.Add(season.Id);

            return new LedgerFilterSet(costTypeIds, seasons)
            {
                EarliestDate = UseCustomEarliestDate ? SelectedEarliestDate : DateTime.MinValue,
                LatestDate = UseCustomLatestDate ? SelectedLatestDate : DateTime.MaxValue,
                SmallestBalanceChange = UseCustomSmallestChange ? SmallestBalanceChange : decimal.MinValue, 
                LargestBalanceChange = UseCustomLargestChange ? LargestBalanceChange : decimal.MaxValue, 
                SortingMethod = SelectedSortMethod,
                DescendingSort = UseDescendingSortOrder
            };
        }

        [RelayCommand]
        private void Apply()
        {
            //How to pass the ersulting LedgerFilterSet back to LedgerPageVM?
        }
        
        [RelayCommand]
        private void Cancel() => _popUpSvc.PopAsync();

        //TODO: applies to all partial methods below - load default timespan from Preferences
        partial void OnUseCustomEarliestDateChanged(bool oldValue, bool newValue)
        {
            SelectedEarliestDate = newValue ? DateTime.Now.AddMonths(-1) : DateTime.MinValue;
        }

        partial void OnUseCustomLatestDateChanged(bool oldValue, bool newValue)
        {
            SelectedLatestDate = newValue ? DateTime.Now : DateTime.MaxValue;
        }

        partial void OnUseCustomSmallestChangeChanged(bool oldValue, bool newValue)
        {
            SmallestBalanceChange = newValue ? 0.00m : decimal.MinValue;
        }

        partial void OnUseCustomLargestChangeChanged(bool oldValue, bool newValue)
        {
            LargestBalanceChange = newValue ? 1000m : decimal.MaxValue;
        }
    }
}

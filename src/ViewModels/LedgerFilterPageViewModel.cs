using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.Converters;
using FarmOrganizer.ViewModels.Helpers;
using System.Collections.ObjectModel;

namespace FarmOrganizer.ViewModels
{
    [QueryProperty(nameof(_filterSet), "filterSet")]
    public partial class LedgerFilterPageViewModel : ObservableObject, IQueryAttributable
    {
        readonly IPopupService popupService;

        LedgerFilterSet _filterSet;

        #region Collections and Choices Bindings
        [ObservableProperty]
        List<CropField> allCropFields = new();
        [ObservableProperty]
        ObservableCollection<object> selectedCropFields = new();

        [ObservableProperty]
        ObservableCollection<CostTypeGroup> allCostTypes = new();
        [ObservableProperty]
        ObservableCollection<object> selectedCostTypes = new();

        [ObservableProperty]
        List<Season> allSeasons = new();
        [ObservableProperty]
        ObservableCollection<object> selectedSeasons = new();

        [ObservableProperty]
        DateTime selectedEarliestDate = DateTime.MinValue;
        [ObservableProperty]
        bool useCustomEarliestDate;

        [ObservableProperty]
        DateTime selectedLatestDate = Season.MaximumDate;
        [ObservableProperty]
        bool useCustomLatestDate;

        [ObservableProperty]
        decimal smallestBalanceChange;
        [ObservableProperty]
        bool useCustomSmallestChange;

        [ObservableProperty]
        decimal largestBalanceChange;
        [ObservableProperty]
        bool useCustomLargestChange;
        #endregion

        #region Sorting Related Fields
        [ObservableProperty]
        ObservableCollection<string> sortMethods;
        [ObservableProperty]
        LedgerFilterSet.SortingCriteria selectedSortMethod;
        [ObservableProperty]
        bool useDescendingSortOrder;
        #endregion

        public static event EventHandler<LedgerFilterSet> OnFilterSetCreated;
        public static event EventHandler OnPageQuit;

        public LedgerFilterPageViewModel(IPopupService popupService)
        {
            this.popupService = popupService;
            //The only way to open this Page is through LedgerPage, which already validates all tables
            //Because of this, LedgerFilterPage is allowed to proceed without validation
            using var context = new DatabaseContext();
            SortMethods = new()
            {
                SortingCriteriaToStringConverter.DateAdded,
                SortingCriteriaToStringConverter.BalanceChange,
                SortingCriteriaToStringConverter.CostTypes,
                SortingCriteriaToStringConverter.SeasonStartDate
            };
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _filterSet = query["filterSet"] as LedgerFilterSet;
            using var context = new DatabaseContext();

            AllCropFields = context.CropFields.OrderBy(field => field.Name).ToList();
            SelectedCropFields.Clear();
            foreach (int id in _filterSet.SelectedCropFieldIds)
                SelectedCropFields.Add(context.CropFields.FirstOrDefault(e => e.Id == id));

            AllCostTypes = CostType.BuildCostTypeGroups("Uwzględnij rodzaje przychodów:", "Uwzględnij rodzaje wydatków:", context);
            SelectedCostTypes.Clear();
            foreach (int id in _filterSet.SelectedCostTypeIds)
                SelectedCostTypes.Add(context.CostTypes.FirstOrDefault(e => e.Id == id));

            AllSeasons = context.Seasons.OrderBy(season => season.DateStart).ToList();
            SelectedSeasons.Clear();
            foreach (int id in _filterSet.SelectedSeasonIds)
                SelectedSeasons.Add(context.Seasons.FirstOrDefault(e => e.Id == id));

            UseCustomEarliestDate = _filterSet.EarliestDate != DateTime.MinValue;
            SelectedEarliestDate = UseCustomEarliestDate ? _filterSet.EarliestDate : DateTime.MinValue;

            UseCustomLatestDate = _filterSet.LatestDate != Season.MaximumDate;
            SelectedLatestDate = UseCustomLatestDate ? _filterSet.LatestDate : Season.MaximumDate;

            UseCustomSmallestChange = _filterSet.SmallestBalanceChange != decimal.MinValue;
            SmallestBalanceChange = UseCustomSmallestChange ? _filterSet.SmallestBalanceChange : decimal.MinValue;

            UseCustomLargestChange = _filterSet.LargestBalanceChange != decimal.MaxValue;
            LargestBalanceChange = UseCustomLargestChange ? _filterSet.LargestBalanceChange : decimal.MaxValue;

            SelectedSortMethod = _filterSet.SortingMethod;
            UseDescendingSortOrder = _filterSet.DescendingSort;
        }

        [RelayCommand]
        private void CropFieldsClearAndFill(bool refillAfterClearing)
        {
            SelectedCropFields.Clear();
            if (refillAfterClearing)
            {
                foreach (var field in AllCropFields)
                    SelectedCropFields.Add(field);
            }
        }

        [RelayCommand]
        private void CostTypesClearAndFill(bool refillAfterClearing)
        {
            SelectedCostTypes.Clear();
            if (refillAfterClearing)
            {
                foreach (var cost in AllCostTypes[0])
                    SelectedCostTypes.Add(cost);
                foreach (var cost in AllCostTypes[1])
                    SelectedCostTypes.Add(cost);
            }
        }

        [RelayCommand]
        private void SeasonsClearAndFill(bool refillAfterClearing)
        {
            SelectedSeasons.Clear();
            if (refillAfterClearing)
            {
                foreach (var season in AllSeasons)
                    SelectedSeasons.Add(season);
            }
        }

        [RelayCommand]
        private async Task ReturnToPreviousPage()
        {
            OnPageQuit?.Invoke(this, null);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private void ResetFilters()
        {
            var selfQuery = new Dictionary<string, object>()
            {
                { "filterSet", LedgerPageViewModel.GetDefaultFilterSet() }
            };
            ApplyQueryAttributes(selfQuery);
        }

        [RelayCommand]
        private async Task Apply()
        {
            List<int> cropFieldsIds = new();
            foreach (CropField field in SelectedCropFields.Cast<CropField>())
                cropFieldsIds.Add(field.Id);

            //Warn when no crop field was selected
            if (cropFieldsIds.Count == 0)
            {
                await App.AlertSvc.ShowAlertAsync(
                    "Brak wybranych pól uprawnych",
                    "Nie wybrano żadnych pól uprawnych do uwzględnienia. Oznacza to, że nie zostanie pokazany żaden wpis. Zaznacz na zielono pola uprawne, którch wpisy mają zostać pokazane.");
                return;
            }

            List<int> costTypeIds = new();
            foreach (CostType cost in SelectedCostTypes.Cast<CostType>())
                costTypeIds.Add(cost.Id);

            //Warn when no cost type was selected
            if (costTypeIds.Count == 0)
            {
                await App.AlertSvc.ShowAlertAsync(
                    "Brak wybranych rodzajów wpisów",
                    "Nie wybrano żadnych rodzajów wpisów do uwzględnienia. Oznacza to, że nie zostanie pokazany żaden wpis. Zaznacz na zielono rodzaje, które mają posiadać wpisy aby zostały pokazane.");
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
                    "Najmniejszy koszt jest większy od największego kosztu, przez co zakres wartości kosztów jest nie poprawny. Zamień je miejscami, lub wyłącz jeden z nich aby ustawić jednostronnie otwarty zakres.");
                return;
            }

            //Condition EarliestDate >= LatestDate is enforced in View
            var newFilterSet = new LedgerFilterSet(cropFieldsIds, costTypeIds, seasons)
            {
                EarliestDate = UseCustomEarliestDate ? SelectedEarliestDate.Date : DateTime.MinValue,
                LatestDate = UseCustomLatestDate ? SelectedLatestDate.Date.AddDays(1).AddMicroseconds(-1) : Season.MaximumDate,
                SmallestBalanceChange = UseCustomSmallestChange ? SmallestBalanceChange : decimal.MinValue,
                LargestBalanceChange = UseCustomLargestChange ? LargestBalanceChange : decimal.MaxValue,
                SortingMethod = SelectedSortMethod,
                DescendingSort = UseDescendingSortOrder
            };
            OnFilterSetCreated?.Invoke(this, newFilterSet);
            await ReturnToPreviousPage();
        }

        //TODO: applies to all partial methods below - load default timespan from Preferences
        partial void OnUseCustomEarliestDateChanged(bool oldValue, bool newValue) =>
            SelectedEarliestDate = newValue ? DateTime.Now.AddMonths(-1) : DateTime.MinValue;

        partial void OnUseCustomLatestDateChanged(bool oldValue, bool newValue) =>
            SelectedLatestDate = newValue ? DateTime.Now : Season.MaximumDate;

        partial void OnUseCustomSmallestChangeChanged(bool value) =>
            SmallestBalanceChange = 0.00m;

        partial void OnUseCustomLargestChangeChanged(bool value) =>
            LargestBalanceChange = 999_999m;
    }
}
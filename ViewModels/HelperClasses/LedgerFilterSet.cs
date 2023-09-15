using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels.HelperClasses
{
    public class LedgerFilterSet
    {
        public List<int> SelectedCostTypeIds { get; set; }
        public DateTime EarliestDate { get; set; } = DateTime.MinValue;
        public DateTime LatestDate { get; set; } = DateTime.MaxValue;
        public List<int> SelectedSeasonIds { get; set; }
        public decimal SmallestBalanceChange { get; set; } = decimal.MinValue;
        public decimal LargestBalanceChange { get; set; } = decimal.MaxValue;

        public bool DescendingSort { get; set; } = true;
        public SortBy SortingMethod { get; set; } = SortBy.DateAdded;

        public enum SortBy
        {
            CostTypes,
            DateAdded,
            SeasonStartDate,
            BalanceChange
        }

        /// <summary>
        /// Creates a LedgerFilterSet which attempts to load all IDs of all <see cref="CostType"/> and <see cref="Season"/> entries from a database through instance of <see cref="DatabaseContext"/>. Might throw an exception.
        /// </summary>
        public LedgerFilterSet()
        {
            try
            {
                using var context = new DatabaseContext();

                SelectedCostTypeIds = new();
                foreach (CostType cost in context.CostTypes.ToList())
                    SelectedCostTypeIds.Add(cost.Id);

                SelectedSeasonIds = new();
                foreach (Season season in context.Seasons.ToList())
                    SelectedSeasonIds.Add(season.Id);
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        /// <summary>
        /// Creates a LedgerFilter which skips any queries to <see cref="DatabaseContext"/> and instead takes in lists of IDs provided as parameters.
        /// </summary>
        /// <param name="costTypeIds">List of <see cref="CostType"/> ID's to add.</param>
        /// <param name="seasonIds">List of <see cref="Season"/> ID's to add.</param>
        public LedgerFilterSet(List<int> costTypeIds, List<int> seasonIds)
        {
            SelectedCostTypeIds = new();
            SelectedCostTypeIds.AddRange(costTypeIds);
            SelectedSeasonIds = new();
            SelectedSeasonIds.AddRange(seasonIds);
        }
    }
}

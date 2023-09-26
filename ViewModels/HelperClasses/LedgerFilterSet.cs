using FarmOrganizer.Database;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels.HelperClasses
{
    public class LedgerFilterSet
    {
        public List<int> SelectedCropFieldIds { get; set; } = new();
        public List<int> SelectedCostTypeIds { get; set; } = new();
        public List<int> SelectedSeasonIds { get; set; } = new();
        public DateTime EarliestDate { get; set; } = DateTime.MinValue;
        public DateTime LatestDate { get; set; } = Season.MaximumDate;
        public decimal SmallestBalanceChange { get; set; } = decimal.MinValue;
        public decimal LargestBalanceChange { get; set; } = decimal.MaxValue;

        public bool DescendingSort { get; set; } = true;
        public SortingCriteria SortingMethod { get; set; } = SortingCriteria.DateAdded;

        public enum SortingCriteria
        {
            CostTypes,
            DateAdded,
            SeasonStartDate,
            BalanceChange
        }

        /// <summary>
        /// Creates a LedgerFilterSet which attempts to load all IDs of all <see cref="CropField"/>, <see cref="CostType"/> and <see cref="Season"/> entries from a database.
        /// </summary>
        public LedgerFilterSet()
        {
            using var context = new DatabaseContext();
            foreach (CropField field in context.CropFields.ToList())
                SelectedCropFieldIds.Add(field.Id);

            foreach (CostType cost in context.CostTypes.ToList())
                SelectedCostTypeIds.Add(cost.Id);

            foreach (Season season in context.Seasons.ToList())
                SelectedSeasonIds.Add(season.Id);
        }

        /// <summary>
        /// Creates a LedgerFilter which skips any queries to <see cref="DatabaseContext"/> and instead takes in lists of IDs provided as parameters.
        /// </summary>
        /// <param name="cropFieldIds">List of <see cref="CropField"/> ID's to add.</param>
        /// <param name="costTypeIds">List of <see cref="CostType"/> ID's to add.</param>
        /// <param name="seasonIds">List of <see cref="Season"/> ID's to add.</param>
        public LedgerFilterSet(List<int> cropFieldIds, List<int> costTypeIds, List<int> seasonIds)
        {
            SelectedCropFieldIds = new();
            SelectedCropFieldIds.AddRange(cropFieldIds);
            SelectedCostTypeIds = new();
            SelectedCostTypeIds.AddRange(costTypeIds);
            SelectedSeasonIds = new();
            SelectedSeasonIds.AddRange(seasonIds);
        }
    }
}

using FarmOrganizer.ViewModels.HelperClasses;
using System.Globalization;

namespace FarmOrganizer.ViewModels.Converters
{
    public class SortingCriteriaToStringConverter : IValueConverter
    {
        public const string BalanceChange = "Wartość wydatku";
        public const string CostTypes = "Rodzaje kosztów";
        public const string DateAdded = "Data dodania";
        public const string SeasonStartDate = "Data początku sezonu";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not LedgerFilterSet.SortingCriteria)
                return string.Empty;
            return (LedgerFilterSet.SortingCriteria)value switch
            {
                LedgerFilterSet.SortingCriteria.BalanceChange => BalanceChange,
                LedgerFilterSet.SortingCriteria.CostTypes => CostTypes,
                LedgerFilterSet.SortingCriteria.DateAdded => DateAdded,
                LedgerFilterSet.SortingCriteria.SeasonStartDate => SeasonStartDate,
                _ => string.Empty,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string)
                return LedgerFilterSet.SortingCriteria.DateAdded;
            return (string)value switch
            {
                BalanceChange => LedgerFilterSet.SortingCriteria.BalanceChange,
                CostTypes => LedgerFilterSet.SortingCriteria.CostTypes,
                DateAdded => LedgerFilterSet.SortingCriteria.DateAdded,
                SeasonStartDate => LedgerFilterSet.SortingCriteria.SeasonStartDate,
                _ => LedgerFilterSet.SortingCriteria.DateAdded
            };
        }
    }
}

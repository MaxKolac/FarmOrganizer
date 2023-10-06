using FarmOrganizer.Models;
using System.Globalization;

namespace FarmOrganizer.ViewModels.Converters
{
    /// <summary>
    /// A workaround for bugged DataTemplate Selector.<br/>
    /// Used in <see cref="Views.LedgerPage"/> to change the balance change's text color to be red when it's a cost
    /// </summary>
    public class CostTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not CostType)
                return Colors.Black;
            CostType cost = value as CostType;
            return cost.IsExpense ? Colors.DarkRed : Colors.DarkGreen;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

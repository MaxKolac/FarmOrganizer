using System.Globalization;

namespace FarmOrganizer.ViewModels.Converters
{
    /// <summary>
    /// Used by <see cref="CostTypePageViewModel"/>. 
    /// <para>
    /// Converts <see cref="Models.CostType.IsExpense"/> to a user-friendly string. 
    /// Converting back will throw <see cref="NotImplementedException"/>.
    /// </para>
    /// </summary>
    internal class IsExpenseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
                return string.Empty;
            bool isExpense = (bool)value;
            return isExpense ? "Wydatek" : "Przychód";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

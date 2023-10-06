using FarmOrganizer.Models;
using System.Globalization;

namespace FarmOrganizer.ViewModels.Converters
{
    /// <summary>
    /// Used by <see cref="SeasonsPageViewModel"/>. 
    /// <para>
    /// Converts <see cref="DateTime"/> struct to a user-friendly string. If the <see cref="DateTime"/> is greater than year 9999, empty string is returned. 
    /// Converting back will throw <see cref="NotImplementedException"/>.
    /// </para>
    /// </summary>
    internal class DateEndConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime)
                return string.Empty;
            DateTime date = (DateTime)value;
            if (date >= Season.MaximumDate)
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

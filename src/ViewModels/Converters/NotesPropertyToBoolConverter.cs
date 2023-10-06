using FarmOrganizer.Models;
using FarmOrganizer.Views;
using System.Globalization;

namespace FarmOrganizer.ViewModels.Converters
{
    /// <summary>
    /// Used in <see cref="LedgerPage"/> to hide DataTemplate's Label when <see cref="BalanceLedger.Notes"/> is empty. 
    /// </summary>
    public class NotesPropertyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string)
                return false;
            string note = (string)value;
            return !string.IsNullOrEmpty(note);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

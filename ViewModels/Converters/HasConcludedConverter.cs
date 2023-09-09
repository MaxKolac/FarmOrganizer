using System.Globalization;

namespace FarmOrganizer.ViewModels.Converters
{
    /// <summary>
    /// Used by <see cref="SeasonsPageViewModel"/>. 
    /// <para>
    /// Converts <see cref="Models.Season.HasConcluded"/> to a user-friendly string. 
    /// Converting back one of valid strings returns a boolean <see cref="Models.Season.HasConcluded"/>.
    /// </para>
    /// </summary>
    internal class HasConcludedConverter : IValueConverter
    {
        private readonly string _done = "Zakończony";
        private readonly string _inProgress = "W trakcie";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
                return "ERROR";
            bool hasConcluded = (bool)value;
            return hasConcluded ? _done : _inProgress;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string)
                return false;
            string text = (string)value;
            return text.Equals(_done);
        }
    }
}

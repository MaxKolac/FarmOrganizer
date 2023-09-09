using System.Globalization;

namespace FarmOrganizer.ViewModels.Converters
{
    /// <summary>
    /// Used by <see cref="SettingsPageViewModel"/>. 
    /// <para>
    /// Converts <see cref="AppTheme"/> to a user-friendly string. 
    /// Converting back one of predefined strings will return a <see cref="AppTheme"/>.
    /// </para>
    /// </summary>
    internal class AppThemeToStringConverter : IValueConverter
    {
        public const string Default = "Obecny motyw urządzenia";
        public const string Light = "Jasny";
        public const string Dark = "Ciemny";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not AppTheme)
                return string.Empty;
            return (AppTheme)value switch
            {
                AppTheme.Unspecified => Default,
                AppTheme.Light => Light,
                AppTheme.Dark => Dark,
                _ => string.Empty,
            };
        }

        /// <summary>
        /// Converts a string into an AppTheme enum with the matching name.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string)
                return AppTheme.Unspecified;
            return (string)value switch
            {
                Default => AppTheme.Unspecified,
                Light => AppTheme.Light,
                Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }
    }
}

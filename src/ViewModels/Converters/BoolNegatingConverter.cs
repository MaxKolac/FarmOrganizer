﻿using System.Globalization;

namespace FarmOrganizer.ViewModels.Converters
{
    /// <summary>
    /// A simple converter, which converts a boolean value to its opposite value.<br/>
    /// </summary>
    public class BoolNegatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}

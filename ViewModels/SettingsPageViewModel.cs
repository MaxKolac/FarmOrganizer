﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace FarmOrganizer.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<string> appThemes = Enum.GetNames<AppTheme>().ToList();
        [ObservableProperty]
        private AppTheme selectedTheme = 
            Enum.Parse<AppTheme>(Preferences.Get("appTheme", Enum.GetName(AppTheme.Unspecified)));

        partial void OnSelectedThemeChanging(AppTheme oldValue, AppTheme newValue)
        {
            Preferences.Set("appTheme", Enum.GetName(newValue));
            ApplyPreferences();
        }

        public static void ApplyPreferences()
        {
            Application.Current.UserAppTheme = Enum.Parse<AppTheme>(Preferences.Get("appTheme", Enum.GetName(AppTheme.Unspecified)));
        }
    }

    internal class StringToAppThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not AppTheme)
                return string.Empty;
            return Enum.GetName((AppTheme)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string)
                return AppTheme.Unspecified;
            foreach (string theme in Enum.GetNames<AppTheme>().ToList())
            {
                if (theme == (string)value)
                    return Enum.Parse<AppTheme>(theme);
            }
            return AppTheme.Unspecified;
        }
    }
}

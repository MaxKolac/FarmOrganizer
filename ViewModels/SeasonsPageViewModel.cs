﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels
{
    public partial class SeasonsPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<Season> seasons = new();
        [ObservableProperty]
        private bool addingNewSeason = false;

        #region New Season Details
        [ObservableProperty]
        private string newSeasonName;
        [ObservableProperty]
        private DateTime newSeasonDateStart;
        #endregion

        public SeasonsPageViewModel()
        {
            try
            {
                Season.Validate();
                Seasons.AddRange(new DatabaseContext().Seasons.ToList());
                NewSeasonName = "Nowy sezon " + DateTime.Now.Year.ToString();
                NewSeasonDateStart = DateTime.Now;
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        [RelayCommand]
        private void StartNewSeason()
        {
            try
            {
                Season newSeason = new()
                {
                    Name = NewSeasonName,
                    DateStart = NewSeasonDateStart,
                    DateEnd = DateTime.MaxValue,
                    HasConcluded = false
                };
                Season.AddEntry(newSeason);
                Seasons = new DatabaseContext().Seasons.ToList();
                ToggleNewSeasonFrame();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        [RelayCommand]
        private static void ResumePreviousSeason()
        {

        }

        [RelayCommand]
        private void ToggleNewSeasonFrame() =>
            AddingNewSeason = !AddingNewSeason;
    }
}

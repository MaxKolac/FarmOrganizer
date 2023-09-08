using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels
{
    public partial class CropFieldPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private List<CropField> cropFields;
        [ObservableProperty]
        private bool addingNewCropField = false;

        [ObservableProperty]
        private string newCropFieldName = "Nowe pole";
        [ObservableProperty]
        private string newCropFieldHectares;

        public CropFieldPageViewModel()
        {
            try
            {
                using var context = new DatabaseContext();
                CropFields = context.CropFields.ToList();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert();
            }
        }

        [RelayCommand]
        private void OpenEfficiencyPage(CropField cropField)
        {

        }

        [RelayCommand]
        private void AddNewCropField()
        {
            try
            {
                using var context = new DatabaseContext();
                CropField newField = new()
                {
                    Name = NewCropFieldName,
                    Hectares = Utils.CastToValue(NewCropFieldHectares)
                };
                context.CropFields.Add(newField);
                context.SaveChanges();
                CropFields = context.CropFields.ToList();
                ToggleAddingNewCropField();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        [RelayCommand]
        private async Task RemoveCropField(CropField cropFieldToRemove)
        {
            if (!await App.AlertSvc.ShowConfirmationAsync(
                "Uwaga!",
                "Usunięcie pola uprawnego usunie również WSZYSTKIE wpisy, które były podpięte pod usuwane pole. Tej operacji nie można cofnąć. Czy chcesz kontynuować?",
                "Tak, usuń",
                "Anuluj"))
                return;
            try
            {
                using var context = new DatabaseContext();
                context.CropFields.Remove(cropFieldToRemove);
                context.SaveChanges();
                CropFields = context.CropFields.ToList();
            }
            catch (Exception ex)
            {
                new ExceptionHandler(ex).ShowAlert(false);
            }
        }

        [RelayCommand]
        private void ToggleAddingNewCropField() =>
            AddingNewCropField = !AddingNewCropField;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;

namespace FarmOrganizer.ViewModels
{
    public partial class PopupPageViewModel : ObservableObject
    {
        [ObservableProperty]
        string title;
        [ObservableProperty]
        string description;
        [ObservableProperty]
        bool isConfirmable;

        /// <summary>
        /// Allows to set the popup's information shown to the user through a single method.
        /// </summary>
        /// <param name="title">The bolder label shown at the top of the popup.</param>
        /// <param name="description">The label describing a confirmable action or information to the user.</param>
        /// <param name="isConfirmable">If <c>true</c> the user will be presented with a choice between "Yes" and "No" buttons. Otherwise, only a single "OK" dismissing button is shown.</param>
        public void SetInfo(string title, string description, bool isConfirmable = false)
        {
            Title = title;
            Description = description;
            IsConfirmable = isConfirmable;
        }
    }
}
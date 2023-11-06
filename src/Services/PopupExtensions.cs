using CommunityToolkit.Maui.Core;
using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Services
{
    public static class PopupExtensions
    {
        /// <summary>
        /// Prompts the user with a popup to confirm or cancel a certain action.
        /// </summary>
        /// <param name="popupService">The viewmodel's dependency injected <see cref="IPopupService"/> object.</param>
        /// <param name="title">A short bold label at the top of the popup.</param>
        /// <param name="description">A label describing an action to confirm or simple information.</param>
        /// <returns><c>false</c>, if the user either manually dismissed the popup with the "Cancel" button or clicked outside the popup. Otherwise <c>true</c>.</returns>
        public static async Task<bool> ShowConfirmationAsync(this IPopupService popupService, string title, string description)
        {
            return await popupService.ShowPopupAsync<PopupPageViewModel>(
                onPresenting: vm => vm.SetInfo(title, description, true)
                ) is bool result && result == true;
        }

        /// <summary>
        /// "Fire and forget" method. Shows to the user a popup and continues execution.
        /// </summary>
        /// <param name="popupService">The viewmodel's dependency injected <see cref="IPopupService"/> object.</param>
        /// <param name="title">A short bold label at the top of the popup.</param>
        /// <param name="description">A label describing an action to confirm or simple information.</param>
        public static void ShowAlert(this IPopupService popupService, string title, string description)
        {
            popupService.ShowPopup<PopupPageViewModel>(
                onPresenting: vm => vm.SetInfo(
                    title,
                    description
                    )
                );
        }
    }
}
namespace FarmOrganizer.Services
{
    /// <inheritdoc/>
    [Obsolete("Replaced with CommunityToolkitMaui's PopupService, see https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/views/popup#presenting-a-popup. This class is only preserved as a working fallback in case the PopupService breaks down.")]
    internal class AlertService : IAlertService
    {
        /// <inheritdoc/>
        public Task ShowAlertAsync(string title, string message, string cancel = "OK") =>
            Application.Current.MainPage.DisplayAlert(title, message, cancel);

        /// <inheritdoc/>
        public Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No") =>
            Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);

        /// <inheritdoc/>
        public void ShowAlert(string title, string message, string cancel = "OK")
        {
            Application.Current.MainPage.Dispatcher.Dispatch(async () =>
                await ShowAlertAsync(title, message, cancel)
            );
        }

        /// <inheritdoc/>
        public void ShowConfirmation(string title, string message, Action<bool> callback, string accept = "Yes", string cancel = "No")
        {
            Application.Current.MainPage.Dispatcher.Dispatch(async () =>
            {
                bool answer = await ShowConfirmationAsync(title, message, accept, cancel);
                callback(answer);
            });
        }
    }
}

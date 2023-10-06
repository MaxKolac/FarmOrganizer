namespace FarmOrganizer.Services
{
    /// <inheritdoc/>
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

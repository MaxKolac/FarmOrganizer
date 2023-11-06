using CommunityToolkit.Maui.Core;
using FarmOrganizer.Services;
using Microsoft.Data.Sqlite;

namespace FarmOrganizer.Exceptions
{
    /// <summary>
    /// Static class containing static methods for handling exceptions as a pop-up alert to the user.
    /// </summary>
    public static class ExceptionHandler
    {
        /// <summary>
        /// Handles exceptions which inherit <see cref="FarmOrganizerException"/> class.
        /// </summary>
        /// <param name="popupService">An <see cref="IPopupService"/> object used by method to show appropriate alerts.</param>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="returnToPreviousPage">If <c>true</c>, the app will return the user to the previous page on the navigation stack.</param>
        public static void Handle(IPopupService popupService, FarmOrganizerException exception, bool returnToPreviousPage)
        {
            PopupExtensions.ShowAlert(popupService, exception.Title, exception.Message);
            if (returnToPreviousPage)
                ReturnToPreviousPage();
        }

        /// <summary>
        /// Handles the <see cref="SqliteException"/>, which require a different approach.
        /// </summary>
        /// <inheritdoc cref="Handle(IPopupService, FarmOrganizerException, bool)"/>
        public static void Handle(IPopupService popupService, SqliteException exception, bool returnToPreviousPage)
        {
            string message = $"Kod błędu: ({exception.SqliteErrorCode}/{exception.SqliteExtendedErrorCode});";
            if (exception.InnerException is not null)
                message += $" Błąd wewnętrzny: {exception.InnerException.Message};";
            if (exception.Message is not null)
                message += $" Wiadomość: {exception.Message};";
            PopupExtensions.ShowAlert(popupService, "Błąd SQLite", message);
            if (returnToPreviousPage)
                ReturnToPreviousPage();
        }

        /// <summary>
        /// Handles the <see cref="IOException"/> when manipulating files.
        /// </summary>
        /// <inheritdoc cref="Handle(IPopupService, FarmOrganizerException, bool)"/>
        public static void Handle(IPopupService popupService, IOException exception, bool returnToPreviousPage)
        {
            PopupExtensions.ShowAlert(popupService, "Błąd podczas pracy na plikach", exception.Message);
            if (returnToPreviousPage)
                ReturnToPreviousPage();
        }

        /// <summary>
        /// <b>Important!</b> This method should NOT be used to handle actual exceptions. Attempting to handle exceptions this way is a bad practice. <br/>
        /// Use this method only to diagnose the cause and type of exceptions being thrown at unexpected moments at runtime.
        /// </summary>
        [Obsolete("Do not use this method to handle actual exceptions. For debugging purposes only.")]
        public static void EmergencyHandle(IPopupService popupService, Exception exception)
        {
            PopupExtensions.ShowAlert(popupService, "Fatalny błąd", exception.Message + "\n\t" + exception.StackTrace);
            ReturnToPreviousPage();
        }

        private static void ReturnToPreviousPage()
        {
            Application.Current.MainPage.Dispatcher.Dispatch(async () =>
            {
                await Shell.Current.GoToAsync("..");
            });
        }
    }
}

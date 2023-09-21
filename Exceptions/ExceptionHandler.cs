using Microsoft.Data.Sqlite;

namespace FarmOrganizer.Exceptions
{
    /// <summary>
    /// Static class containing static methods for handling exceptions as a pop-up alert to the user.
    /// </summary>
    public static class ExceptionHandler
    {
        //public static void Handle(Exception exception, bool returnToPreviousPage = true)
        //{
        //    App.AlertSvc.ShowAlert("Fatalny błąd", exception.Message + "\n\t" + exception.StackTrace);
        //    if (returnToPreviousPage)
        //        ReturnToPreviousPage();
        //}

        /// <summary>
        /// Handles exceptions which inherit <see cref="FarmOrganizerException"/> class.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="returnToPreviousPage">If <c>true</c>, the app will return the user to the previous page on the navigation stack.</param>
        public static void Handle(FarmOrganizerException exception, bool returnToPreviousPage)
        {
            App.AlertSvc.ShowAlert(exception.Title, exception.Message);
            if (returnToPreviousPage)
                ReturnToPreviousPage();
        }

        /// <summary>
        /// Handles the <see cref="SqliteException"/>, which require a different approach.
        /// </summary>
        /// <inheritdoc cref="Handle(FarmOrganizerException, bool)"/>
        public static void Handle(SqliteException exception, bool returnToPreviousPage)
        {
            string message = $"Kod błędu: ({exception.SqliteErrorCode}/{exception.SqliteExtendedErrorCode});";
            if (exception.InnerException is not null)
                message += $" Błąd wewnętrzny: {exception.InnerException.Message};";
            if (exception.Message is not null)
                message += $" Wiadomość: {exception.Message};";
            App.AlertSvc.ShowAlert("Błąd SQLite", message);
            if (returnToPreviousPage)
                ReturnToPreviousPage();
        }

        /// <summary>
        /// Handles the <see cref="IOException"/> when manipulating files.
        /// </summary>
        /// <inheritdoc cref="Handle(FarmOrganizerException, bool)"/>
        public static void Handle(IOException exception, bool returnToPreviousPage)
        {
            App.AlertSvc.ShowAlert("Błąd podczas pracy na plikach", exception.Message);
            if (returnToPreviousPage)
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

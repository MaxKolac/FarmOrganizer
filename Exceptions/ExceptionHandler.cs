﻿using Microsoft.Data.Sqlite;

namespace FarmOrganizer.Exceptions
{
    /// <summary>
    /// A wrapper class for exceptions, which allows the program to show the exception as a pop-up alert.
    /// </summary>
    public class ExceptionHandler
    {
        private readonly Exception _exception;
        private readonly string _title;
        private readonly string _message;
        private const string _genericTitle = "Coś poszło nie tak";

        /// <summary>
        /// Creates an instance for a thrown exception.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        public ExceptionHandler(Exception exception)
        {
            _exception = exception;
            if (exception is SqliteException)
            {
                SqliteException ex = exception as SqliteException;
                _title = _genericTitle;
                _message = $"Program zwrócił błąd związany z bazą danych. " +
                    $"Kod błędu SQLite: {ex.SqliteErrorCode} ({ex.SqliteExtendedErrorCode}).";
            }
            else if (exception is NoRecordFoundException)
            {
                NoRecordFoundException ex = exception as NoRecordFoundException;
                _title = "Nie odnaleziono rekordu";
                _message = $"W tabeli {ex.QuerriedTable} nie odnaleziono rekordu o następujących właściwościach: {ex.QuerriedRecord}.";
            }
            else if (exception is InvalidPageQueryException)
            {
                InvalidPageQueryException ex = exception as InvalidPageQueryException;
                _title = "Błąd podczas ładowania nowej strony";
                _message = $"Program próbował przejść do strony z nieprawidłowymi atrybutami: {ex}";
            }
            else
            {
                _title = "Coś poszło nie tak - Nieobsługiwany błąd";
                _message = exception.Message;
            }
        }

        /// <summary>
        /// Shows the pop-up alert containing info about the wrapped exception.
        /// </summary>
        /// <param name="returnToPreviousPage">Whether or not, should the user be kicked back to the previous page before showing the pop-up.</param>
        public void ShowAlert(bool returnToPreviousPage = true)
        {
            App.AlertSvc.ShowAlert(_title, _message);
            if (returnToPreviousPage)
            {
                Application.Current.MainPage.Dispatcher.Dispatch(async () =>
                {
                    await Shell.Current.GoToAsync("..");
                });
            }
        }
    }
}
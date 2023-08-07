using Microsoft.Data.Sqlite;

namespace FarmOrganizer.Exceptions
{
    public static class ErrorMessages
    {
        public static string GenericTitle => 
            "Coś poszło nie tak";

        /// <summary>
        /// Returns a generic error message, simply stating that something went wrong with the database.
        /// Provided message contains the simple and extended SQLite error codes.
        /// </summary>
        /// <param name="exception">An SqliteException object containing simple and extended error codes.</param>
        public static string GenericMessage(SqliteException exception) =>
            $"Program zwrócił błąd związany z bazą danych. Kod błędu SQLite: {exception.SqliteErrorCode}, {exception.SqliteExtendedErrorCode}.";

        public static string InvalidPropertyQueries(string queryVarName, string queryValue) =>
            $"Próbowano otworzyć stronę z nieprawidłową wartością wyszukania: {queryVarName}={queryValue}";

        public static string RecordNotFound(string querriedTable, int recordId) =>
            $"Nie znaleziono rekordu w tabeli {querriedTable} o wartości ID = {recordId}.";
    }
}

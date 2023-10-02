using FarmOrganizer.Database;

namespace FarmOrganizer.Exceptions
{
    /// <summary>
    /// A critical exception, thrown when program attempted to find a record in the database, but the <see cref="DatabaseContext"/>'s result was null.
    /// </summary>
    public class NoRecordFoundException : FarmOrganizerException
    {
        /// <summary>
        /// Creates a new <see cref="NoRecordFoundException"/>.
        /// </summary>
        /// <param name="querriedTable">The name of the table which was querried for the missing record.</param>
        /// <param name="searchCriteria">The string representation of the search criteria, used to identify the missing record.</param>
        public NoRecordFoundException(string querriedTable, string searchCriteria)
            : base("Nie odnaleziono rekordu", BuildMessage(querriedTable, searchCriteria))
        {
        }

        private static string BuildMessage(string querriedTable, string searchCriteria)
        {
            return $"Nie odnaleziono pasującego rekordu. Tabela: {querriedTable}; Kryterium: {searchCriteria};";
        }
    }
}

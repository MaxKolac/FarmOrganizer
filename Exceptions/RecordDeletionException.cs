using FarmOrganizer.Database;

namespace FarmOrganizer.Exceptions
{
    /// <summary>
    /// Thrown by <see cref="IValidatable{T}.DeleteEntry(T)"/> if the user's attempt to delete a record would cause the table to become invalid, or would impact program's functionality.
    /// </summary>
    public class RecordDeletionException : FarmOrganizerException
    {
        /// <summary>
        /// Creates a new <see cref="RecordDeletionException"/> whose message is simply stating that the given <paramref name="table"/> needs to contain at least 1 record.
        /// </summary>
        /// <param name="table">The name of the table.</param>
        public RecordDeletionException(string table) : base("Nie można usunąć ostatniego rekordu", BuildMessage(table))
        {
        }

        /// <summary>
        /// Creates a new <see cref="RecordDeletionException"/> with another <paramref name="exceptionThrowReason"/> other than the "min. 1 record required".
        /// </summary>
        /// <param name="table">The name of the table.</param>
        /// <param name="exceptionThrowReason">Additional user-friendly sentence, describing the reason for the thrown exception, other than the standard "min. 1 record required".</param>
        public RecordDeletionException(string table, string exceptionThrowReason) : base("Nie można usunąć rekordu", BuildMessage(table, exceptionThrowReason))
        {
        }

        private static string BuildMessage(string table)
        {
            return $"Aby aplikacja działała poprawnie, tabela \'{table}\' musi posiadać przynajmniej jeden rekord.";
        }

        private static string BuildMessage(string table, string exceptionThrowReason)
        {
            return $"Aby aplikacja działała poprawnie, tabela \'{table}\' nie może usunąć tego rekordu. {exceptionThrowReason}";
        }
    }
}

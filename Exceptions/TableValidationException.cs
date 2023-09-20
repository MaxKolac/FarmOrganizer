using FarmOrganizer.Database;

namespace FarmOrganizer.Exceptions
{
    /// <summary>
    /// A critical exception, thrown exclusively by <see cref="IValidatable{T}.Validate()"/> when the table failed a validation process.
    /// </summary>
    public class TableValidationException : FarmOrganizerException
    {
        /// <summary>
        /// Creates a new <see cref="TableValidationException"/>.
        /// </summary>
        /// <param name="table">The name of the table which failed the validation process.</param>
        /// <param name="failedValidationCheck">Description of the failed validation check.</param>
        public TableValidationException(string table, string failedValidationCheck)
            : base("Błąd w tabeli", BuildMessage(table, failedValidationCheck))
        {
        }

        /// <inheritdoc cref="TableValidationException(string, string)"/>
        /// <param name="invalidRecord">The string representation of the record which caused the validation to fail.</param>
        /// <param name="invalidProperty">The name of the property which caused the validation to fail.</param>
        public TableValidationException(string table, string failedValidationCheck, string invalidRecord, string invalidProperty)
            : base("Bład w tabeli", BuildMessage(table, failedValidationCheck, invalidRecord, invalidProperty))
        {
        }

        private static string BuildMessage(string table, string failedValidationCheck)
        {
            return $"Tabela \'{table}\' nie spełniła warunku walidacji: {failedValidationCheck}.";
        }

        private static string BuildMessage(string table, string failedValidationCheck, string invalidRecord, string invalidProperty)
        {
            return BuildMessage(table, failedValidationCheck) + $" Problematyczny rekord: {invalidRecord}; Problematyczny właściwość: {invalidProperty};";
        }
    }
}

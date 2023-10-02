using FarmOrganizer.Database;

#nullable enable
namespace FarmOrganizer.Exceptions
{
    /// <summary>
    /// Thrown by <see cref="IValidatable{T}.AddEntry(T)"/> and <see cref="IValidatable{T}.EditEntry(T)"/> when one of the properties could cause the entry to turn the table invalid, if added or edited. 
    /// </summary>
    public class InvalidRecordPropertyException : FarmOrganizerException
    {
        /// <summary>
        /// Creates a new <see cref="InvalidRecordPropertyException"/>.
        /// </summary>
        /// <param name="property">Name of the property which contained the invalid value.</param>
        /// <param name="value">The invalid value itself</param>
        /// <param name="correctionSuggestion">A user-friendly sentence, explaining how the value should be modified to be valid.</param>
        public InvalidRecordPropertyException(string property, string? value, string correctionSuggestion) :
            base("Nieprawidłowa wartość w rekordzie", BuildMessage(property, value, correctionSuggestion))
        {
        }

        private static string BuildMessage(string property, string? value, string correctionSuggestion)
        {
            value ??= "<brak>";
            return $"W polu {property} znajduje się nieprawidłowa wartość \'{value}\'. {correctionSuggestion}";
        }
    }
}

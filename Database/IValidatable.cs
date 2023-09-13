namespace FarmOrganizer.Database
{
    /// <summary>
    /// Interface implemented by classes in <see cref="Models"/> namespace which require special care and validation for the application to operate correctly. This includes validation before loading their records, before adding new records to their tables or when editing existing records.
    /// </summary>
    public interface IValidatable<T>
    {
        /// <summary>
        /// Retrieves all <typeparamref name="T"/> records from the database and checks their validity.
        /// If at any point any validity check fails, an unhandled exception is thrown.
        /// </summary>
        /// <exception cref="Exceptions.NoRecordFoundException"></exception>
        /// <exception cref="Exceptions.InvalidRecordException"></exception>
        static abstract void Validate();

        /// <summary>
        /// Validates the <typeparamref name="T"/> entry and, if successful, adds it to the table.
        /// If at any points any validity check fails, an unhandled exception is thrown.
        /// </summary>
        /// <exception cref="Exceptions.InvalidRecordException"></exception>
        /// <param name="entry">The <typeparamref name="T"/> object which will be added as a record.</param>
        static abstract void AddEntry(T entry);

        //TODO
        //static abstract void EditEntry(T entry);
    }
}

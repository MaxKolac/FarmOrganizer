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
        /// <inheritdoc cref="Validate()"/>
        /// <para>
        /// This overload allows to immediately retrieve all <typeparamref name="T"/> records to avoid querying the table twice for the same result.
        /// </para>
        /// </summary>
        /// <param name="allEntries">A <see cref="List{T}"/> of all <typeparamref name="T"/> records in the database.</param>
        static abstract void Validate(out List<T> allEntries);

        /// <summary>
        /// Validates the <typeparamref name="T"/> entry and, if successful, adds it to the table.
        /// If at any points any validity check fails, an unhandled exception is thrown.
        /// </summary>
        /// <exception cref="Exceptions.InvalidRecordException"></exception>
        /// <param name="entry">The <typeparamref name="T"/> object which will be added as a record.</param>
        static abstract void AddEntry(T entry);

        /// <summary>
        /// Finds the <typeparamref name="T"/> entry in the table by the ID property and, if found and validates successfully, edits the entry in the table.
        /// If at any points any validity check fails or the record isn't found, an unhandled exception is thrown.
        /// </summary>
        /// <exception cref="Exceptions.NoRecordFoundException"></exception>
        /// <exception cref="Exceptions.InvalidRecordException"></exception>
        /// <param name="entry">The <typeparamref name="T"/> object to look for in the table. Only the object's ID property will be used. The rest of the properties will have their values assigned to the entry in the table.</param>
        static abstract void EditEntry(T entry);

        /// <summary>
        /// Finds the <typeparamref name="T"/> entry in the table by the ID property and, if found, is deleted safely.
        /// If at any points any validity check fails, an unhandled exception is thrown. If the record isn't found, no action is taken.
        /// </summary>
        /// <exception cref="Exceptions.InvalidRecordException"></exception>
        /// <param name="entry">The <typeparamref name="T"/> object to look for in the table. Only the object's ID property will be used.</param>
        static abstract void DeleteEntry(T entry);
    }
}

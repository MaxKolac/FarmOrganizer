namespace FarmOrganizer.Database
{
    /// <summary>
    /// Interface implemented by classes in <see cref="Models"/> namespace which have their representation in the database as a table. This interface provides a layer of abstraction between <see cref="Models"/> classes and performing CRUD operations on their tables. Some of them might also implement a special method to validate if their respective table does not hold any records which could cause the application to not operate correctly.
    /// </summary>
    public interface IDatabaseAccesible<T>
    {
        /// <summary>
        /// Retrieves all <typeparamref name="T"/> records from the database and checks their validity.
        /// If at any point any validity check fails, an unhandled exception is thrown.
        /// </summary>
        /// <exception cref="Exceptions.TableValidationException"></exception>
        /// <param name="context">
        /// An optional <see cref="DatabaseContext"/> object. Use when retrieving records from multiple records and their links between primary and foreign keys should be preserved by retrieving them from the same <see cref="DatabaseContext"/> context.<br/>
        /// If it is passed as <c>null</c>, method creates new <see cref="DatabaseContext"/> for itself and continues execution.</param>
        static abstract void Validate(DatabaseContext context);

        /// <summary>
        /// <inheritdoc cref="Validate(DatabaseContext)"/><br/>
        /// This method allows to immediately retrieve all <typeparamref name="T"/> records to avoid querying the table twice for the same result.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of all <typeparamref name="T"/> records in the database.</returns>
        /// <exception cref="Exceptions.TableValidationException"></exception>
        /// <param name="context"><inheritdoc cref="Validate(DatabaseContext)"/></param>
        static abstract List<T> RetrieveAll(DatabaseContext context);

        /// <summary>
        /// Validates the <typeparamref name="T"/> entry and, if successful, adds it to the table.
        /// The ID property is ignored.
        /// If at any points any validity check fails, an unhandled exception is thrown.
        /// </summary>
        /// <exception cref="Exceptions.InvalidRecordPropertyException"></exception>
        /// <param name="entry">The <typeparamref name="T"/> object which will be added as a record.</param>
        /// <param name="context"><inheritdoc cref="Validate(DatabaseContext)"/></param>
        static abstract void AddEntry(T entry, DatabaseContext context);

        /// <summary>
        /// Finds the <typeparamref name="T"/> entry in the table by the ID property and, if found and validates successfully, edits the found entry with values of <paramref name="entry"/>'s properties.
        /// If at any points any validity check fails or the record isn't found, an unhandled exception is thrown.
        /// </summary>
        /// <exception cref="Exceptions.NoRecordFoundException"></exception>
        /// <exception cref="Exceptions.InvalidRecordPropertyException"></exception>
        /// <param name="entry">The <typeparamref name="T"/> object to look for in the table. Only the object's ID property will be used. The rest of the properties will have their values assigned to the entry in the table.</param>
        /// <param name="context"><inheritdoc cref="Validate(DatabaseContext)"/></param>
        static abstract void EditEntry(T entry, DatabaseContext context);

        /// <summary>
        /// Finds the <typeparamref name="T"/> entry in the table by the ID property and, if found, is deleted safely.
        /// If at any points any validity check fails, an unhandled exception is thrown. If the record isn't found, no action is taken.
        /// </summary>
        /// <exception cref="Exceptions.RecordDeletionException"></exception>
        /// <param name="entry">The <typeparamref name="T"/> object to look for in the table. Only the object's ID property will be used.</param>
        /// <param name="context"><inheritdoc cref="Validate(DatabaseContext)"/></param>
        static abstract void DeleteEntry(T entry, DatabaseContext context);
    }
}

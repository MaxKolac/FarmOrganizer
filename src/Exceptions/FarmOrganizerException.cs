namespace FarmOrganizer.Exceptions
{
    /// <summary>
    /// Base class exception for all app-specific exceptions.
    /// </summary>
    public class FarmOrganizerException : Exception
    {
        /// <summary>
        /// The string which will be shown by <see cref="ExceptionHandler"/> as a pop-up title.
        /// </summary>
        public string Title { get; }

        public FarmOrganizerException(string title, string message) : base(message)
        {
            Title = title;
        }
    }
}

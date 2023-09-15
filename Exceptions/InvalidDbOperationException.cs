namespace FarmOrganizer.Exceptions
{
    public class InvalidDbOperationException : Exception
    {
        public InvalidDbOperationException(string message) : base(message)
        {
        }
    }
}

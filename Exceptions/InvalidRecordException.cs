namespace FarmOrganizer.Exceptions
{
    public class InvalidRecordException : Exception
    {
        public string InvalidProperty { get; }
        public string InvalidValue { get; }

        public InvalidRecordException(string property, string value) 
        {
            InvalidProperty = property;
            InvalidValue = value;
        }
    }
}

namespace FarmOrganizer.Exceptions
{
    internal class NoRecordFoundException : Exception
    {
        public string QuerriedTable { get; }
        public string QuerriedRecord { get; }
        
        public NoRecordFoundException(string querriedTable, string querriedRecord)
        {
            QuerriedTable = querriedTable;
            QuerriedRecord = querriedRecord;
        }
    }
}

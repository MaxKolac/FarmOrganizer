namespace FarmOrganizer
{
    public static class Globals
    {
        /// <summary>
        /// The maximum literal length of a decimal number in NumericEntries.
        /// Used to prevent the app from hanging when Entry.MaxLength is programmatically bypassed by multiplying massive decimals.
        /// </summary>
        public static int NumericEntryMaxLength => 10;
    }
}

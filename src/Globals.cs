namespace FarmOrganizer
{
    public static class Globals
    {
        /// <summary>
        /// The maximum literal length of a decimal number in NumericEntries.
        /// Used to prevent the app from hanging when <see cref="Entry"/>.MaxLength is programmatically bypassed by multiplying massive decimals.
        /// </summary>
        public static int NumericEntryMaxLength => 10;

        /// <summary>
        /// A string message to show on the <see cref="Entry"/> to indicate that <see cref="NumericEntryMaxLength"/> has been exceeded.
        /// </summary>
        public static readonly string NumericEntryMaxLengthExceeded = "Za dużo";
    }
}

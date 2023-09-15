namespace FarmOrganizer.ViewModels.HelperClasses
{
    public class FilterSetEventArgs
    {
        public LedgerFilterSet FilterSet { get; }

        public FilterSetEventArgs(LedgerFilterSet filterSet)
        {
            FilterSet = filterSet;
        }
    }
}

namespace FarmOrganizer.ViewModels.HelperClasses
{
    /// <summary>
    /// Used by <see cref="ReportPageViewModel"/> to show individual cost types and the sum of their expenses.
    /// </summary>
    public class CostTypeReportEntry
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
}
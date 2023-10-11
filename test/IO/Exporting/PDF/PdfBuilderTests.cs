using FarmOrganizer.IO.Exporting.PDF;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.Helpers;

namespace FarmOrganizerTests.IO.Exporting.PDF
{
    public class PdfBuilderTests
    {
        [Fact]
        public static void Test()
        {
            var builder = new PdfBuilder();
            builder.AddCropField(new CropField());
            builder.AddSeason(new Season);
            builder.AddProfitEntry(new CostTypeReportEntry("Profit 1", 123.456m));
            builder.AddExpenseEntry(new CostTypeReportEntry("Expense 1", 123.456m));
        }
    }
}
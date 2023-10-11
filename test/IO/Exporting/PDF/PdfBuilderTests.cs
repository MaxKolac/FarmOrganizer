using FarmOrganizer.IO.Exporting.PDF;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels;

namespace FarmOrganizerTests.IO.Exporting.PDF
{
    public class PdfBuilderTests
    {
        [Fact]
        public static void Test()
        {
            var builder = new PdfBuilder();
            builder.AddCropField(new CropField());
            builder.AddSeason(new Season());

            var random = new Random();
            for (int i = 0; i < 12; i++)
            {
                decimal amount = (decimal)(random.Next(1, 100) + random.NextDouble());
                builder.AddProfitEntry(new CostTypeReportEntry($"Profit {i}", amount));
            }
            for (int i = 0; i < 5; i++)
            {
                decimal amount = (decimal)(random.Next(1, 100) + random.NextDouble());
                builder.AddExpenseEntry(new CostTypeReportEntry($"Expense {i}", amount));
            }
        }
    }
}
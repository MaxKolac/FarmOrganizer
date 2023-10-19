using FarmOrganizer.IO.Exporting.PDF;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels;

namespace FarmOrganizerTests.IO.Exporting.PDF
{
    public class PdfBuilderTests
    {
        [Fact]
        public static void EmptyReportBuildDoesntThrow()
        {
            var builder = new PdfBuilder("FarmOrganizerTests", "X.X.X");
            builder.Build();
        }

        [Fact]
        public static void ExtendedConstructorWithEmptyParamsDoesntThrow()
        {
            var builder = new PdfBuilder("FarmOrganizerTests", "X.X.X", new(), new(), new(), new());
            builder.Build();
        }

        //[Theory]
        //[InlineData("Poletko", 1.25)]
        //[InlineData("Duże pole", 3.2)]
        //public static void AddingCropfields(string fieldName, decimal fieldHectares)
        //{
        //    var cropField = new CropField(fieldName, fieldHectares);
        //    var builder = new PdfBuilder("FarmOrganizerTests", "X.X.X");

        //    builder.AddCropField(cropField);
        //    var document = builder.Build();
        //    var textExtractionResult = document.Pages[0].ExtractText();

        //    Assert.Contains<string>(cropField.Name, textExtractionResult);
        //    Assert.Contains<string>(cropField.Hectares.ToString(), textExtractionResult);
        //}


        void AddingSeasons(Season season)
        {

        }

        void AddingExpenses(CostTypeReportEntry entry)
        {

        }

        void AddingProfits(CostTypeReportEntry entry)
        {

        }

        void AddingEntriesCalculatesTotalCorrectly()
        {

        }
    }
}
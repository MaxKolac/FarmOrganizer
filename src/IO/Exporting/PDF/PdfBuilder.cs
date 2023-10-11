using FarmOrganizer.Models;
using FarmOrganizer.ViewModels;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using System.Globalization;
using static MigraDocCore.DocumentObjectModel.Colors;

namespace FarmOrganizer.IO.Exporting.PDF
{
    /// <summary>
    /// Responsible for instantiating, creating and then building reports as PDF files.<br/>
    /// First, create a new object instance. Use any method with <c>Add</c> prefix to add data to the report. Once ready to be rendered, use <see cref="Build"/> to return a rendered a <see cref="PdfDocument"/> and call <see cref="PdfDocument.Save(Stream)"/> or any of its overloads to save it as a PDF file.
    /// <para>
    /// Class contains some code originally written by <see href="https://github.com/icebeam7">Luis Beltran</see>. Some of it was also taken from his <see href="https://github.com/icebeam7/PDFDemo"> example GitHub repository - PDFDemo</see>
    /// </para>
    /// </summary>
    public class PdfBuilder
    {
        private readonly Document _document;

        private string _cropFieldsLabel = "Pole uprawne:";
        private readonly List<CropField> _cropFields = new();
        private string _seasonsLabel = "Sezon:";
        private readonly List<Season> _seasons = new();
        private readonly List<CostTypeReportEntry> _expenses = new();
        private readonly List<CostTypeReportEntry> _profits = new();

        public static string Filename { get; } = $"Raport-{DateTime.Now:HH-mm-ss-dd-MM-yy}.pdf";

        public PdfBuilder()
        {
            GlobalFontSettings.FontResolver = new GenericFontResolver();
            CultureInfo.CurrentCulture = new CultureInfo("pl-PL", false);
            _document = new();

            _document.Info.Title = "Title";
            _document.Info.Subject = "Subject";
            _document.Info.Author = "Author";
            _document.Info.Keywords = "Keywords";
        }

        /// <summary>
        /// Original Author: <see href="https://github.com/icebeam7">Luis Beltran</see> | <see href="https://github.com/icebeam7/PDFDemo">Source GitHub repository - PDFDemo</see>
        /// </summary>
        public PdfDocument BuildDemo()
        {
            // Modifying default style
            var style = _document.Styles["Normal"];
            style.Font.Name = "OpenSans";
            style.Font.Color = Black;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            style.ParagraphFormat.PageBreakBefore = false;

            // Header style
            style = _document.Styles[StyleNames.Header];
            style.Font.Name = "OpenSans";
            style.Font.Size = 18;
            style.Font.Color = Black;
            style.Font.Bold = true;
            style.Font.Underline = Underline.Single;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            // Footer style
            style = _document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Right);

            // Modifying predefined style: HeadingN (where N goes from 1 to 9)
            style = _document.Styles["Heading1"];
            style.Font.Name = "OpenSans"; // Can be changed (don't forget to add and register the Fonts!)
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.Font.Italic = false;
            style.Font.Color = DarkBlue;
            style.ParagraphFormat.Shading.Color = SkyBlue;
            style.ParagraphFormat.Borders.Distance = "3pt";
            style.ParagraphFormat.Borders.Width = 2.5;
            style.ParagraphFormat.Borders.Color = CadetBlue;
            style.ParagraphFormat.SpaceAfter = "1cm";

            // Modifying predefined style: Heading2
            style = _document.Styles["Heading2"];
            style.Font.Size = 12;
            style.Font.Bold = false;
            style.Font.Italic = true;
            style.Font.Color = DeepSkyBlue;
            style.ParagraphFormat.Shading.Color = White;
            style.ParagraphFormat.Borders.Width = 0;
            style.ParagraphFormat.SpaceAfter = 3;
            style.ParagraphFormat.SpaceBefore = 3;

            // Adding new style
            style = _document.Styles.AddStyle("MyParagraphStyle", "Normal");
            style.Font.Size = 10;
            style.Font.Color = Blue;
            style.ParagraphFormat.SpaceAfter = 3;

            style = _document.Styles.AddStyle("MyTableStyle", "Normal");
            style.Font.Size = 9;
            style.Font.Color = SlateBlue;

            //Adding a header
            var headerSection = _document.AddSection();

            var headerConfig = headerSection.PageSetup;
            headerConfig.Orientation = Orientation.Portrait;
            headerConfig.TopMargin = "3cm";
            headerConfig.LeftMargin = 15;
            headerConfig.BottomMargin = "3cm";
            headerConfig.RightMargin = 15;
            headerConfig.PageFormat = PageFormat.A4;
            headerConfig.OddAndEvenPagesHeaderFooter = true;
            headerConfig.StartingNumber = 1;

            var oddHeader = headerSection.Headers.Primary;

            var headerContent = new Paragraph();
            headerContent.AddText("\tProduct Catalog 2021 - Tech Solutions Inc\t");
            oddHeader.Add(headerContent);
            oddHeader.AddTable();

            var headerForEvenPages = headerSection.Headers.EvenPage;
            headerForEvenPages.AddParagraph("Product Catalog 2021");
            headerForEvenPages.AddTable();

            //Adding a footer
            var footerContent = new Paragraph();
            footerContent.AddText(" Page ");
            footerContent.AddPageField();
            footerContent.AddText(" of ");
            footerContent.AddNumPagesField();

            _document.LastSection.Footers.Primary.Add(footerContent);

            var contentForEvenPages = footerContent.Clone();
            contentForEvenPages.AddTab();
            contentForEvenPages.AddText("\tDate: ");
            contentForEvenPages.AddDateField("dddd, MMMM dd, yyyy HH:mm:ss tt");

            headerSection.Footers.EvenPage.Add(contentForEvenPages);

            //Adding text
            var text1 = "At Tech Solutions Inc, it is our top priority to bring only products of the highest quality to our customers. Products always pass a strict quality control process before they are delivered to you. We put ourselves in the customer's shoes, and only want to offer products that will make our clients happy.";

            var mainParagraph = _document.LastSection.AddParagraph(text1, "Heading1");
            mainParagraph.AddLineBreak();

            text1 = "All components of Tech Solutions Inc sample products have undergone strict laboratory tests for lead, nickel and cadmium content. A world-leading inspection, testing, and certification company has conducted these testsm and as you can see below, our products have passed with perfect note.";
            _document.LastSection.AddParagraph(text1, "Heading2");

            var renderer = new PdfDocumentRenderer
            {
                Document = _document
            };
            renderer.RenderDocument();
            return renderer.PdfDocument;
        }

        public PdfDocument Build()
        {
            //Setup styles:
            // Header style
            // TableHeader style
            // TableColumnHeader style
            // TableRowContent style
            // TableRowSum style
            // Footer style

            //Populate with content
            // Add Header
            // Create table with Expenses
            // Create table with Profits
            // Create table with Grand Total
            // Add Footer

            var renderer = new PdfDocumentRenderer
            {
                Document = _document
            };
            renderer.RenderDocument();
            return renderer.PdfDocument;
        }

        public void AddCropField(CropField cropField)
        {
            _cropFields.Add(cropField);
            _cropFieldsLabel = _cropFields.Count > 1 ? "Pola uprawne:" : "Pole uprawne:";
        }

        public void AddSeason(Season season)
        {
            _seasons.Add(season);
            _seasonsLabel = _seasons.Count > 1 ? "Sezony:" : "Sezon:";
        }

        public void AddExpenseEntry(CostTypeReportEntry costTypeEntry) => _expenses.Add(costTypeEntry);
        public void AddProfitEntry(CostTypeReportEntry costTypeEntry) => _profits.Add(costTypeEntry);
    }
}
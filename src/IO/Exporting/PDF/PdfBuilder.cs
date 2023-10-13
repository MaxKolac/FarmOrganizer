using CommunityToolkit.Maui.Storage;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.Content.Objects;
using PdfSharpCore.Pdf.Content;
using System.Globalization;
using static MigraDocCore.DocumentObjectModel.Colors;

namespace FarmOrganizer.IO.Exporting.PDF
{
    /// <summary>
    /// Responsible for instantiating, creating and then building reports as PDF files.<br/>
    /// First, create a new object instance. Use any method with <c>Add</c> prefix to add data to the report. Once ready to be rendered, use <see cref="Build"/> to return a rendered a <see cref="PdfDocument"/> and call <see cref="PdfDocument.Save(Stream)"/> or any of its overloads to save it as a PDF file.
    /// <para>
    /// Class contains some code originally written by <see href="https://github.com/icebeam7">Luis Beltran</see>. His code was copied and partially modified from his <see href="https://github.com/icebeam7/PDFDemo"> example GitHub repository - PDFDemo</see>
    /// </para>
    /// </summary>
    public class PdfBuilder
    {
        private Document _document;
        private readonly List<CropField> _cropFields = new();
        private readonly List<Season> _seasons = new();
        private readonly List<CostTypeReportEntry> _expenses = new();
        private readonly List<CostTypeReportEntry> _profits = new();

        /// <summary>
        /// The 0. index holds the total sum of expenses, and 1. index is the total sum of profits.
        /// </summary>
        private readonly decimal[] _totalAmounts = new decimal[] { 0, 0 };

        public static string Filename { get; } = $"Raport_{DateTime.Now:HH-mm-ss_dd-MM-yy}.pdf";

        public PdfBuilder()
        {
            InitializeDocument();
        }

        public PdfBuilder(List<CropField> cropFields, List<Season> seasons, List<CostTypeReportEntry> expenses, List<CostTypeReportEntry> profits) : base()
        {
            foreach (var field in cropFields)
                AddCropField(field);
            foreach (var season in seasons)
                AddSeason(season);
            foreach (var expense in expenses)
                AddExpenseEntry(expense);
            foreach (var profit in profits)
                AddProfitEntry(profit);
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

        /// <summary>
        /// Creates a new blank document, runs a sequence of methods which populate it with content, applies styles to it and finally renders it.
        /// </summary>
        /// <returns>
        /// A rendered <see cref="PdfDocument"/>. 
        /// </returns>
        public PdfDocument Build()
        {
            InitializeDocument();
            SetupStyles();

            AddHeader();
            AddReportInfo();
            AddGap();
            AddTable(_expenses);
            AddGap();
            AddTable(_profits);

            var grandTotalList = new List<CostTypeReportEntry>
            {
                new("Suma wydatków", _totalAmounts[0]),
                new("Suma przychodów", _totalAmounts[1])
            };
            AddTable(grandTotalList);

            AddFooter();

            var renderer = new PdfDocumentRenderer
            {
                Document = _document
            };
            renderer.RenderDocument();
            return renderer.PdfDocument;
        }

        void InitializeDocument()
        {
            _document = new();

            _document.Info.Title = "Title";
            _document.Info.Subject = "Subject";
            _document.Info.Author = "Author";
            _document.Info.Keywords = "Keywords";

            var config = _document.AddSection().PageSetup;
            config.Orientation = Orientation.Portrait;
            config.TopMargin = "3cm";
            config.LeftMargin = 15;
            config.BottomMargin = "3cm";
            config.RightMargin = 15;
            config.PageFormat = PageFormat.A4;
            config.OddAndEvenPagesHeaderFooter = true;
            config.StartingNumber = 1;
        }

        void SetupStyles()
        {
            //Header
            var header = _document.Styles[StyleNames.Header];

            //TableHeader
            var tableHeader = _document.Styles.AddStyle("TableHeader", "Normal");
            tableHeader.Font.Size = 20;
            tableHeader.Font.Bold = true;
            tableHeader.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            //TableColumnHeader
            var tableColumnHeader = _document.Styles.AddStyle("TableColumnHeader", "Normal");
            tableColumnHeader.Font.Size = 16;
            tableColumnHeader.Font.Bold = true;

            //TableRowContent
            var tableRowContent = _document.Styles.AddStyle("TableRowContent", "Normal");
            tableRowContent.Font.Size = 12;

            //TableRowSum
            var tableRowSum = _document.Styles.AddStyle("TableRowSum", "TableRowContent");
            tableRowSum.Font.Bold = true;

            //Footer
            var footer = _document.Styles[StyleNames.Footer];
            footer.Font.Italic = true;
            footer.Font.Color = Gray;
        }

        void AddHeader()
        {
            var content = new Paragraph
            {
                Style = StyleNames.Header
            };
            content.AddText("Zestawienie przychodów/wydatków");

            _document.LastSection.Headers.Primary.Add(content);
        }

        void AddReportInfo()
        {
            _document.LastSection.AddParagraph("Informacje o danych w raporcie", "TableHeader");

            var table = _document.LastSection.AddTable();
            table.Rows.HeightRule = MigraDocCore.DocumentObjectModel.Tables.RowHeightRule.Auto;

            var column = table.AddColumn(new Unit(9.5d, UnitType.Centimeter));
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn(new Unit(9.5d, UnitType.Centimeter));
            column.Format.Alignment = ParagraphAlignment.Right;

            var seasonHeader = table.AddRow();
            seasonHeader.Style = "TableRowContent";
            seasonHeader.Cells[0].AddParagraph(_seasons.Count > 1 ? "Sezony:" : "Sezon:");
            seasonHeader.Cells[1].AddParagraph(_seasons.Count == 0 ? "<brak>" : _seasons[0].ToString());
            for (int i = 1; i < _seasons.Count; i++)
            {
                var anotherSeason = table.AddRow();
                anotherSeason.Style = "TableRowContent";
                anotherSeason.Cells[1].AddParagraph(_seasons[i].ToString());
            }

            var cropfieldHeader = table.AddRow();
            cropfieldHeader.Style = "TableRowContent";
            cropfieldHeader.Cells[0].AddParagraph(_cropFields.Count > 1 ? "Pola uprawne:" : "Pole uprawne:");
            cropfieldHeader.Cells[1].AddParagraph(_cropFields.Count == 0 ? "<brak>" : _cropFields[0].ToString());
            for (int i = 1; i < _cropFields.Count; i++)
            {
                var anotherField = table.AddRow();
                anotherField.Style = "TableRowContent";
                anotherField.Cells[1].AddParagraph(_cropFields[i].ToString());
            }
        }

        void AddTable(List<CostTypeReportEntry> tableContents)
        {
            var table = _document.LastSection.AddTable();
            table.Rows.HeightRule = MigraDocCore.DocumentObjectModel.Tables.RowHeightRule.Auto;

            var column = table.AddColumn(new Unit(9.5d, UnitType.Centimeter));
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn(new Unit(9.5d, UnitType.Centimeter));
            column.Format.Alignment = ParagraphAlignment.Right;

            var headerRow = table.AddRow();
            headerRow.Style = "TableColumnHeader";
            headerRow.Cells[0].AddParagraph("Kategoria");
            headerRow.Cells[1].AddParagraph("Suma");

            decimal grandTotal = 0m;
            foreach (CostTypeReportEntry entry in tableContents)
            {
                var contentRow = table.AddRow();
                contentRow.Style = "TableRowContent";
                contentRow.Cells[0].AddParagraph(entry.Name);
                contentRow.Cells[1].AddParagraph($"{entry.Amount:F2}");
                grandTotal += entry.Amount;
            }

            var grandTotalRow = table.AddRow();
            grandTotalRow.Style = "TableRowSum";
            grandTotalRow.Cells[0].AddParagraph("Razem:");
            grandTotalRow.Cells[1].AddParagraph($"{grandTotal:F2}");
        }

        void AddGap()
        {
            var gap = _document.LastSection.AddParagraph();
            gap.AddSpace(10);
        }

        void AddFooter()
        {
            var content = new Paragraph
            {
                Style = StyleNames.Footer
            };
            content.AddText(" Strona ");
            content.AddPageField();
            content.AddText(" z ");
            content.AddNumPagesField();
            content.AddTab();
            content.AddText($"Raport wygenerowano dnia {DateTime.Now:G}. ");
            content.AddText($"Wygenerowano w {AppInfo.Current.Name} w wersji {AppInfo.Current.VersionString}.");

            _document.LastSection.Footers.Primary.Add(content);
        }

        public void AddCropField(CropField cropField) =>
            _cropFields.Add(cropField);

        public void AddSeason(Season season) =>
            _seasons.Add(season);

        public void AddExpenseEntry(CostTypeReportEntry costTypeEntry)
        {
            _totalAmounts[0] += costTypeEntry.Amount;
            _expenses.Add(costTypeEntry);
        }

        public void AddProfitEntry(CostTypeReportEntry costTypeEntry)
        {
            _totalAmounts[1] += costTypeEntry.Amount;
            _profits.Add(costTypeEntry);
        }

        public static async Task Export(PdfDocument document)
        {
            try
            {
                if (!await PermissionManager.RequestPermissionsAsync())
                    return;
                FolderPickerResult folder = await FolderPicker.PickAsync(default);
                if (!folder.IsSuccessful)
                    return;
                document.Save(Path.Combine(folder.Folder.Path, PdfBuilder.Filename));
                App.AlertSvc.ShowAlert("Sukces", $"Raport wyeksportowano do folderu {folder.Folder.Path} z nazwą {PdfBuilder.Filename}.");
            }
            catch (IOException ex)
            {
                ExceptionHandler.Handle(ex, false);
            }
        }
    }

    /// <summary>
    /// Origin of code: <see href="https://stackoverflow.com/questions/10141143/c-sharp-extract-text-from-pdf-using-pdfsharp"/><br/>
    /// Author: <see href="https://stackoverflow.com/users/355899/sergio">Sergio</see>; 
    /// Modified by: <see href="https://stackoverflow.com/users/64334/ronnie-overby">Ronnie Overby</see>.
    /// </summary>
    public static class PdfSharpExtensions
    {
        public static IEnumerable<string> ExtractText(this PdfPage page)
        {
            var content = ContentReader.ReadContent(page);
            var text = content.ExtractText();
            return text;
        }

        public static IEnumerable<string> ExtractText(this CObject cObject)
        {
            if (cObject is COperator)
            {
                var cOperator = cObject as COperator;
                if (cOperator.OpCode.Name == OpCodeName.Tj.ToString() ||
                    cOperator.OpCode.Name == OpCodeName.TJ.ToString())
                {
                    foreach (var cOperand in cOperator.Operands)
                        foreach (var txt in ExtractText(cOperand))
                            yield return txt;
                }
            }
            else if (cObject is CSequence)
            {
                var cSequence = cObject as CSequence;
                foreach (var element in cSequence)
                    foreach (var txt in ExtractText(element))
                        yield return txt;
            }
            else if (cObject is CString)
            {
                var cString = cObject as CString;
                yield return cString.Value;
            }
        }
    }
}
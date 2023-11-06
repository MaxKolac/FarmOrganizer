using FarmOrganizer.Models;
using FarmOrganizer.ViewModels;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Pdf;
using static MigraDocCore.DocumentObjectModel.Colors;

namespace FarmOrganizer.IO.Exporting.PDF
{
    /// <summary>
    /// Responsible for instantiating, creating and then building reports as PDF files.<br/>
    /// First, create a new object instance. Initialize the object with predefined lists right away or use any method with <c>Add</c> prefix to add data to the report. Once ready to be rendered, use <see cref="Build"/> to return a rendered a <see cref="PdfDocument"/> and call <see cref="PdfDocument.Save(Stream)"/> on it to save it as a PDF file.
    /// <para>
    /// Class contains some code originally written by <see href="https://github.com/icebeam7">Luis Beltran</see>. His code was copied and partially modified from his <see href="https://github.com/icebeam7/PDFDemo"> example GitHub repository - PDFDemo</see>
    /// </para>
    /// </summary>
    public class PdfBuilder
    {
        Document _document;
        readonly List<CropField> _cropFields = new();
        readonly List<Season> _seasons = new();
        readonly List<CostTypeReportEntry> _expenses = new();
        readonly List<CostTypeReportEntry> _profits = new();

        /// <summary>
        /// The <see cref="Tuple{T,T}.Item1"/> holds the total sum of expenses, and <see cref="Tuple{T,T}.Item2"/> is the total sum of profits.
        /// </summary>
        Tuple<decimal, decimal> totalAmounts = new(0, 0);

        /// <summary>
        /// The name of the application, which will show up in the footer of the document<br/>
        /// This needs to be passed from outside the <see cref="PdfBuilder"/> class. 
        /// Directly putting <see cref="AppInfo"/> properties causes an exception when running tests in FarmOrganizerTests, because that project doesn't have a reference to .NET MAUI in its assembly. Which is where <see cref="AppInfo"/> properties, such as <see cref="AppInfo.Name"/> and <see cref="AppInfo.VersionString"/>, come from.
        /// </summary>
        public string AppName { get; init; }
        /// <summary>
        /// The string representation of the current application's version.<br/>
        /// This needs to be passed from outside the <see cref="PdfBuilder"/> class. 
        /// Directly putting <see cref="AppInfo"/> properties causes an exception when running tests in FarmOrganizerTests, because that project doesn't have a reference to .NET MAUI in its assembly. Which is where <see cref="AppInfo"/> properties, such as <see cref="AppInfo.Name"/> and <see cref="AppInfo.VersionString"/>, come from.
        /// </summary>
        public string AppVersion { get; init; }

        public static string Filename { get { return $"Raport_{DateTime.Now:dd.MM.yy_HH.mm.ss}.pdf"; } }

        /// <summary>
        /// Creates a new <see cref="PdfBuilder"/> with all its lists empty.
        /// </summary>
        /// <param name="appName"><see cref="AppName"/></param>
        /// <param name="version"><see cref="AppVersion"/></param>
        public PdfBuilder(string appName, string version)
        {
            AppName = appName;
            AppVersion = version;
            InitializeDocument();
        }

        /// <summary>
        /// Creates a new <see cref="PdfBuilder"/> with its lists containing predefined entries.
        /// </summary>
        /// <param name="appName"><see cref="AppName"/></param>
        /// <param name="version"><see cref="AppVersion"/></param>
        /// <param name="cropFields"><see cref="CropField"/> entries to add.</param>
        /// <param name="seasons"><see cref="Season"/> entries to add.</param>
        /// <param name="expenses"><see cref="CostType"/> entries to add under Expenses section.</param>
        /// <param name="profits"><see cref="CostType"/> entries to add under Profits section.</param>
        public PdfBuilder(string appName,
                          string version,
                          List<CropField> cropFields,
                          List<Season> seasons,
                          List<CostTypeReportEntry> expenses,
                          List<CostTypeReportEntry> profits) : this(appName, version)
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
        /// Creates a new blank document, runs a sequence of methods which populate it with content, applies styles to it and finally renders it.
        /// </summary>
        /// <returns>
        /// A rendered <see cref="PdfDocument"/>. 
        /// </returns>
        public PdfDocument Build()
        {
            //things to try to narrow down the exact exception cause:
            //Does it happen if we comment out all building methosd except InitializeDcoument and SetupSTyles?
            //It doesnt, no exception thrown

            //Does it happen when we comment out AddReportInfo and all AddTable calls?
            //It doesn't, no exception thrown - AddHeader, AddGap and AddFooter are clear

            //Does it happen when we comment out all AddTable method calls and leave AddReportInfo?
            //Yep, the exact exception is thrown - AddReportInfo is an offender

            //Does it happen when we comment out all AddGap, AddTable method calls and leave AddReportInfo?
            //Yes, AddReportInfo is 100% an offender

            //Does it happen if all that is left is AddTables?
            //yes, AddTables is 100% an offender

            //Does it happen after fully disabling any trimming (this includes disabling AOT compilation)?
            //There it is - it is trimming fault

            InitializeDocument();
            SetupStyles();

            AddHeader();
            AddReportInfo();
            AddGap();
            AddTable(_expenses, "Wydatki");
            AddGap();
            AddTable(_profits, "Przychody");
            AddGap();

            var grandTotalList = new List<CostTypeReportEntry>
            {
                new("Suma wydatków", totalAmounts.Item1 * -1),
                new("Suma przychodów", totalAmounts.Item2)
            };
            AddTable(grandTotalList, totalAmounts.Item1 <= totalAmounts.Item2 ? "Zyski" : "Straty");

            AddFooter();

            var renderer = new PdfDocumentRenderer(true)
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
            config.PageFormat = PageFormat.A4;
            config.TopMargin = new Unit(3, UnitType.Centimeter);
            config.LeftMargin = new Unit(1.5, UnitType.Centimeter);
            config.BottomMargin = new Unit(3, UnitType.Centimeter);
            config.RightMargin = new Unit(1.5, UnitType.Centimeter);
            config.StartingNumber = 1;
        }

        void SetupStyles()
        {
            //Header
            var header = _document.Styles[StyleNames.Header];
            header.Font.Size = 16;
            header.Font.Bold = true;
            header.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            //TableHeader
            var tableHeader = _document.Styles.AddStyle("TableHeader", "Normal");
            tableHeader.Font.Size = 14;
            tableHeader.Font.Bold = true;
            tableHeader.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            //TableRowContent
            var tableRowContent = _document.Styles.AddStyle("TableRowContent", "Normal");
            tableRowContent.Font.Size = 12;

            //TableRowBold
            var tableRowBold = _document.Styles.AddStyle("TableRowBold", "TableRowContent");
            tableRowBold.Font.Bold = true;

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
            content.AddText("Zestawienie przychodów i wydatków");

            _document.LastSection.Headers.Primary.Add(content);
        }

        void AddReportInfo()
        {
            _document.LastSection.AddParagraph("Informacje o raporcie", "TableHeader");

            var table = _document.LastSection.AddTable();
            table.Rows.HeightRule = MigraDocCore.DocumentObjectModel.Tables.RowHeightRule.Auto;

            var column = table.AddColumn(new Unit(9, UnitType.Centimeter));
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn(new Unit(9, UnitType.Centimeter));
            column.Format.Alignment = ParagraphAlignment.Right;

            var seasonHeader = table.AddRow();
            seasonHeader.Style = "TableRowContent";
            seasonHeader.Cells[0].AddParagraph(_seasons.Count > 1 ? "Sezony:" : "Sezon:");
            seasonHeader.Cells[1].AddParagraph(_seasons.Count == 0 ? "<brak>" : _seasons[0].ToString());
            seasonHeader.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            for (int i = 1; i < _seasons.Count; i++)
            {
                var anotherSeason = table.AddRow();
                anotherSeason.Style = "TableRowContent";
                anotherSeason.Cells[1].AddParagraph(_seasons[i].ToString());
                anotherSeason.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            }

            var cropfieldHeader = table.AddRow();
            cropfieldHeader.Style = "TableRowContent";
            cropfieldHeader.Cells[0].AddParagraph(_cropFields.Count > 1 ? "Pola uprawne:" : "Pole uprawne:");
            cropfieldHeader.Cells[1].AddParagraph(_cropFields.Count == 0 ? "<brak>" : _cropFields[0].ToString());
            cropfieldHeader.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            for (int i = 1; i < _cropFields.Count; i++)
            {
                var anotherField = table.AddRow();
                anotherField.Style = "TableRowContent";
                anotherField.Cells[1].Format.Alignment = ParagraphAlignment.Right;
                anotherField.Cells[1].AddParagraph(_cropFields[i].ToString());
            }
        }

        void AddTable(List<CostTypeReportEntry> tableContents, string tableHeaderText)
        {
            if (!string.IsNullOrEmpty(tableHeaderText))
                _document.LastSection.AddParagraph(tableHeaderText, "TableHeader");

            var table = _document.LastSection.AddTable();
            table.Rows.HeightRule = MigraDocCore.DocumentObjectModel.Tables.RowHeightRule.Auto;

            var column = table.AddColumn(new Unit(9, UnitType.Centimeter));
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn(new Unit(9, UnitType.Centimeter));
            column.Format.Alignment = ParagraphAlignment.Right;

            var headerRow = table.AddRow();
            headerRow.Style = "TableRowBold";
            headerRow.Cells[0].AddParagraph("Kategoria");
            headerRow.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            headerRow.Cells[1].AddParagraph("Suma");

            decimal grandTotal = 0m;
            foreach (CostTypeReportEntry entry in tableContents)
            {
                var contentRow = table.AddRow();
                contentRow.Style = "TableRowContent";
                contentRow.Cells[0].AddParagraph(entry.Name);
                contentRow.Cells[1].Format.Alignment = ParagraphAlignment.Right;
                contentRow.Cells[1].AddParagraph($"{entry.Amount:F2} zł");
                grandTotal += entry.Amount;
            }

            var grandTotalRow = table.AddRow();
            grandTotalRow.Style = "TableRowBold";
            grandTotalRow.Cells[0].AddParagraph("Razem:");
            grandTotalRow.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            grandTotalRow.Cells[1].AddParagraph($"{grandTotal:F2} zł");
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
            content.AddText($"Wygenerowano w {AppName} w wersji {AppVersion}.");

            _document.LastSection.Footers.Primary.Add(content);
        }

        public void AddCropField(CropField cropField) =>
            _cropFields.Add(cropField);

        public void AddSeason(Season season) =>
            _seasons.Add(season);

        public void AddExpenseEntry(CostTypeReportEntry costTypeEntry)
        {
            totalAmounts = new(totalAmounts.Item1 + costTypeEntry.Amount, totalAmounts.Item2);
            _expenses.Add(costTypeEntry);
        }

        public void AddProfitEntry(CostTypeReportEntry costTypeEntry)
        {
            totalAmounts = new(totalAmounts.Item1, totalAmounts.Item2 + costTypeEntry.Amount);
            _profits.Add(costTypeEntry);
        }
    }
}
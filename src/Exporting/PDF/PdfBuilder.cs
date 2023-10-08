using Android.Graphics.Pdf;
using FarmOrganizer.Models;
using FarmOrganizer.ViewModels.HelperClasses;

namespace FarmOrganizer.Exporting.PDF
{
    public class PdfBuilder
    {
        private string _cropFieldsLabel = "Pole uprawne:";
        private readonly List<CropField> _cropFields = new();
        private string _seasonsLabel = "Sezon:";
        private readonly List<Season> _seasons = new();
        private readonly List<CostTypeGroup> _expenses = new();
        private readonly List<CostTypeGroup> _profits = new();

        public PdfBuilder()
        {
        }

        public PdfDocument Build()
        {
            throw new NotImplementedException();
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

        public void AddExpense(CostTypeGroup costTypeGroup) => _expenses.Add(costTypeGroup);
        public void AddProfit(CostTypeGroup costTypeGroup) => _profits.Add(costTypeGroup);

    }
}

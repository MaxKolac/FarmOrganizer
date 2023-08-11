using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FarmOrganizer.ViewModels
{
    public partial class QuickCalculatorViewModel : ObservableObject
    {
        [ObservableProperty]
        public bool cropAmountReadOnly = false;
        [ObservableProperty]
        public string cropAmountValue;
        [ObservableProperty]
        public bool sellRateReadOnly = false;
        [ObservableProperty]
        public string sellRateValue;
        [ObservableProperty]
        public bool pureIncomeReadOnly = true;
        [ObservableProperty]
        public string pureIncomeValue;

        private readonly Queue<string> lastEditedControls = new();

        public QuickCalculatorViewModel()
        {
            lastEditedControls.Enqueue("sellRate");
            lastEditedControls.Enqueue("cropAmount");
        }

        [RelayCommand]
        void TextChanged()
        {
            double CropAmount = Utils.CastToValue(CropAmountValue);
            double SellRate = Utils.CastToValue(SellRateValue);
            double PureIncome = Utils.CastToValue(PureIncomeValue);

            if (CropAmountReadOnly)
            {
                CropAmountValue = (PureIncome / SellRate).ToString();
            }
            else if (SellRateReadOnly)
            {
                SellRateValue = (PureIncome / CropAmount).ToString();
            }
            else if (PureIncomeReadOnly)
            {
                PureIncomeValue = (CropAmount * SellRate).ToString();
            }
        }

        [RelayCommand]
        void LastTappedControlsChanged(string caller)
        {
            lastEditedControls.Enqueue(caller);
            lastEditedControls.Dequeue();

            CropAmountReadOnly = !lastEditedControls.Contains("cropAmount");
            SellRateReadOnly = !lastEditedControls.Contains("sellRate");
            PureIncomeReadOnly = !lastEditedControls.Contains("pureIncome");
        }
    }
}

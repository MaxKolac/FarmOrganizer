using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FarmOrganizer.ViewModels
{
    public partial class QuickCalculatorViewModel : ObservableObject
    {
        private readonly Color defaultColor = Color.FromRgb(255, 255, 255);
        private readonly Color selectedColor = Color.FromRgb(166, 158, 45);

        private const string cropAmountName = "cropAmount";
        [ObservableProperty]
        public string cropAmountValue;
        [ObservableProperty]
        public Color cropAmountColor;

        private const string sellRateName = "sellRate";
        [ObservableProperty]
        public string sellRateValue;
        [ObservableProperty]
        public Color sellRateColor;

        private const string pureIncomeName = "pureIncome";
        [ObservableProperty]
        public string pureIncomeValue;
        [ObservableProperty]
        public Color pureIncomeColor;

        protected readonly Queue<string> lastEditedControls = new();

        public QuickCalculatorViewModel()
        {
            lastEditedControls.Enqueue(cropAmountName);
            lastEditedControls.Enqueue(sellRateName);
            CropAmountColor = selectedColor;
            SellRateColor = selectedColor;
        }

        [RelayCommand]
        protected void TextChanged()
        {
            double CropAmount = Utils.CastToValue(CropAmountValue);
            double SellRate = Utils.CastToValue(SellRateValue);
            double PureIncome = Utils.CastToValue(PureIncomeValue);

            if (lastEditedControls.Contains(pureIncomeName) && lastEditedControls.Contains(sellRateName))
            {
                CropAmountValue = (PureIncome / SellRate).ToString();
            }
            else if (lastEditedControls.Contains(pureIncomeName) && lastEditedControls.Contains(cropAmountName))
            {
                SellRateValue = (PureIncome / CropAmount).ToString();
            }
            else if (lastEditedControls.Contains(cropAmountName) && lastEditedControls.Contains(sellRateName))
            {
                PureIncomeValue = (CropAmount * SellRate).ToString();
            }
        }

        [RelayCommand]
        protected void LastTappedControlsChanged(string caller)
        {
            //TODO: backgroudn colors refuse to change once they turn yello
            if (lastEditedControls.Contains(caller))
                return;
            lastEditedControls.Enqueue(caller);
            switch (lastEditedControls.Dequeue())
            {
                case cropAmountName:
                    CropAmountColor = defaultColor;
                    break;
                case sellRateName:
                    SellRateColor = defaultColor;
                    break;
                case pureIncomeName:
                    PureIncomeColor = defaultColor;
                    break;
            }
            switch (caller)
            {
                case cropAmountName:
                    CropAmountColor = selectedColor;
                    break;
                case sellRateName:
                    SellRateColor = selectedColor;
                    break;
                case pureIncomeName:
                    PureIncomeColor = selectedColor;
                    break;
            }
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FarmOrganizer.ViewModels
{
    public partial class QuickCalculatorViewModel : ObservableObject
    {
        #region Standard Inheritable Fields
        protected const string cropAmountName = "cropAmount";
        [ObservableProperty]
        private string cropAmountValue;
        [ObservableProperty]
        protected bool cropAmountFocused;

        protected const string sellRateName = "sellRate";
        [ObservableProperty]
        private string sellRateValue;
        [ObservableProperty]
        protected bool sellRateFocused;

        protected const string pureIncomeName = "pureIncome";
        [ObservableProperty]
        private string pureIncomeValue;
        [ObservableProperty]
        protected bool pureIncomeFocused;

        protected readonly Queue<string> lastEditedControls = new();
        #endregion

        #region Additional Private Fields
        [ObservableProperty]
        private string exampleExpenseValue;
        [ObservableProperty]
        private string exampleChangeValue;
        [ObservableProperty]
        private string exampleChangeText = _exampleChangeProfitText;
        private const string _exampleChangeProfitText = "Przykładowy zysk (zł):";
        private const string _exampleChangeLossText = "Przykładowe straty (zł):";
        #endregion 

        #region VAT-RR Calculator

        #endregion

        public QuickCalculatorViewModel()
        {
            lastEditedControls.Enqueue(cropAmountName);
            lastEditedControls.Enqueue(sellRateName);
            cropAmountFocused = sellRateFocused = true;
        }

        [RelayCommand]
        protected void TextChanged()
        {
            decimal CropAmount = Utils.CastToValue(CropAmountValue);
            decimal SellRate = Utils.CastToValue(SellRateValue);
            decimal PureIncome = Utils.CastToValue(PureIncomeValue);

            if (lastEditedControls.Contains(pureIncomeName) && lastEditedControls.Contains(sellRateName))
            {
                CropAmount = SellRate != 0 ? decimal.Divide(PureIncome, SellRate) : 0;
                string CropAmountFormatted = CropAmount.ToString("0.00");
                CropAmountValue = CropAmountFormatted.Length > Globals.NumericEntryMaxLength ? maxLengthExceededMessage : CropAmountFormatted;
            }
            else if (lastEditedControls.Contains(pureIncomeName) && lastEditedControls.Contains(cropAmountName))
            {
                SellRate = CropAmount != 0 ? decimal.Divide(PureIncome, CropAmount) : 0;
                string SellRateFormatted = SellRate.ToString("0.00");
                SellRateValue = SellRateFormatted.Length > Globals.NumericEntryMaxLength ? maxLengthExceededMessage : SellRateFormatted;
            }
            else if (lastEditedControls.Contains(cropAmountName) && lastEditedControls.Contains(sellRateName))
            {
                PureIncome = decimal.Multiply(CropAmount, SellRate);
                string PureIncomeFormatted = PureIncome.ToString("0.00");
                PureIncomeValue = PureIncomeFormatted.Length > Globals.NumericEntryMaxLength ? maxLengthExceededMessage : PureIncomeFormatted;
            }
        }

        [RelayCommand]
        protected void LastTappedControlsChanged(string caller)
        {
            if (lastEditedControls.Contains(caller))
                return;
            lastEditedControls.Enqueue(caller);
            lastEditedControls.Dequeue();
            CropAmountFocused = lastEditedControls.Contains(cropAmountName);
            SellRateFocused = lastEditedControls.Contains(sellRateName);
            PureIncomeFocused = lastEditedControls.Contains(pureIncomeName);
        }

        private void CalculateExampleChange()
        {
            decimal expenses = Utils.CastToValue(ExampleExpenseValue);
            decimal profits = Utils.CastToValue(PureIncomeValue);
            ExampleChangeValue = (profits - expenses).ToString("0.00");
        }

        partial void OnCropAmountValueChanged(string value) =>
            TextChanged();

        partial void OnSellRateValueChanged(string value) =>
            TextChanged();

        partial void OnPureIncomeValueChanged(string value)
        {
            TextChanged();
            OnIncomeChanged(value);
            CalculateExampleChange();
        }

        partial void OnExampleExpenseValueChanged(string oldValue, string newValue) =>
            CalculateExampleChange();

        partial void OnExampleChangeValueChanged(string value) =>
            ExampleChangeText = Utils.CastToValue(value) >= 0 ? _exampleChangeProfitText : _exampleChangeLossText;

        //Ugly workarounds. Yuck. It works though!
        protected virtual void OnIncomeChanged(string value) { }

    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.ViewModels.HelperClasses;

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
                CropAmountValue = SellRate != 0 ? (PureIncome / SellRate).ToString("0.00") : "0";
            }
            else if (lastEditedControls.Contains(pureIncomeName) && lastEditedControls.Contains(cropAmountName))
            {
                SellRateValue = CropAmount != 0 ? (PureIncome / CropAmount).ToString("0.00") : "0";
            }
            else if (lastEditedControls.Contains(cropAmountName) && lastEditedControls.Contains(sellRateName))
            {
                PureIncomeValue = (CropAmount * SellRate).ToString("0.00");
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

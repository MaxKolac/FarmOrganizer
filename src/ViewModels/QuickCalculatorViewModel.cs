using CommunityToolkit.Mvvm.ComponentModel;

namespace FarmOrganizer.ViewModels
{
    public partial class QuickCalculatorViewModel : ObservableObject
    {
        [ObservableProperty]
        decimal pureIncome;

        [ObservableProperty]
        string exampleExpenseValue;
        [ObservableProperty]
        string exampleChangeValue;
        [ObservableProperty]
        string exampleChangeText = _exampleChangeProfitText;
        const string _exampleChangeProfitText = "Przykładowy zysk (zł):";
        const string _exampleChangeLossText = "Przykładowe straty (zł):";

        #region VAT-RR Calculator

        #endregion

        private void CalculateExampleChange() => 
            ExampleChangeValue = (PureIncome - Utils.CastToValue(ExampleExpenseValue)).ToString("0.00");

        partial void OnPureIncomeChanged(decimal value) => 
            CalculateExampleChange();

        partial void OnExampleExpenseValueChanged(string value) =>
            CalculateExampleChange();

        partial void OnExampleChangeValueChanged(string value) =>
            ExampleChangeText = Utils.CastToValue(value) >= 0 ? _exampleChangeProfitText : _exampleChangeLossText;
    }
}

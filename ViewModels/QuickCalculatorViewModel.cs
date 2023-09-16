using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.ViewModels.HelperClasses;

namespace FarmOrganizer.ViewModels
{
    public partial class QuickCalculatorViewModel : ObservableObject
    {
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
                CropAmountValue = SellRate != 0 ? (PureIncome / SellRate).ToString() : "0";
            }
            else if (lastEditedControls.Contains(pureIncomeName) && lastEditedControls.Contains(cropAmountName))
            {
                SellRateValue = CropAmount != 0 ? (PureIncome / CropAmount).ToString() : "0";
            }
            else if (lastEditedControls.Contains(cropAmountName) && lastEditedControls.Contains(sellRateName))
            {
                PureIncomeValue = (CropAmount * SellRate).ToString();
            }
        }

        [RelayCommand]
        protected void LastTappedControlsChanged(string caller)
        {
            //int maui_entries_suck = 0;
            //Increment this value if you ever feel like trying to implement the whole idea of dynamic background colors on entries, and (shock!) it doesn't work.

            //Upon debugging at runtime, these values are modified correctly, but once the
            //app is allowed to continue execution, SOMETHING is changing it back to (1,1,1,1)
            //Task.Delaying and Async keywords didnt do anything
            //VisualStates of Normal and Focus also dont fully work, one time Normal only works, the other Focused only works

            //UPDATE: I have had enough of this Entry's bullcrap... I'm resorting to just showing which label will be used in calculation by displaying a disabled Checkbox next to it. Looks prettier imo.

            if (lastEditedControls.Contains(caller))
                return;
            lastEditedControls.Enqueue(caller);
            lastEditedControls.Dequeue();
            CropAmountFocused = lastEditedControls.Contains(cropAmountName);
            SellRateFocused = lastEditedControls.Contains(sellRateName);
            PureIncomeFocused = lastEditedControls.Contains(pureIncomeName);

            //CropAmountColor = SellRateColor = PureIncomeColor = Colors.White;
            //if (lastEditedControls.Contains(cropAmountName))
            //    CropAmountColor = selectedColor;
            //if (lastEditedControls.Contains(sellRateName))
            //    SellRateColor = selectedColor;
            //if (lastEditedControls.Contains(pureIncomeName))
            //    SellRateColor = selectedColor;

            //The below stuff was used while debugging and losing sanity
            //CropAmountColor = SellRateColor = PureIncomeColor = Colors.White;
            //if (caller.Equals(cropAmountName))
            //    CropAmountColor = selectedColor;
            //if (caller.Equals(sellRateName))
            //    SellRateColor = selectedColor;
            //if (caller.Equals(pureIncomeName))
            //    SellRateColor = selectedColor;
        }

        //Class over-coupling? Never heard of it *shrug*
        //I blame partial methods not being inherited
        partial void OnPureIncomeValueChanged(string value) =>
            OnIncomeChanged(value);

        protected virtual void OnIncomeChanged(string value) { }
    }
}

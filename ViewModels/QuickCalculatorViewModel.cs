using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
            float CropAmount = CastToValue(CropAmountValue);
            float SellRate = CastToValue(SellRateValue);
            float PureIncome = CastToValue(PureIncomeValue);

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

        /// <summary>
        /// <para>Android's Numeric Keyboard has the period hardcoded into it and C#'s <see cref="float.TryParse(string?, out float)">float.TryParse</see> breaks when it receives a string with a dot, instead of a comma. This allows the number to have a comma from a previous calculation and a dot from the keyboard, which breaks calculations of the third value.</para>
        /// <para>This method is a workaround of this problem. Despite it being overly engineered and probably in need of refactoring, at the moment it's the only solution which works and can handle both commas and dots. It can even verify and modify a number with both a comma and dot to be correct.</para>
        /// </summary>
        /// <returns>A floating point value of the input string. If the input is not a valid number, 0 is returned instead.</returns>
        private float CastToValue(string input)
        {
            //im gonna loose my god damn mind with this goofy ass keyboard
            //the comma is literally RIGHT there, its just greyed out, god dammit
            if (input is null)
                return 0;

            List<char> inputChars = new();
            inputChars.AddRange(input.ToCharArray());
            bool commaDetected = false;
            for (int i = 0; i < inputChars.Count; i++)
            {
                char c = inputChars[i];
                if (c == '.')
                {
                    c = ',';
                }
                
                if (c == ',')
                {
                    if (commaDetected)
                    {
                        inputChars.RemoveAt(i);
                    }
                    commaDetected = true;
                }
            }

            StringBuilder builder = new();
            foreach (char c in inputChars)
                builder.Append(c);
            string sanitazitedString = builder.ToString();

            //https://stackoverflow.com/questions/29452263/make-tryparse-compatible-with-comma-or-dot-decimal-separator
            var cultureInfo = CultureInfo.InvariantCulture;
            // if the first regex matches, the number string is in us culture
            if (Regex.IsMatch(sanitazitedString, @"^(:?[\d,]+\.)*\d+$"))
            {
                cultureInfo = new CultureInfo("en-US");
            }
            // if the second regex matches, the number string is in de culture
            else if (Regex.IsMatch(sanitazitedString, @"^(:?[\d.]+,)*\d+$"))
            {
                cultureInfo = new CultureInfo("de-DE");
            }
            NumberStyles styles = NumberStyles.Number;
            return float.TryParse(sanitazitedString, styles, cultureInfo, out float value) ? value : 0;
        }
    }
}

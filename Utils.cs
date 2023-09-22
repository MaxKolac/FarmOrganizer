using System.Text;

namespace FarmOrganizer
{
    public static class Utils
    {
        /// <summary>
        /// <para>Android's Numeric Keyboard has the period hardcoded into it and C#'s <see cref="decimal.TryParse(string, out decimal)">decimal.TryParse</see> breaks when it receives a string with a period, instead of a comma. Under a few circumstances, it's possible for the input value to even contain both a period and a comma.</para>
        /// <para>This method converts all input numbers to only include the first left-most separator, be it either comma or period. If the separator happened to be a period, it's swapped out for a comma</para>
        /// </summary>
        /// <returns>A floating point value of the input string. If the input is not a valid number, 0 is returned instead.</returns>
        public static decimal CastToValue(string input)
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
                if (inputChars[i] == '.')
                {
                    inputChars[i] = ',';
                }

                if (inputChars[i] == ',')
                {
                    if (commaDetected)
                    {
                        inputChars.RemoveAt(i);
                        i--;
                    }
                    commaDetected = true;
                }
            }

            StringBuilder builder = new();
            foreach (char c in inputChars)
                builder.Append(c);
            string sanitazitedString = builder.ToString();
            return decimal.TryParse(sanitazitedString, out decimal result) ? result : 0;
        }
    }
}
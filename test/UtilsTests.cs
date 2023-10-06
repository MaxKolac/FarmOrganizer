namespace FarmOrganizerTests
{
    public class UtilsTests
    {
        [Theory]
        [InlineData(new object[] { "123,123", 123.123 })]
        [InlineData(new object[] { "123,123.123", 123.123123 })]
        [InlineData(new object[] { "123,12.3.123", 123.123123 })]
        [InlineData(new object[] { "1.23,1.23", 1.23123 })]
        [InlineData(new object[] { "000123,123", 123.123 })]
        [InlineData(new object[] { "0.00123,123", 0.00123123 })]
        public void CastToValueTests(string input, decimal expectedOutput)
        {
            var result = FarmOrganizer.Utils.CastToValue(input);

            Assert.Equal(result, expectedOutput);
        }
    }
}
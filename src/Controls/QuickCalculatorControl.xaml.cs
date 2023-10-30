namespace FarmOrganizer.Controls;

public partial class QuickCalculatorControl : ContentView
{
    readonly Queue<Entry> _lastTappedEntries = new();

    public decimal CropAmount
    {
        get => (decimal)GetValue(CropAmountProperty);
        set => SetValue(CropAmountProperty, value);
    }
    
    public decimal SellRate
    {
        get => (decimal)GetValue(SellRateProperty);
        set => SetValue(SellRateProperty, value);
    }
    
    public decimal PureIncome
    {
        get => (decimal)GetValue(PureIncomeProperty);
        set => SetValue(PureIncomeProperty, value);
    }

    public static readonly BindableProperty CropAmountProperty = BindableProperty.Create(nameof(CropAmount), typeof(decimal), typeof(QuickCalculatorControl), 0m, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
    {
        var control = bindable as QuickCalculatorControl;
        control.cropAmountEntry.Text = ((decimal)newValue).ToString("0.00");
    });
    public static readonly BindableProperty SellRateProperty = BindableProperty.Create(nameof(SellRate), typeof(decimal), typeof(QuickCalculatorControl), 0m, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
    {
        var control = bindable as QuickCalculatorControl;
        control.sellRateEntry.Text = ((decimal)newValue).ToString("0.00");
    });
    public static readonly BindableProperty PureIncomeProperty = BindableProperty.Create(nameof(PureIncome), typeof(decimal), typeof(QuickCalculatorControl), 0m, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
    {
        var control = bindable as QuickCalculatorControl;
        control.pureIncomeEntry.Text = ((decimal)newValue).ToString("0.00");
    });

	public QuickCalculatorControl()
	{
		InitializeComponent();
        _lastTappedEntries.Enqueue(cropAmountEntry);
        _lastTappedEntries.Enqueue(sellRateEntry);
	}

    void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        decimal cropAmount = Utils.CastToValue(cropAmountEntry.Text);
        decimal sellRate = Utils.CastToValue(sellRateEntry.Text);
        decimal pureIncome = Utils.CastToValue(pureIncomeEntry.Text);

        if (_lastTappedEntries.Contains(cropAmountEntry) && _lastTappedEntries.Contains(sellRateEntry))
        {
            pureIncome = decimal.Multiply(cropAmount, sellRate);
            string pureIncomeFormatted = pureIncome.ToString("0.00");
            pureIncomeEntry.Text = 
                pureIncomeFormatted.Length > Globals.NumericEntryMaxLength ? 
                Globals.NumericEntryMaxLengthExceeded: 
                pureIncomeFormatted;
        }
        else if (_lastTappedEntries.Contains(sellRateEntry) && _lastTappedEntries.Contains(pureIncomeEntry))
        {
            cropAmount = sellRate != 0m ? decimal.Divide(pureIncome, sellRate) : 0;
            string cropAmountFormatted = cropAmount.ToString("0.00");
            cropAmountEntry.Text = 
                cropAmountFormatted.Length > Globals.NumericEntryMaxLength ?
                Globals.NumericEntryMaxLengthExceeded :
                cropAmountFormatted;
        }
        else if (_lastTappedEntries.Contains(pureIncomeEntry) && _lastTappedEntries.Contains(cropAmountEntry))
        {
            sellRate = cropAmount != 0 ? decimal.Divide(pureIncome, cropAmount) : 0;
            string sellRateFormatted = sellRate.ToString("0.00");
            sellRateEntry.Text = 
                sellRateFormatted.Length > Globals.NumericEntryMaxLength ?
                Globals.NumericEntryMaxLengthExceeded : 
                sellRateFormatted;
        }

        CropAmount = cropAmount;
        SellRate = sellRate;
        PureIncome = pureIncome;
    }

    void OnEntryTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Entry tappedEntry || _lastTappedEntries.Contains(tappedEntry)) return;

        _lastTappedEntries.Enqueue(tappedEntry);
        _lastTappedEntries.Dequeue();
        cropAmountFocusIndicator.IsChecked = _lastTappedEntries.Contains(cropAmountEntry);
        sellRateFocusIndicator.IsChecked = _lastTappedEntries.Contains(sellRateEntry);
        pureIncomeFocusIndicator.IsChecked = _lastTappedEntries.Contains(pureIncomeEntry);
    }
}
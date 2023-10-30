namespace FarmOrganizer.Controls;

public partial class QuickCalculatorControl : ContentView
{
    const string _maxLengthExceededMessage = "Za du¿o";
    const string _cropAmountKey = "cropAmount";
    const string _sellRateKey = "sellRate";
    const string _pureIncomeKey = "pureIncome";
    readonly Queue<Entry> _lastFocusedQueue = new();

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

    });
    public static readonly BindableProperty SellRateProperty = BindableProperty.Create(nameof(SellRate), typeof(decimal), typeof(QuickCalculatorControl), 0m, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
    {

    });
    public static readonly BindableProperty PureIncomeProperty = BindableProperty.Create(nameof(PureIncome), typeof(decimal), typeof(QuickCalculatorControl), 0m, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
    {

    });

	public QuickCalculatorControl()
	{
		InitializeComponent();
        _lastFocusedQueue.Enqueue(cropAmountEntry);
        cropAmountFocusIndicator.IsChecked = true;
        _lastFocusedQueue.Enqueue(sellRateEntry);
        sellRateFocusIndicator.IsChecked = true;
	}

    void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        decimal cropAmount = Utils.CastToValue(cropAmountEntry.Text);
        decimal sellRate = Utils.CastToValue(sellRateEntry.Text);
        decimal pureIncome = Utils.CastToValue(pureIncomeEntry.Text);

        if (_lastFocusedQueue.Contains(cropAmountEntry) && _lastFocusedQueue.Contains(sellRateEntry))
        {
            pureIncome = decimal.Multiply(cropAmount, sellRate);
            string pureIncomeFormatted = pureIncome.ToString("0.00");
            pureIncomeEntry.Text = pureIncomeFormatted.Length > Globals.NumericEntryMaxLength ? _maxLengthExceededMessage : pureIncomeFormatted;
        }
        else if (_lastFocusedQueue.Contains(sellRateEntry) && _lastFocusedQueue.Contains(pureIncomeEntry))
        {
            cropAmount = sellRate != 0m ? decimal.Divide(pureIncome, sellRate) : 0;
            string cropAmountFormatted = cropAmount.ToString("0.00");
            cropAmountEntry.Text = cropAmountFormatted.Length > Globals.NumericEntryMaxLength ? _maxLengthExceededMessage : cropAmountFormatted;
        }
        else if (_lastFocusedQueue.Contains(pureIncomeEntry) && _lastFocusedQueue.Contains(cropAmountEntry))
        {
            sellRate = cropAmount != 0 ? decimal.Divide(pureIncome, cropAmount) : 0;
            string sellRateFormatted = sellRate.ToString("0.00");
            sellRateEntry.Text = sellRateFormatted.Length > Globals.NumericEntryMaxLength ? _maxLengthExceededMessage : sellRateFormatted;
        }

        CropAmount = cropAmount;
        SellRate = sellRate;
        PureIncome = pureIncome;
    }

    void OnEntryTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Entry tappedEntry || _lastFocusedQueue.Contains(tappedEntry))
        {
            return;
        }

        _lastFocusedQueue.Enqueue(tappedEntry);
        _lastFocusedQueue.Dequeue();
        cropAmountFocusIndicator.IsChecked = _lastFocusedQueue.Contains(cropAmountEntry);
        sellRateFocusIndicator.IsChecked = _lastFocusedQueue.Contains(sellRateEntry);
        pureIncomeFocusIndicator.IsChecked = _lastFocusedQueue.Contains(pureIncomeEntry);
    }
}
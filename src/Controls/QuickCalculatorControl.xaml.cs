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
        var entry = control.cropAmountEntry;

        if (!entry.IsFocused)
        {
            var formattedValue = ((decimal)newValue).ToString("0.00");
            entry.Text =
                formattedValue.Length > Globals.NumericEntryMaxLength ?
                Globals.NumericEntryMaxLengthExceeded :
                formattedValue;
        }
    });

    public static readonly BindableProperty SellRateProperty = BindableProperty.Create(nameof(SellRate), typeof(decimal), typeof(QuickCalculatorControl), 0m, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
    {
        var control = bindable as QuickCalculatorControl;
        var entry = control.sellRateEntry;

        if (!entry.IsFocused)
        {
            var formattedValue = ((decimal)newValue).ToString("0.00");
            entry.Text =
                formattedValue.Length > Globals.NumericEntryMaxLength ?
                Globals.NumericEntryMaxLengthExceeded :
                formattedValue;
        }
    });

    public static readonly BindableProperty PureIncomeProperty = BindableProperty.Create(nameof(PureIncome), typeof(decimal), typeof(QuickCalculatorControl), 0m, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
    {
        var control = bindable as QuickCalculatorControl;
        var entry = control.pureIncomeEntry;

        if (!entry.IsFocused)
        {
            var formattedValue = ((decimal)newValue).ToString("0.00");
            entry.Text = 
                formattedValue.Length > Globals.NumericEntryMaxLength ?
                Globals.NumericEntryMaxLengthExceeded : 
                formattedValue;
        }
    });

    public QuickCalculatorControl()
    {
        InitializeComponent();
        _lastTappedEntries.Enqueue(cropAmountEntry);
        _lastTappedEntries.Enqueue(sellRateEntry);
    }

    void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue == Globals.NumericEntryMaxLengthExceeded) return;

        CropAmount = Utils.CastToValue(cropAmountEntry.Text);
        SellRate = Utils.CastToValue(sellRateEntry.Text);
        PureIncome = Utils.CastToValue(pureIncomeEntry.Text);

        if (_lastTappedEntries.Contains(cropAmountEntry) && _lastTappedEntries.Contains(sellRateEntry))
        {
            PureIncome = CropAmount * SellRate;
        }
        else if (_lastTappedEntries.Contains(sellRateEntry) && _lastTappedEntries.Contains(pureIncomeEntry))
        {
            CropAmount = SellRate != 0 ? PureIncome / SellRate : 0;
        }
        else if (_lastTappedEntries.Contains(pureIncomeEntry) && _lastTappedEntries.Contains(cropAmountEntry))
        {
            SellRate = CropAmount != 0 ? PureIncome / CropAmount : 0;
        }
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
namespace FarmOrganizer.Controls;

public partial class TwoOptionSwitch : ContentView
{
    string _leftOption;
    string _rightOption;

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsSwitchedToRight
    {
        get => (bool)optionSwitch.GetValue(Switch.IsToggledProperty);
        set => optionSwitch.SetValue(Switch.IsToggledProperty, value);
    }

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(TwoOptionSwitch), propertyChanged: (bindable, oldValue, newValue) =>
    {
        var control = bindable as TwoOptionSwitch;
        control.optionLabel.Text = (string)newValue;
    });

	public TwoOptionSwitch()
	{
		InitializeComponent();
        optionSwitch.Toggled += (sender, e) => {  };
	}
}
namespace FarmOrganizer.Controls;

public partial class TwoOptionSwitch : ContentView
{
    public string LeftOptionText
    {
        get => (string)GetValue(LeftOptionTextProperty);
        set => SetValue(LeftOptionTextProperty, value);
    }
    
    public string RightOptionText
    {
        get => (string)GetValue(RightOptionTextProperty);
        set => SetValue(RightOptionTextProperty, value);
    }

    //True == RightOption
    public bool OptionSelected
    {
        get => (bool)GetValue(OptionSelectedProperty);
        set => SetValue(OptionSelectedProperty, value);
    }

    public static readonly BindableProperty LeftOptionTextProperty = BindableProperty.Create(nameof(LeftOptionText), typeof(string), typeof(TwoOptionSwitch), "Opcja A");
    public static readonly BindableProperty RightOptionTextProperty = BindableProperty.Create(nameof(RightOptionText), typeof(string), typeof(TwoOptionSwitch), "Opcja B");
    public static readonly BindableProperty OptionSelectedProperty = BindableProperty.Create(nameof(OptionSelected), typeof(bool), typeof(TwoOptionSwitch), false, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
    {
        var control = bindable as TwoOptionSwitch;
        control.optionLabel.Text = (bool)newValue ? control.RightOptionText : control.LeftOptionText;
        control.optionSwitch.IsToggled = (bool)newValue;
    });

	public TwoOptionSwitch()
	{
		InitializeComponent();
        optionSwitch.Toggled += (sender, e) => { OptionSelected = e.Value; };
        optionSwitch.IsToggled = true;
	}
}
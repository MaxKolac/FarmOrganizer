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

    public bool RightOptionSelected
    {
        get => (bool)GetValue(RightOptionSelectedProperty);
        set => SetValue(RightOptionSelectedProperty, value);
    }

    public bool LeftOptionSelected
    {
        get => !(bool)GetValue(RightOptionSelectedProperty);
        set => SetValue(RightOptionSelectedProperty, !value);
    }

    public static readonly BindableProperty LeftOptionTextProperty = BindableProperty.Create(nameof(LeftOptionText), typeof(string), typeof(TwoOptionSwitch), "Opcja A", BindingMode.TwoWay);
    public static readonly BindableProperty RightOptionTextProperty = BindableProperty.Create(nameof(RightOptionText), typeof(string), typeof(TwoOptionSwitch), "Opcja B", BindingMode.TwoWay);
    public static readonly BindableProperty RightOptionSelectedProperty = BindableProperty.Create(nameof(RightOptionSelected), typeof(bool), typeof(TwoOptionSwitch), false, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
    {
        var control = bindable as TwoOptionSwitch;
        control.optionLabel.Text = (bool)newValue ? control.RightOptionText : control.LeftOptionText;
        control.optionSwitch.IsToggled = (bool)newValue;
    });

	public TwoOptionSwitch()
	{
		InitializeComponent();
        optionSwitch.Toggled += (sender, e) => { RightOptionSelected = e.Value; };
        optionSwitch.IsToggled = true;
	}
}
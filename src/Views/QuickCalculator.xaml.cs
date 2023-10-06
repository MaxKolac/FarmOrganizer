using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class QuickCalculator : ContentPage
{
    public QuickCalculator(QuickCalculatorViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
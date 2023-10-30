using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class QuickCalculatorPage : ContentPage
{
    public QuickCalculatorPage(QuickCalculatorViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
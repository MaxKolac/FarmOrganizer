using CommunityToolkit.Maui.Views;
using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class PopupPage : Popup
{
    public PopupPage(PopupPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Closed += (sender, e) =>
        {
            if (e.WasDismissedByTappingOutsideOfPopup)
                ((PopupPage)sender).OnDeclined(sender, null);
        };
    }

    async void OnDeclined(object sender, EventArgs e) => await CloseAsync(false);
    async void OnAccepted(object sender, EventArgs e) => await CloseAsync(true);
}
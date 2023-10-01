using FarmOrganizer.ViewModels;

namespace FarmOrganizer.Views;

public partial class MainPage : ContentPage
{
    //private bool workaroundAlreadyFired = false;
    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        //Task.Run(PaddingWorkaround);
    }

    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    Task.Run(PaddingWorkaround);
    //}

    //protected override void OnNavigatedTo(NavigatedToEventArgs args)
    //{
    //    base.OnNavigatedTo(args);
    //    Task.Run(PaddingWorkaround);
    //}

    ///// <summary>
    ///// <para>
    ///// The Padding property seems to be bugged and is not applied correctly when the Button element has an image attached to it.
    ///// Combination of having both an ImageSource and Padding added, through Style or directly as a property, causes the text to contort in a bizarre way.
    ///// Updating Padding value through XAML Hot Reload instantly fixes it, likely by invalidating the measures and thus, forcing them to be re-calculated.
    ///// In fact, any action which causes the need to recalculate measures fixes it. Be it screen turning on and off, or switching out and back to the app while it's running.
    ///// </para>
    ///// <para>
    ///// Best workaround found so far is task-spamming this method from the constructor and two overridden methods.
    ///// The method simply assigns the ImageSource properties manually.
    ///// </para>
    ///// <para>
    ///// If you ever feel like fighting .NET MAUI's typical stubborn jank again, increment the value below:<br/>
    ///// <c>int fix_attempts = 0;</c>
    ///// </para>
    ///// </summary>
    //private async Task PaddingWorkaround()
    //{
    //    //Those did not work
    //    //InvalidateMeasure();
    //    //InvalidateMeasureOverride();

    //    // the amount of ms that the thread has to wait, through trial and error, has been determined to be somewhere between 400 & 800
    //    // this is likely phone performance dependent
    //    if (workaroundAlreadyFired) return;
    //    //await Task.Delay(800);
    //    await Task.Delay(600);
    //    //await Task.Delay(400);
    //    LedgerButton.ImageSource = ImageSource.FromFile("icon_settings.png");
    //    QuickCalcButton.ImageSource = ImageSource.FromFile("icon_settings.png");
    //    SeasonsButton.ImageSource = ImageSource.FromFile("icon_seasons.png");
    //    CropFieldsButton.ImageSource = ImageSource.FromFile("icon_cropfields.png");
    //    CostTypesButton.ImageSource = ImageSource.FromFile("icon_cropfields.png");
    //    SettingsButton.ImageSource = ImageSource.FromFile("icon_settings.png");
    //    workaroundAlreadyFired = true;

    //    //This with:
    //    //  await Task.Delay(3000);
    //    //at the method beggining somewhat worked but it took noticeably way too long
    //    //
    //    //LedgerButton.Padding = new Thickness(25, 41);
    //    //LedgerButton.Padding = new Thickness(25, 40);
    //    //LedgerButton.Padding = new Thickness(25, 41);
    //    //LedgerButton.Padding = new Thickness(25, 40);
    //    //LedgerButton.Padding = new Thickness(25, 41);
    //    //await Task.Delay(1000);
    //    ////LedgerButton.Padding = new Thickness(25, 40);
    //    ////LedgerButton.Padding = new Thickness(25, 41);
    //    ////LedgerButton.Padding = new Thickness(25, 40);
    //    ////LedgerButton.Padding = new Thickness(25, 41);
    //    //LedgerButton.Padding = new Thickness(25, 40);
    //    //App.AlertSvc.ShowAlert("maui sucks", ":(");
    //}
}
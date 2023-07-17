using CommunityToolkit.Mvvm.ComponentModel;

namespace FarmOrganizer.ViewModels
{
    [QueryProperty(nameof(PassedQuery), "query")]
    public partial class DatabasePageViewModel : ObservableObject
    {
        [ObservableProperty]
        string passedQuery;
    }
}

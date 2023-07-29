using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels
{
    public partial class DebugPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public string debugText;

        public void AppendDebugText(string s) =>
            DebugText += s;

        [RelayCommand]
        void PerformTest()
        {
            var context = new DatabaseContext();
            var entries = context.BalanceLedger.Find(1);
            AppendDebugText(entries.ToString());
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels
{
    public partial class DebugPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public string debugText;

        public void AppendText(string s)
        {
            DebugText += s;
            PutPause();
        }

        public void ClearText() =>
            DebugText = string.Empty;

        public void PutPause() =>
            DebugText += "\n=========\n";


        [RelayCommand]
        void PerformCRUDTests()
        {
            var context = DatabaseContext.Instance;
            ClearText();

            //Query whole table
            var qr_fullTable = context.BalanceLedger.ToList();
            AppendText(qr_fullTable.ToString());

            //Create new entry
            BalanceLedger newEntry = new()
            {
                IdCostType = 0,
                IdCropField = 1,
                DateAdded = DateTime.Now,
                BalanceChange = 1234.12,
                Notes = "Created fresh new entry!"
            };
            context.BalanceLedger.Add(newEntry);
            context.SaveChanges();

            //Read existing entry
            var qr_existingEntry = context.BalanceLedger.Find(1);
            AppendText(qr_existingEntry.ToString());

            //Read created entry
            var qr_createdEntry = context.BalanceLedger.Where(b => b.BalanceChange == 1234.12);
            AppendText(qr_createdEntry.ToString());

            //Update existing entry
            //this is a bit more complicated, because of Tracked Untracked attributes attachted to entities

            //Update created entry

            //Read existing entry again
            //qr_existingEntry = context.BalanceLedger.Find(1);
            //AppendText(qr_existingEntry.ToString());

            //Read created entry again
            //qr_createdEntry = context.BalanceLedger.Where(b => b.BalanceChange == 1234.12);
            //AppendText(qr_createdEntry.ToString());

            //Delete existing entry
            qr_existingEntry = context.BalanceLedger.Find(1);
            context.BalanceLedger.Remove(qr_existingEntry);
            context.SaveChanges();

            //Delete created entry
            qr_createdEntry = context.BalanceLedger.Where(b => b.BalanceChange == 1234.12);
            context.BalanceLedger.Remove((BalanceLedger)qr_createdEntry); 
            context.SaveChanges();

            //Query whole table
            qr_fullTable = context.BalanceLedger.ToList();
            AppendText(qr_fullTable.ToString());
        }
    }
}

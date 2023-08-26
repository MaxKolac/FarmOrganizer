using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels
{
    public partial class DebugPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public string debugText;

        public void AppendText(string s, bool addPause = true)
        {
            DebugText += s;
            if (addPause)
                PutPause();
        }
        public void ClearText() => DebugText = string.Empty;
        public void PutPause() => DebugText += "\n=========\n";

        [RelayCommand]
        async Task PerformTest() =>
            //PerformCRUDTests();
            //await PerformDatabaseFileTests();

        [RelayCommand]
        void PerformCRUDTests()
        {
            using var context = new DatabaseContext();
            ClearText();

            //Query whole table
            AppendText("QUERY WHOLE TABLE");
            var qr_fullTable = context.BalanceLedger.ToList();
            foreach (var qr in qr_fullTable)
                AppendText(qr.ToString());

            //Create new entry
            AppendText("CREATED NEW ENTRY");
            BalanceLedger newEntry = new()
            {
                IdCostType = 2,
                IdCropField = 2,
                DateAdded = DateTime.Now,
                BalanceChange = 1234.12,
                Notes = "Created fresh new entry!"
            };
            context.BalanceLedger.Add(newEntry);
            context.SaveChanges();

            //Read existing entry
            AppendText("READ EXISTING ENTRY");
            var qr_existingEntry = context.BalanceLedger.Find(7);
            AppendText(qr_existingEntry.ToString());

            //Read created entry
            AppendText("READ CREATED ENTRY");
            var qr_createdEntry = context.BalanceLedger.Where(b => b.BalanceChange == 1234.12).First();
            AppendText(qr_createdEntry.ToString());

            //Update existing entry
            //this is a bit more complicated, because of Tracked Untracked attributes attachted to entities
            AppendText("UPDATE EXISTING ENTRY");
            qr_existingEntry.BalanceChange = 69.69;
            context.SaveChanges();

            //Update created entry
            AppendText("UPDATE CREATED ENTRY");
            qr_createdEntry.BalanceChange = 71.71;
            context.SaveChanges();

            //Read existing entry again
            AppendText("READ EXISTING ENTRY AFTER UPDATE");
            qr_existingEntry = context.BalanceLedger.Find(7);
            AppendText(qr_existingEntry.ToString());

            //Read created entry again
            AppendText("READ CREATED ENTRY AFTER UPDATE");
            qr_createdEntry = context.BalanceLedger.Where(b => b.BalanceChange == 71.71).First();
            AppendText(qr_createdEntry.ToString());

            //Delete existing entry
            AppendText("DELETE EXISTING ENTRY");
            qr_existingEntry = context.BalanceLedger.Find(1);
            context.BalanceLedger.Remove(qr_existingEntry);
            context.SaveChanges();

            //Delete created entry
            AppendText("DELETE CREATED ENTRY");
            qr_createdEntry = context.BalanceLedger.Where(b => b.BalanceChange == -12.99).First();
            context.BalanceLedger.Remove(qr_createdEntry);
            context.SaveChanges();

            //Query whole table
            AppendText("QUERY WHOLE TABLE - CRUD OPERATIONS RESULT");
            qr_fullTable = context.BalanceLedger.ToList();
            foreach (var qr in qr_fullTable)
                AppendText(qr.ToString());
        }
        [RelayCommand]
        async Task PerformDatabaseFileTests()
        {
            ClearText();
            try
            {
                //After starting the app the DB should already be there, check for it
                AppendText($"DATABASE FILE EXISTS: {DatabaseFile.Exists()}");

                //Delete it and check if it deleted
                AppendText("DELETING DB FILE...");
                await DatabaseFile.Delete();
                AppendText($"DATABASE FILE EXISTS: {DatabaseFile.Exists()}");

                //Create it again
                AppendText("CREATING DB FILE...");
                await MainThread.InvokeOnMainThreadAsync(DatabaseFile.Create);
                AppendText($"DATABASE FILE EXISTS: {DatabaseFile.Exists()}");
            }
            catch (Exception ex)
            {
                AppendText("EXCEPTION WAS THROWN!");
                AppendText(ex.ToString());
                AppendText(ex.Message);
            }
        }
    }
}

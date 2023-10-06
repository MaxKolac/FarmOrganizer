using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels
{
    public partial class DebugPageViewModel : ObservableObject
    {
        [ObservableProperty]
        public string debugText;

        public void AppendLine(string s, bool addPause = true)
        {
            DebugText += s + "\n";
            if (addPause)
                PutPause();
        }
        public void ClearText() => DebugText = string.Empty;
        public void PutPause() => DebugText += "\n=========\n";

        [RelayCommand]
        void PerformTest() =>
            //PerformCRUDTests();
            //await PerformDatabaseFileTests();
            NewSeasonTests();

        [RelayCommand]
        void PerformCRUDTests()
        {
            using var context = new DatabaseContext();
            ClearText();

            //Query whole table
            AppendLine("QUERY WHOLE TABLE");
            var qr_fullTable = context.BalanceLedgers.ToList();
            foreach (var qr in qr_fullTable)
                AppendLine(qr.ToString());

            //Create new entry
            AppendLine("CREATED NEW ENTRY");
            BalanceLedger newEntry = new()
            {
                IdCostType = 2,
                IdCropField = 2,
                DateAdded = DateTime.Now,
                BalanceChange = 1234.12m,
                Notes = "Created fresh new entry!"
            };
            context.BalanceLedgers.Add(newEntry);
            context.SaveChanges();

            //Read existing entry
            AppendLine("READ EXISTING ENTRY");
            var qr_existingEntry = context.BalanceLedgers.FirstOrDefault(e => e.Id == 7);
            AppendLine(qr_existingEntry.ToString());

            //Read created entry
            AppendLine("READ CREATED ENTRY");
            var qr_createdEntry = context.BalanceLedgers.Where(b => b.BalanceChange == 1234.12m).First();
            AppendLine(qr_createdEntry.ToString());

            //Update existing entry
            //this is a bit more complicated, because of Tracked Untracked attributes attachted to entities
            AppendLine("UPDATE EXISTING ENTRY");
            qr_existingEntry.BalanceChange = 69.69m;
            context.SaveChanges();

            //Update created entry
            AppendLine("UPDATE CREATED ENTRY");
            qr_createdEntry.BalanceChange = 71.71m;
            context.SaveChanges();

            //Read existing entry again
            AppendLine("READ EXISTING ENTRY AFTER UPDATE");
            qr_existingEntry = context.BalanceLedgers.FirstOrDefault(e => e.Id == 7);
            AppendLine(qr_existingEntry.ToString());

            //Read created entry again
            AppendLine("READ CREATED ENTRY AFTER UPDATE");
            qr_createdEntry = context.BalanceLedgers.Where(b => b.BalanceChange == 71.71m).First();
            AppendLine(qr_createdEntry.ToString());

            //Delete existing entry
            AppendLine("DELETE EXISTING ENTRY");
            qr_existingEntry = context.BalanceLedgers.FirstOrDefault(e => e.Id == 1);
            context.BalanceLedgers.Remove(qr_existingEntry);
            context.SaveChanges();

            //Delete created entry
            AppendLine("DELETE CREATED ENTRY");
            qr_createdEntry = context.BalanceLedgers.Where(b => b.BalanceChange == -12.99m).First();
            context.BalanceLedgers.Remove(qr_createdEntry);
            context.SaveChanges();

            //Query whole table
            AppendLine("QUERY WHOLE TABLE - CRUD OPERATIONS RESULT");
            qr_fullTable = context.BalanceLedgers.ToList();
            foreach (var qr in qr_fullTable)
                AppendLine(qr.ToString());
        }
        [RelayCommand]
        async Task PerformDatabaseFileTests()
        {
            ClearText();
            try
            {
                //After starting the app the DB should already be there, check for it
                AppendLine($"DATABASE FILE EXISTS: {DatabaseFile.Exists()}");

                //Delete it and check if it deleted
                AppendLine("DELETING DB FILE...");
                await DatabaseFile.Delete();
                AppendLine($"DATABASE FILE EXISTS: {DatabaseFile.Exists()}");

                //Create it again
                AppendLine("CREATING DB FILE...");
                await MainThread.InvokeOnMainThreadAsync(DatabaseFile.Create);
                AppendLine($"DATABASE FILE EXISTS: {DatabaseFile.Exists()}");
            }
            catch (Exception ex)
            {
                AppendLine("EXCEPTION WAS THROWN!");
                AppendLine(ex.ToString());
                AppendLine(ex.Message);
            }
        }
        [RelayCommand]
        void NewSeasonTests()
        {
            //Make sure to reset the DB before testing, you dingus
            try
            {
                int[] ids = new int[4];
                using (var context = new DatabaseContext())
                {
                    ids[0] = context.Seasons.FirstOrDefault(season => season.DateStart.Date == new DateTime(2023, 1, 1)).Id;
                    #region VALIDATE
                    PrintWholeSeasonTable();
                    AppendLine("Season Table Validation");
                    Season.Validate(null);

                    AppendLine("Season Table Validation (with return table)");
                    //List<Season> allSeasons = new();
                    //Season.Validate(out List<Season> list1);
                    //allSeasons.AddRange(list1);
                    List<Season> allSeasons = Season.RetrieveAll(null);
                    foreach (Season season in allSeasons)
                    {
                        AppendLine(season.ToDebugString() + "\n", false);
                    }
                    #endregion

                    //This section should create the following timeline:
                    //0: 01.01.2023 - Today => Sezon 23
                    //1: Today - Today+1Day => validSeason1
                    //2: Today+1Day - Today+1Year+1Day => validSeason2
                    //3: Today+1Year+1Day - Season.MaximumDate => validSeason3
                    #region CREATE
                    AppendLine("Add valid Season (DateStart = Today):", false);
                    Season validSeason = new()
                    {
                        Name = "validSeason1",
                        DateStart = DateTime.Now
                    };
                    SeasonValidAddTest(validSeason);
                    ids[1] = context.Seasons.FirstOrDefault(season => season.Name.Equals("validSeason1")).Id;

                    AppendLine("Add invalid Season (DateStart = Today):", false);
                    Season invalidSeason = new()
                    {
                        Name = "invalidSeason1",
                        DateStart = DateTime.Now
                    };
                    SeasonInvalidAddTest(invalidSeason);

                    AppendLine("Add invalid Season (DateStart = Today - 1 microsecond):", false);
                    invalidSeason = new()
                    {
                        Name = "invalidSeason2",
                        DateStart = DateTime.Now.AddMicroseconds(-1)
                    };
                    SeasonInvalidAddTest(invalidSeason);

                    AppendLine("Add invalid Season (DateStart = Today + 1 microsecond):", false);
                    invalidSeason = new()
                    {
                        Name = "invalidSeason3",
                        DateStart = DateTime.Now.AddMicroseconds(1)
                    };
                    SeasonInvalidAddTest(invalidSeason);

                    AppendLine("Add valid 1day Season (DateStart = Today + 1 day):", false);
                    validSeason = new()
                    {
                        Name = "validSeason2",
                        DateStart = DateTime.Now.AddDays(1)
                    };
                    SeasonValidAddTest(validSeason);
                    ids[2] = context.Seasons.FirstOrDefault(season => season.Name.Equals("validSeason2")).Id;

                    AppendLine("Add valid 1year+1day Season (DateStart = Today + 1 Day + 1 Year", false);
                    validSeason = new()
                    {
                        Name = "validSeason3",
                        DateStart = DateTime.Now.AddYears(1).AddDays(1)
                    };
                    SeasonValidAddTest(validSeason);
                    ids[3] = context.Seasons.FirstOrDefault(season => season.Name.Equals("validSeason3")).Id;
                    context.SaveChanges();
                    #endregion
                }

                #region UPDATE
                using (var context = new DatabaseContext())
                {
                    //Any record retrieved is retrieved with DateEnd of MaximumDate no matter what is actually in the table's record; very strange
                    //PSA: NVM. After adding new records, reinitialize the DatabaseContext.
                    //PrintWholeSeasonTable();

                    AppendLine("Edit DateStart without previous season (Sezon 23 to start earlier)");
                    Season sezon23 = context.Seasons.FirstOrDefault(e => e.Id == ids[0]);
                    sezon23.DateStart = new DateTime(2022, 2, 2);
                    SeasonValidEditTest(sezon23);

                    AppendLine("Edit DateEnd without next season (validSeason3 to end earlier)");
                    Season validSeason3 = context.Seasons.FirstOrDefault(e => e.Id == ids[3]);
                    validSeason3.DateEnd = new DateTime(7777, 1, 1);
                    SeasonValidEditTest(validSeason3);

                    AppendLine("Edit DateStart with a previous season (validSeason1 to start earlier inside Sezon23)");
                    Season validSeason1 = context.Seasons.FirstOrDefault(e => e.Id == ids[1]);
                    validSeason1.DateStart = DateTime.Now.AddMonths(-1);
                    SeasonValidEditTest(validSeason1);

                    AppendLine("Edit DateEnd with a next season (validSeason2 to end later inside validSeason3)");
                    Season validSeason2 = context.Seasons.FirstOrDefault(e => e.Id == ids[2]);
                    validSeason2.DateEnd = DateTime.Now.AddDays(1).AddYears(1).AddMonths(3);
                    SeasonValidEditTest(validSeason2);

                    AppendLine("Invalid: Edit DateStart to start too early (validSeason2 before sezon23)");
                    Season invalidSeason1 = context.Seasons.FirstOrDefault(e => e.Id == ids[2]);
                    invalidSeason1.DateStart = DateTime.Now;
                    SeasonInvalidEditTest(invalidSeason1);

                    AppendLine("Invalid: Edit DateEnd to end too late (Season23 after sezon23)");
                    Season invalidSeason2 = context.Seasons.FirstOrDefault(e => e.Id == ids[0]);
                    invalidSeason2.DateEnd = DateTime.Now.AddDays(1);
                    SeasonInvalidEditTest(invalidSeason2);
                }
                #endregion

                #region DELETE
                using (var context = new DatabaseContext())
                {
                    AppendLine("Delete chronologically last season");
                    SeasonValidDeleteTest(context.Seasons.FirstOrDefault(e => e.Id == ids[3])); /////this did nothing???

                    AppendLine("Delete a session in between other seasons (Sezon23 / validSeason1 / validSeason2)");
                    SeasonValidDeleteTest(context.Seasons.FirstOrDefault(e => e.Id == ids[1]));

                    AppendLine("Delete a season with only a season after it and none before it");
                    SeasonValidDeleteTest(context.Seasons.FirstOrDefault(e => e.Id == ids[0]));

                    AppendLine("Invalid: Delete last season");
                    SeasonInvalidDeleteTest(context.Seasons.FirstOrDefault(e => e.Id == ids[2]));
                }
                #endregion
            }
            catch (FarmOrganizerException ex)
            {
                AppendLine("BAD EXCEPTION CAUGHT:");
                AppendLine(ex.Title + "\n" + ex.ToString());
            }
        }

        void SeasonValidAddTest(Season seasonToAdd)
        {
            AppendLine(seasonToAdd.ToDebugString());
            Season.AddEntry(seasonToAdd, null);
            PrintWholeSeasonTable();
        }
        void SeasonInvalidAddTest(Season seasonToAdd)
        {
            try
            {
                SeasonValidAddTest(seasonToAdd);
            }
            catch (InvalidRecordPropertyException ex)
            {
                AppendLine("Exception thrown & caught correctly:");
                AppendLine(ex.Title + "\n" + ex.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }
        void SeasonValidEditTest(Season seasonToEdit)
        {
            AppendLine(seasonToEdit.ToDebugString());
            Season.EditEntry(seasonToEdit, null);
            PrintWholeSeasonTable();
        }
        void SeasonInvalidEditTest(Season seasonToEdit)
        {
            try
            {
                SeasonValidEditTest(seasonToEdit);
            }
            catch (InvalidRecordPropertyException ex)
            {
                AppendLine("Exception thrown & caught correctly:");
                AppendLine(ex.Title + "\n" + ex.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }
        void SeasonValidDeleteTest(Season seasonToDelete)
        {
            AppendLine(seasonToDelete.ToDebugString());
            Season.DeleteEntry(seasonToDelete, null);
            PrintWholeSeasonTable();
        }
        void SeasonInvalidDeleteTest(Season seasonToDelete)
        {
            try
            {
                SeasonValidDeleteTest(seasonToDelete);
            }
            catch (RecordDeletionException ex)
            {
                AppendLine("Exception thrown & caught correctly:");
                AppendLine(ex.Title + "\n" + ex.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }
        void PrintWholeSeasonTable()
        {
            AppendLine("Current DB Seasons Records:", false);
            foreach (Season season in new DatabaseContext().Seasons.ToList())
            {
                AppendLine(season.ToDebugString(), false);
            }
        }
    }
}

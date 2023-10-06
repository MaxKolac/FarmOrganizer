using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;

namespace FarmOrganizer.Models;

public partial class BalanceLedger : IDatabaseAccesible<BalanceLedger>
{
    public int Id { get; set; }
    public int IdCostType { get; set; }
    public int IdCropField { get; set; }
    public int IdSeason { get; set; }
    public DateTime DateAdded { get; set; }
    public decimal BalanceChange { get; set; }
    public string Notes { get; set; }

    public virtual CostType IdCostTypeNavigation { get; set; }
    public virtual CropField IdCropFieldNavigation { get; set; }
    public virtual Season IdSeasonNavigation { get; set; }

    public static void Validate(DatabaseContext context) => _ = RetrieveAll(context);

    public static List<BalanceLedger> RetrieveAll(DatabaseContext context)
    {
        context ??= new DatabaseContext();
        var allEntries = new List<BalanceLedger>();
        allEntries.AddRange(context.BalanceLedgers.ToList());

        //The only real condition for BalanceLedger is that BalanceChange cant be lower than 0
        foreach (var entry in allEntries)
        {
            if (entry.BalanceChange < 0)
                throw new TableValidationException(nameof(DatabaseContext.BalanceLedgers), "Odnaleziono wpis z negatywną kwotą.");
        }

        return allEntries;
    }

    public static void AddEntry(BalanceLedger entry, DatabaseContext context)
    {
        context ??= new();
        var dateTimeCorrected = new DateTime(
                            entry.DateAdded.Year,
                            entry.DateAdded.Month,
                            entry.DateAdded.Day,
                            DateTime.Now.Hour,
                            DateTime.Now.Minute,
                            DateTime.Now.Second
                            );
        BalanceLedger newRecord = new()
        {
            IdCostType = entry.IdCostType,
            IdCropField = entry.IdCropField,
            IdSeason = entry.IdSeason,
            DateAdded = dateTimeCorrected,
            BalanceChange =
            Math.Abs(
                Math.Round(
                    Utils.CastToValue($"{entry.BalanceChange}"), 2)),
            Notes = entry.Notes
        };
        context.BalanceLedgers.Add(newRecord);
        context.SaveChanges();
    }

    public static void EditEntry(BalanceLedger entry, DatabaseContext context)
    {
        context ??= new();
        BalanceLedger existingRecord = context.BalanceLedgers.FirstOrDefault(e => e.Id == entry.Id) ??
            throw new NoRecordFoundException(nameof(DatabaseContext.BalanceLedgers), $"Id == {entry.Id}");

        existingRecord.IdCostType = entry.IdCostType;
        existingRecord.IdCropField = entry.IdCropField;
        existingRecord.IdSeason = entry.IdSeason;
        existingRecord.DateAdded = entry.DateAdded;
        existingRecord.BalanceChange =
            Math.Abs(
                Math.Round(
                    Utils.CastToValue($"{entry.BalanceChange}"), 2));
        existingRecord.Notes = entry.Notes;
        context.SaveChanges();
    }

    public static void DeleteEntry(int id, DatabaseContext context)
    {
        context ??= new();
        BalanceLedger entryToDelete = context.BalanceLedgers.FirstOrDefault(e => e.Id == id);
        if (entryToDelete is null)
            return;
        context.BalanceLedgers.Remove(entryToDelete);
        context.SaveChanges();
    }

    public override string ToString()
    {
        string s =
            $"Id = {Id}; " +
            $"CostType = {IdCostType}; " +
            $"CropField = {IdCropField}; " +
            $"DateAdded Raw = {DateAdded};" +
            $"BalanceChange = {BalanceChange}; " +
            $"Notes = {Notes}";
        return s;
    }
}

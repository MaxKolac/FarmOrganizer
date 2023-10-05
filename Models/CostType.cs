using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.ViewModels.HelperClasses;
using System.Collections.ObjectModel;

namespace FarmOrganizer.Models;

/// <summary>
/// A category of a <see cref="BalanceLedger"/> entry. Determines how it will be calculated when generating a new report.<br/>
/// In user interface, it should be referred to as "Rodzaj wpisu".
/// </summary>
public partial class CostType : IDatabaseAccesible<CostType>
{
    public int Id { get; set; }
    /// <summary>
    /// A user-friendly name of the <see cref="CostType"/>.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Determines if this <see cref="CostType"/> is considered a loss, or a profit.
    /// </summary>
    public bool IsExpense { get; set; }

    public virtual ICollection<BalanceLedger> BalanceLedgers { get; set; } = new List<BalanceLedger>();

    public override string ToString()
    {
        return Name;
    }

    //Database related methods
    public static void Validate(DatabaseContext context) => _ = RetrieveAll(context);

    public static List<CostType> RetrieveAll(DatabaseContext context)
    {
        context ??= new();
        var allEntries = new List<CostType>();
        allEntries.AddRange(context.CostTypes.ToList());

        //There needs to be at least 1 expense and 1 income category
        bool expenseFound = false;
        bool incomeFound = false;
        foreach (CostType type in allEntries)
        {
            if (string.IsNullOrEmpty(type.Name))
                throw new TableValidationException(nameof(DatabaseContext.CostTypes), "Odnaleziono rodzaj wpisu z pustą nazwą", type.ToString(), nameof(Name));

            if (type.IsExpense)
                expenseFound = true;
            else
                incomeFound = true;
            if (expenseFound && incomeFound)
                break;
        }

        if (!expenseFound || !incomeFound)
            throw new TableValidationException(nameof(DatabaseContext.CostTypes), "Nie znaleziono przynajmniej jednego rodzaju wpisu traktowanego jako wydatek lub przynajmniej jednego rodzaju wpisu traktowanego jako zysk.");

        
        return allEntries;
    }

    public static void AddEntry(CostType entry, DatabaseContext context)
    {
        context ??= new();
        if (string.IsNullOrEmpty(entry.Name))
            throw new InvalidRecordPropertyException("Nazwa", null, "Pole musi posiadać niepustą nazwę.");
        context.CostTypes.Add(entry);
        context.SaveChanges();
        
    }

    public static void EditEntry(CostType entry, DatabaseContext context)
    {
        context ??= new();
        CostType existingEntry = context.CostTypes.Find(entry.Id) ?? throw new NoRecordFoundException(nameof(DatabaseContext.CostTypes), $"Id == {entry.Id}");

        if (string.IsNullOrEmpty(entry.Name))
            throw new InvalidRecordPropertyException("Nazwa", null, "Pole musi posiadać niepustą nazwę.");
        existingEntry.Name = entry.Name;

        //If the editing is more than just name change, check if editing IsExpense wouldn't cause the last costType of unique IsExpense value to be gone.
        if (existingEntry.IsExpense != entry.IsExpense)
        {
            int expensesFound = 0;
            int profitsFound = 0;
            foreach (CostType costType in context.CostTypes.ToList())
            {
                if (costType.IsExpense)
                    expensesFound++;
                else
                    profitsFound++;
            }
            if (existingEntry.IsExpense && !entry.IsExpense && expensesFound <= 1)
                throw new InvalidRecordPropertyException("Traktuj jako wydatek", entry.IsExpense.ToString(), "Nie można zmienić ostatniego rodzaju wydatku na rodzaj przychodu.");
            if (!existingEntry.IsExpense && entry.IsExpense && profitsFound <= 1)
                throw new InvalidRecordPropertyException("Traktuj jako wydatek", entry.IsExpense.ToString(), "Nie można zmienić ostatniego rodzaju przychodu na rodzaj wydatku.");
            existingEntry.IsExpense = entry.IsExpense;
        }

        context.SaveChanges();
    }

    public static void DeleteEntry(CostType entry, DatabaseContext context)
    {
        context ??= new();
        CostType entryToDelete = context.CostTypes.Find(entry.Id);
        if (entryToDelete is null)
            return;

        //Check if we're deleting the last costType with unique isExpense value
        int expensesFound = 0;
        int profitsFound = 0;
        foreach (CostType costType in context.CostTypes.ToList())
        {
            if (costType.IsExpense)
                expensesFound++;
            else
                profitsFound++;
        }
        if (entryToDelete.IsExpense && expensesFound <= 1)
            throw new RecordDeletionException("Rodzaje kosztów", "Nie można usunąć ostatniego rodzaju wpisu, który traktowany jest jako wydatek.");
        if (!entryToDelete.IsExpense && profitsFound <= 1)
            throw new RecordDeletionException("Rodzaje kosztów", "Nie można usunąć ostatniego rodzaju wpisu, który traktowany jest jako przychód.");

        context.CostTypes.Remove(entryToDelete);
        context.SaveChanges();
        
    }

    /// <summary>
    /// Builds and returns an <see cref="ObservableCollection{T}"/> containing two <see cref="CostTypeGroup"/>s.<br/>
    /// First group contains all <see cref="CostType"/>s with the value <c>isExpense = false</c>, second group contains <see cref="CostType"/>s with the value <c>isExpense = true</c>.
    /// </summary>
    /// <param name="profitLabel">String label for the profit <see cref="CostType"/>s.</param>
    /// <param name="expensesLabel">String label for the expense <see cref="CostType"/>s.</param>
    /// <param name="context">
    /// An optional <see cref="DatabaseContext"/> object. Use when retrieving records from multiple records and their links between primary and foreign keys should be preserved by retrieving them from the same <see cref="DatabaseContext"/> context.<br/>
    /// If it is passed as <c>null</c>, method creates and disposes a context for itself and continues execution.</param>
    public static ObservableCollection<CostTypeGroup> BuildCostTypeGroups(string profitLabel, string expensesLabel, DatabaseContext context)
    {
        var expenseCostTypes = new List<CostType>();
        var profitCostTypes = new List<CostType>();
        context ??= new();
        foreach (var entry in context.CostTypes.ToList())
        {
            if (entry.IsExpense)
                expenseCostTypes.Add(entry);
            else
                profitCostTypes.Add(entry);
        }
        
        return new()
            {
                { new CostTypeGroup(profitLabel, profitCostTypes) },
                { new CostTypeGroup(expensesLabel, expenseCostTypes) }
            };
    }

    /// <inheritdoc cref="BuildCostTypeGroups(string, string, DatabaseContext)"/>
    public static ObservableCollection<CostTypeGroup> BuildCostTypeGroups(DatabaseContext context) =>
        BuildCostTypeGroups("Rodzaje przychodów", "Rodzaje wydatków", context);
}

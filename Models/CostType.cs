using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.ViewModels.HelperClasses;
using System.Collections.ObjectModel;

namespace FarmOrganizer.Models;

/// <summary>
/// A category of a <see cref="BalanceLedger"/> entry. Determines how it will be calculated when generating a new report.<br/>
/// In user interface, it should be referred to as "Rodzaj wpisu".
/// </summary>
public partial class CostType : IValidatable<CostType>
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
    public static void Validate() => Validate(out _);

    public static void Validate(out List<CostType> allEntries)
    {
        using var context = new DatabaseContext();
        allEntries = new List<CostType>();
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
    }

    public static void AddEntry(CostType entry)
    {
        using var context = new DatabaseContext();
        if (string.IsNullOrEmpty(entry.Name))
            throw new InvalidRecordPropertyException("Nazwa", null, "Pole musi posiadać niepustą nazwę.");
        context.CostTypes.Add(entry);
        context.SaveChanges();
    }

    public static void EditEntry(CostType entry)
    {
        using var context = new DatabaseContext();
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

    public static void DeleteEntry(CostType entry)
    {
        using var context = new DatabaseContext();
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
    /// <param name="activeContext">
    /// The <see cref="DatabaseContext"/> instance used to retrieve <see cref="CostType"/> entries.<br/>
    /// Use this context when filing a collection with selected <see cref="CostType"/> to make sure that the reference between them is preserved and applied.
    /// </param>
    public static ObservableCollection<CostTypeGroup> BuildCostTypeGroups(string profitLabel, string expensesLabel, out DatabaseContext activeContext)
    {
        List<CostType> expenseCostTypes = new();
        List<CostType> profitCostTypes = new();
        activeContext = new();
        foreach (var entry in activeContext.CostTypes.ToList())
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

    /// <inheritdoc cref="BuildCostTypeGroups(string, string, out DatabaseContext)"/>
    public static ObservableCollection<CostTypeGroup> BuildCostTypeGroups(out DatabaseContext activeContext) =>
        BuildCostTypeGroups("Rodzaje przychodów", "Rodzaje wydatków", out activeContext);

    /// <inheritdoc cref="BuildCostTypeGroups(string, string, out DatabaseContext)"/>
    public static ObservableCollection<CostTypeGroup> BuildCostTypeGroups() =>
        BuildCostTypeGroups(out _);
}

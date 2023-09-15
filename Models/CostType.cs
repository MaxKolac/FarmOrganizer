using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;

namespace FarmOrganizer.Models;

public partial class CostType : IValidatable<CostType>
{
    public int Id { get; set; }
    public string Name { get; set; }
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
            if (type.IsExpense)
                expenseFound = true;
            else
                incomeFound = true;
            if (expenseFound && incomeFound)
                break;
        }

        if (!expenseFound || !incomeFound)
            throw new NoRecordFoundException(nameof(DatabaseContext.CostTypes), "W tabeli nie znaleziono przynajmniej jednego kosztu traktowanego jako wydatek lub przynajmniej jednego kosztu traktowanego jako zysk.");
    }

    public static void AddEntry(CostType entry)
    {
        using var context = new DatabaseContext();
        context.CostTypes.Add(entry);
        context.SaveChanges();
    }

    public static void EditEntry(CostType entry)
    {
        using var context = new DatabaseContext();
        CostType existingEntry = context.CostTypes.Find(entry.Id) 
            ?? throw new NoRecordFoundException(
                nameof(DatabaseContext.CostTypes),
                "Nie znaleziono żadnego rekordu z pasującą wartością identyfikatora ID."
                );

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
                throw new InvalidRecordException("entry.IsExpense", "false");
            if (!existingEntry.IsExpense && entry.IsExpense && profitsFound <= 1)
                throw new InvalidRecordException("entry.IsExpense", "true");
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
            throw new InvalidDbOperationException("Nie można usunąć ostatniego rodzaju kosztu, który traktowany jest jako wydatek. Aby aplikacja działała poprawnie, musi istnieć przynajmniej jeden rodzaj kosztu traktowany jako wydatek i przynajmniej jeden rodzaj kosztu traktowany jako przychód.");
        if (!entryToDelete.IsExpense && profitsFound <= 1)
            throw new InvalidDbOperationException("Nie można usunąć ostatniego rodzaju kosztu, który traktowany jest jako przychód. Aby aplikacja działała poprawnie, musi istnieć przynajmniej jeden rodzaj kosztu traktowany jako wydatek i przynajmniej jeden rodzaj kosztu traktowany jako przychód.");

        context.CostTypes.Remove(entryToDelete);
        context.SaveChanges();
    }
}

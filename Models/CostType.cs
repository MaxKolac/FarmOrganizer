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
    public static void Validate()
    {
        using var context = new DatabaseContext();
        List<CostType> CostTypes = context.CostTypes.ToList();

        //There needs to be at least 1 expense and 1 income category
        bool expenseFound = false;
        bool incomeFound = false;
        foreach (CostType type in CostTypes)
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
}

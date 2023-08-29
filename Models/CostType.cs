namespace FarmOrganizer.Models;

public partial class CostType
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsExpense { get; set; }

    public virtual ICollection<BalanceLedger> BalanceLedgers { get; set; } = new List<BalanceLedger>();

    public override string ToString()
    {
        return Name;
    }
}

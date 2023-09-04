using System.Text;

namespace FarmOrganizer.Models;

/// <summary>
/// Represents a period of time, during which costs and profits can be accumulated.
/// <see cref="DateTime"/> of later than 9999.01.01 is treated as "in progress".
/// Only one season in the Seasons table maybe be "opened" - otherwise things will break.
/// </summary>
public partial class Season
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public bool HasConcluded { get; set; }

    public virtual ICollection<BalanceLedger> BalanceLedgers { get; set; } = new List<BalanceLedger>();
    public virtual ICollection<FieldEfficiency> FieldEfficiencies { get; set; } = new List<FieldEfficiency>();

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append(Name + " (" + DateStart.ToString("dd.MM.yy") + " - ");
        builder.Append(new DateTime(9999, 1, 1) < DateEnd ? "dzisiaj)" : DateEnd.ToString("dd.MM.yy") + ")");
        builder.Append(" - ");
        builder.Append(HasConcluded ? "Zakończony" : "W trakcie");
        return builder.ToString();
    }
}

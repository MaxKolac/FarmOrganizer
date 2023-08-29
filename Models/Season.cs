using System.Text;

namespace FarmOrganizer.Models;

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
        builder.Append(Name + " (" + DateStart.Date + " - ");
        builder.Append(HasConcluded ? DateEnd.Date + ")" : "dzisiaj) - Zakończony");
        return builder.ToString();
    }
}

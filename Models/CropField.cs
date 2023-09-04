namespace FarmOrganizer.Models;

public partial class CropField
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Hectares { get; set; }

    public virtual ICollection<BalanceLedger> BalanceLedgers { get; set; } = new List<BalanceLedger>();
    public virtual ICollection<FieldEfficiency> FieldEfficiencies { get; set; } = new List<FieldEfficiency>();

    public override string ToString()
    {
        return $"{Name} ({Hectares} ha)";
    }
}

using Microsoft.EntityFrameworkCore;
using SQLite;
#nullable enable

namespace FarmOrganizer.Models
{
    [Table("balanceLedger")]
    internal class BalanceLedgerEntry : DbContext
    {
        public int Id { get; set; }
        [Column("timeAdded")]
        public DateTime TimeAdded { get; set; }
        [Column("balanceChange"), Precision(12, 2)]
        public float BalanceChange { get; set; }
        [Column("notes")]
        public string? Notes { get; set; }

        //Relationship navigation properties
        public int CropFieldId { get; set; }
        public CropField CropField = null!;
        public int CostTypeId { get; set; }
        public CostType CostType = null!;
    }
}

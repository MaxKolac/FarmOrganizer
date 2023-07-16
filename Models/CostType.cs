using Microsoft.EntityFrameworkCore;
using SQLite;

namespace FarmOrganizer.Models
{
    [Table("costTypes")]
    internal class CostType : DbContext
    {
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("desc")]
        public string? Description { get; set; }

        //Relationship navigation properties
        public ICollection<BalanceLedgerEntry> Entries { get; } = new List<BalanceLedgerEntry>();
    }
}

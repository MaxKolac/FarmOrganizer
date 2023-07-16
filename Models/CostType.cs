using Microsoft.EntityFrameworkCore;
using SQLite;
#nullable enable

namespace FarmOrganizer.Models
{
    [Table("costTypes")]
    internal class CostType : DbContext
    {
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = "Nowe pole";
        [Column("desc")]
        public string? Description { get; set; }

        //Relationship navigation properties
        public ICollection<BalanceLedgerEntry> Entries { get; } = new List<BalanceLedgerEntry>();
    }
}

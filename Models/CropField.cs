using Microsoft.EntityFrameworkCore;
using SQLite;

namespace FarmOrganizer.Models
{
    [Table("cropFields")]
    internal class CropField : DbContext
    {
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("hectares"), Precision(8, 4)]
        public float Hectares { get; set; }
        [Column("mass"), Precision(8, 4)]
        public float CropMass { get; set; }

        //Relationship navigation properties
        public ICollection<BalanceLedgerEntry> Entries { get; } = new List<BalanceLedgerEntry>();

        public CropField()
        {
            Name = "Nowe pole";
        }
    }
}

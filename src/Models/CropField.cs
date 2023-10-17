using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;

namespace FarmOrganizer.Models;

public partial class CropField : IDatabaseAccesible<CropField>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Hectares { get; set; }

    public virtual ICollection<BalanceLedger> BalanceLedgers { get; set; } = new List<BalanceLedger>();
    public virtual ICollection<FieldEfficiency> FieldEfficiencies { get; set; } = new List<FieldEfficiency>();

    public CropField()
    {
    }

    public CropField(string name, decimal hectares)
    {
        Name = name;
        Hectares = hectares;
    }

    public override string ToString()
    {
        return $"{Name} ({Hectares} ha)";
    }

    public static void Validate(DatabaseContext context) => _ = RetrieveAll(context);

    public static List<CropField> RetrieveAll(DatabaseContext context)
    {
        context ??= new();
        var allEntries = new List<CropField>();
        allEntries.AddRange(context.CropFields.ToList());

        //Check if there exists at least 1 crop field
        if (allEntries.Count == 0)
            throw new TableValidationException(nameof(DatabaseContext.CropFields), "Nie znaleziono żadnych rekordów.");

        //Check if all records have positive non-zero hectares and non-empty names
        foreach (CropField field in allEntries)
        {
            if (string.IsNullOrEmpty(field.Name))
                throw new TableValidationException(nameof(DatabaseContext.CropFields), "Odnaleziono pole uprawne z pustą nazwą.", field.ToString(), nameof(Name));
            if (field.Hectares <= 0)
                throw new TableValidationException(nameof(DatabaseContext.CropFields), "Odnaleziono pole uprawne, którego powierzchnia jest zerowa lub mniejsza.", field.ToString(), nameof(Hectares));
        }


        return allEntries;
    }

    public static void AddEntry(CropField entry, DatabaseContext context)
    {
        context ??= new();

        //Name can't be empty for the love of god
        if (string.IsNullOrEmpty(entry.Name))
            throw new InvalidRecordPropertyException("Nazwa", null, "Pole musi posiadać niepustą nazwę.");
        //Hectares needs to be greater than 0
        if (entry.Hectares <= 0)
            throw new InvalidRecordPropertyException("Hektary", entry.Hectares.ToString(), "Zwiększ pole powierzchni pola aby było większe od zera.");

        context.CropFields.Add(entry);
        context.SaveChanges();

    }

    public static void EditEntry(CropField entry, DatabaseContext context)
    {
        context ??= new();
        CropField existingField = context.CropFields.FirstOrDefault(e => e.Id == entry.Id) ??
            throw new NoRecordFoundException(nameof(DatabaseContext.CropFields), $"Id == {entry.Id}");

        //Name can't be empty for the love of god
        if (string.IsNullOrEmpty(entry.Name))
            throw new InvalidRecordPropertyException("Nazwa", null, "Pole musi posiadać niepustą nazwę.");

        //Hectares needs to be greater than 0
        if (entry.Hectares <= 0)
            throw new InvalidRecordPropertyException("Hektary", entry.Hectares.ToString(), "Zwiększ pole powierzchni pola aby było większe od zera.");

        existingField.Name = entry.Name;
        existingField.Hectares = entry.Hectares;
        context.SaveChanges();

    }

    public static void DeleteEntry(int id, DatabaseContext context)
    {
        context ??= new();
        CropField fieldToDelete = context.CropFields.FirstOrDefault(e => e.Id == id);
        if (fieldToDelete is null)
            return;

        //The requirement of at least 1 cropfield
        if (context.CropFields.ToList().Count <= 1)
            throw new RecordDeletionException("Pola uprawne");

        context.CropFields.Remove(fieldToDelete);
        context.SaveChanges();

    }
}

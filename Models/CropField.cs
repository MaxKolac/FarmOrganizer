using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;

namespace FarmOrganizer.Models;

public partial class CropField : IValidatable<CropField>
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

    public static void Validate() => Validate(out _);

    public static void Validate(out List<CropField> allEntries)
    {
        var context = new DatabaseContext();
        allEntries = new();
        allEntries.AddRange(context.CropFields.ToList());

        //Check if there exists at least 1 crop field
        if (allEntries.Count == 0)
            throw new NoRecordFoundException(nameof(DatabaseContext.CropFields), "Nie odnaleziono żadnego rekordu pola uprawnego.");

        //Check if all records have positive non-zero hectares
        foreach (CropField field in allEntries)
        {
            if (field.Hectares <= 0)
                throw new InvalidRecordException("Hectares", field.Hectares.ToString());
        }
    }

    public static void AddEntry(CropField entry)
    {
        using var context = new DatabaseContext();

        //Name can't be empty for the love of god
        if (string.IsNullOrEmpty(entry.Name))
            throw new InvalidRecordException("Name", "Pusty");

        //Hectares needs to be greater than 0
        if (entry.Hectares <= 0)
            throw new InvalidRecordException("Hectares", entry.Hectares.ToString());

        context.CropFields.Add(entry);
        context.SaveChanges();
    }

    public static void EditEntry(CropField entry)
    {
        using var context = new DatabaseContext();
        CropField existingField = context.CropFields.Find(entry.Id) ?? throw new NoRecordFoundException(nameof(DatabaseContext.CropFields), "Nie odnaleziono takiego rekordu z tym Id");

        //Name can't be empty for the love of god
        if (string.IsNullOrEmpty(entry.Name))
            throw new InvalidRecordException("Name", "Pusty");

        //Hectares needs to be greater than 0
        if (entry.Hectares <= 0)
            throw new InvalidRecordException("Hectares", entry.Hectares.ToString());

        existingField.Name = entry.Name;
        existingField.Hectares = entry.Hectares;
        context.SaveChanges();
    }

    public static void DeleteEntry(CropField entry)
    {
        using var context = new DatabaseContext();
        CropField fieldToDelete = context.CropFields.Find(entry.Id);

        if (fieldToDelete is null)
            return;

        //The requirement of at least 1 cropfield
        if (context.CropFields.ToList().Count <= 1)
            throw new InvalidDbOperationException("Nie można usunąć ostatniego pola uprawnego. Aby aplikacja działała poprawnie, wymagany jest przynajmniej jeden rekord pola uprawnego. Zamiast usuwać pole, możesz edytować jego metraż oraz nazwę.");

        context.CropFields.Remove(fieldToDelete);
        context.SaveChanges();
    }
}

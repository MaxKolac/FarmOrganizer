using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using System.Text;

namespace FarmOrganizer.Models;

/// <summary>
/// Represents a period of time, during which costs and profits can be accumulated.
/// <see cref="DateTime"/> of later than 9999.01.01 is treated as "in progress".
/// Only one season in the Seasons table may be "opened" - otherwise things will break.
/// </summary>
public partial class Season : IValidatable<Season>
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

    //Database related methods
    public static void Validate()
    {
        var context = new DatabaseContext();
        List<Season> allSeasons = context.Seasons.ToList();

        //Check if there is at least 1 season
        if (allSeasons.Count == 0)
            throw new NoRecordFoundException(nameof(DatabaseContext.Seasons), "*");

        //Check if the only season is concluded
        if (allSeasons.Count == 1 && allSeasons[0].HasConcluded)
            throw new InvalidRecordException("W tabeli nie może znajdować się tylko jeden sezon, który jest zamknięty.", allSeasons[0].ToString());

        List<Season> openSeasons = allSeasons.FindAll(e => !e.HasConcluded);

        //Check if there are any open seasons
        if (openSeasons.Count == 0)
            throw new NoRecordFoundException(nameof(DatabaseContext.Seasons), "W tabeli nie odnaleziono żadnych otwartych sezonów.");

        //Check if there's more than 1 open season.
        if (openSeasons.Count > 1)
            throw new InvalidRecordException("W tabeli sezonów odnaleziono więcej niż jeden otwarty sezon.", openSeasons.ToString());
    }

    public static void AddEntry(Season entry)
    {
        using var context = new DatabaseContext();
        Season seasonToEnd = GetCurrentSeason();

        //New season cannot start before the previous one started
        if (seasonToEnd.DateStart >= entry.DateStart)
            throw new InvalidRecordException("Data rozpoczęcia", entry.DateStart.ToShortDateString());

        seasonToEnd.HasConcluded = true;
        seasonToEnd.DateEnd = entry.DateStart;
        context.Seasons.Add(entry);
        context.SaveChanges();
    }

    /// <summary>
    /// Retrieves the record of the currently open season. This method does NOT check consistency before retrieving the record.
    /// </summary>
    public static Season GetCurrentSeason()
    {
        using var context = new DatabaseContext();
        return context.Seasons.Where(season => !season.HasConcluded).First();
    }
}
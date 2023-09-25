using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using System.Text;

namespace FarmOrganizer.Models;

/// <summary>
/// Represents a period of time, during which costs and profits can be accumulated. <br/>
/// A collection of Seasons added together chronologically should perfectly recreate a timeline from 01.01.2023 to 12.31.9999 without any dates left out.
/// </summary>
public partial class Season : IValidatable<Season>
{
    public int Id { get; set; }
    /// <summary>
    /// Purely cosmetic string property, holding a user-friendly name of the <see cref="Season"/>.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The <see cref="DateTime"/> representing the beginning of a <see cref="Season"/>. Its <see cref="DateTime.TimeOfDay"/> property should have a value of 00:00:00.
    /// </summary>
    public DateTime DateStart { get; set; }
    /// <summary>
    /// The <see cref="DateTime"/> representing the end of a <see cref="Season"/>. Its <see cref="DateTime.TimeOfDay"/> property should have a value of 23:59:99.<br/>
    /// Values greater than date of 01.01.9999 indicate that this <see cref="Season"/> is chronologically last added.
    /// </summary>
    public DateTime DateEnd { get; set; }
    [Obsolete("The program is no longer making use of this property, it will be deleted on the next database schema update.")]
    public bool HasConcluded { get; set; }

    public virtual ICollection<BalanceLedger> BalanceLedgers { get; set; } = new List<BalanceLedger>();
    public virtual ICollection<FieldEfficiency> FieldEfficiencies { get; set; } = new List<FieldEfficiency>();

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append(Name + " (" + DateStart.ToString("dd.MM.yy") + " - ");
        builder.Append(new DateTime(9999, 1, 1) < DateEnd ? "dzisiaj)" : DateEnd.ToString("dd.MM.yy") + ")");
        //builder.Append(" - ");
        //builder.Append(HasConcluded ? "Zakończony" : "W trakcie");
        return builder.ToString();
    }

    //Database related methods
    public static void Validate() => Validate(out _);

    public static void Validate(out List<Season> allSeasons)
    {
        var context = new DatabaseContext();
        allSeasons = new();
        allSeasons.AddRange(context.Seasons.ToList());

        //Check if there is at least 1 season
        if (allSeasons.Count == 0)
            throw new TableValidationException(nameof(DatabaseContext.Seasons), "Nie odnaleziono żadnych rekordów.");

        //Check if any DateEnd and DateStart doesnt follow the pattern of having TimeOfDay property set to 23:59:99 and 00:00:00 respectively, except for the last Season
        for (int i = 0; i < allSeasons.Count - 1; i++)
        {
            if (allSeasons[i].DateEnd != allSeasons[i + 1].DateEnd.AddMicroseconds(-1))
                throw new TableValidationException(nameof(DatabaseContext.Seasons), "Wykryto dwa sezony, których daty rozpoczęcia i zakończenia nie pokrywają się prawidłowo.", $"{allSeasons[i]}; {allSeasons[i + 1]}", "DateEnd; DateStart");
        }

        //Check if last Season has the MaxValue on DateEnd
        if (allSeasons[^1].DateEnd != DateTime.MaxValue)
            throw new TableValidationException(nameof(DatabaseContext.Seasons), "Końcowy sezon musi posiadać DateTime.MaxValue jako datę zakończenia.");
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para>
    /// The only property that matters for <see cref="Season"/> is <see cref="DateStart"/> and <see cref="Name"/>.
    /// <see cref="DateStart"/> will be automatically formatted so that:
    /// <list type="bullet">
    /// <item>The previous <see cref="Season"/> ends on the <paramref name="entry"/>'s DateStart with time being 23:59:99.</item>
    /// <item>The added <see cref="Season"/> starts on the day after <paramref name="entry"/>'s DateStart with time being 00:00:00.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <inheritdoc/>
    public static void AddEntry(Season entry)
    {
        using var context = new DatabaseContext();
        Season lastSeason = context.Seasons.OrderBy(season => season.DateStart).Last();

        //New season cannot have empty name
        if (string.IsNullOrEmpty(entry.Name))
            throw new InvalidRecordPropertyException("Nazwa", null, "Sezon musi posiadać niepustą nazwę.");

        //New season cannot start before the previous one started
        if (lastSeason.DateStart >= entry.DateStart.Date.AddDays(1).AddMicroseconds(-1))
            throw new InvalidRecordPropertyException("Data rozpoczęcia", entry.DateStart.ToShortDateString(), "Data rozpoczęcia nowego sezonu nie może być wcześniej od daty zakończenia obecnego sezonu.");

        lastSeason.DateEnd = entry.DateStart.Date.AddDays(1).AddMicroseconds(-1);
        Season entryValidated = new()
        {
            Name = entry.Name,
            DateStart = entry.DateStart.Date.AddDays(1),
            DateEnd = DateTime.MaxValue
        };
        context.Seasons.Add(entryValidated);
        context.SaveChanges();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para>
    /// <list type="bullet">
    /// <item>If a previous <see cref="Season"/> exists, its <see cref="DateEnd"/> is set to day before <paramref name="entry"/>.DateStart.Date with a time of 23:59:99 and the edited <see cref="DateStart"/> is set to <paramref name="entry"/>.DateStart.Date with time of 00:00:00.</item>
    /// <item>If a next <see cref="Season"/> exists, its <see cref="DateStart"/> is set to the day after <paramref name="entry"/>.DateEnd with a time of 00:00:00 and the edited <see cref="DateEnd"/> is set to <paramref name="entry"/>.DateEnd.Date with a time of 23:59:99.</item>
    /// </list>
    /// </para>
    /// </summary>
    /// <inheritdoc/>
    public static void EditEntry(Season entry)
    {
        #nullable enable
        using var context = new DatabaseContext();
        Season editedSeason = context.Seasons.Find(entry.Id) ?? throw new NoRecordFoundException(nameof(DatabaseContext.Seasons), $"Id == {entry.Id}");
        Season? previousSeason = null;
        Season? nextSeason = null;

        //New season cannot have empty name
        if (string.IsNullOrEmpty(entry.Name))
            throw new InvalidRecordPropertyException("Nazwa", null, "Sezon musi posiadać niepustą nazwę.");

        //Find out if the edited Season has any chronological neighbours.
        //If it does, their respective DateTime property will also have to be changed, as to not leave out any gaps in timeline
        List<Season> allSeasons = context.Seasons.OrderBy(season => season.DateStart).ToList();
        for (int i = allSeasons.Count - 1; i >= 0; i--)
        {
            if (allSeasons[i].Id == entry.Id)
            {
                previousSeason = context.Seasons.Find(i - 1);
                nextSeason = context.Seasons.Find(i + 1);
                break;
            }
        }

        //Date restraints depending on whether or not Season being edited has neighbouring Seasons
        if (previousSeason is not null)
        {
            if (entry.DateStart.Date <= previousSeason.DateStart.Date.AddMicroseconds(-1))
                throw new InvalidRecordPropertyException("Data rozpoczęcia", entry.DateStart.Date.ToString(), "Przesuń datę rozpoczęcia na późniejszą datę.");
            previousSeason.DateEnd = entry.DateStart.Date.AddMicroseconds(-1);
            editedSeason.DateStart = entry.DateStart.Date;
        }

        if (nextSeason is not null)
        {
            if (nextSeason.DateEnd <= entry.DateEnd.Date.AddDays(1).AddMicroseconds(-1))
                throw new InvalidRecordPropertyException("Data zakończenia", entry.DateEnd.Date.ToString(), "Przesuń datę zakończenia na wcześniejszą datę.");
            editedSeason.DateEnd = entry.DateEnd.Date.AddDays(1).AddMicroseconds(-1);
            nextSeason.DateStart = entry.DateEnd.Date.AddDays(1);
        }
        context.SaveChanges();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para>
    /// If the deleted <see cref="Season"/> is chronologically the last, the previous record's <see cref="DateEnd"/> is changed to <see cref="DateTime.MaxValue"/>.<br/>
    /// Otherwise, the <see cref="Season"/> right after the deleted one fills in the gap left behind after deleting the desired record.
    /// </para>
    /// </summary>
    /// <inheritdoc/>
    public static void DeleteEntry(Season entry)
    {
        using var context = new DatabaseContext();
        Season? seasonToDelete = context.Seasons.Find(entry.Id);
        if (seasonToDelete is null)
            return;
        
        List<Season> allSeasons = context.Seasons.OrderBy(season => season.DateStart).ToList();

        //Minimum of 1 season
        if (allSeasons.Count <= 1)
            throw new RecordDeletionException("Sezony");

        //Find out if the deleted Season is the last Season.
        if (allSeasons[^1].Id == seasonToDelete.Id)
        {
            Season? previousSeason = context.Seasons.Find(allSeasons[^2].Id) ?? 
                throw new RecordDeletionException("Sezony", "Odnaleziono więcej niż 1 sezon, ale nie udało się odnaleźć drugiego sezonu licząc od końca.");
            previousSeason.DateEnd = DateTime.MaxValue;
            return;
        }
        //If it isn't, the next season is chosen to cover the timespan hole left after the entry is deleted
        else
        {
            for (int i = allSeasons.Count - 2; i >= 0; i--)
            {
                if (allSeasons[i].Id == entry.Id)
                {
                    Season? nextSeason = context.Seasons.Find(i + 1) ??
                        throw new RecordDeletionException("Sezony", "Nie odnaleziono sezonu następującego po sezonie usuwanym, mimo że powinien istnieć.");
                    nextSeason.DateStart = seasonToDelete.DateStart;
                }
            }
        }
        context.Seasons.Remove(seasonToDelete);
        context.SaveChanges();
    }

    /// <summary>
    /// Retrieves the record of the Season currently in progress, timewise. This method does NOT check consistency before retrieving the record.
    /// </summary>
    public static Season GetCurrentSeason()
    {
        using var context = new DatabaseContext();
        IEnumerable<Season> query =
            from season in context.Seasons
            where season.DateStart <= DateTime.Now && DateTime.Now <= season.DateEnd
            select season;
        return query.First();
    }
}
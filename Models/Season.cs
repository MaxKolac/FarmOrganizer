using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using System.Text;

namespace FarmOrganizer.Models;

/// <summary>
/// Represents a period of time, during which costs and profits can be accumulated. <br/>
/// A collection of Seasons added together chronologically should perfectly recreate a timeline from 01.01.2023 to 12.31.9999 without any dates left out. Minimum length of a single Season is 1 day - 1 microsecond (23 hours, 59 minutes and 59 seconds).
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
    /// <summary>
    /// <b>Important!</b> SQLite stores <see cref="DateTime.MaxValue"/> as C#'s actual <see cref="DateTime.MaxValue"/> value <b>MINUS 9 TICKS</b>.<br/>
    /// During testing, records retrieved from the database had their <see cref="DateEnd"/> changed <see cref="DateTime.Ticks"/> property from <c>3155378975999999999</c> to <c>3155378975999999990</c>.<br/>
    /// Whenever you wish to use <see cref="DateTime.MaxValue"/>, to avoid this discrepancy use <see cref="MaximumDate"/> instead.
    /// </summary>
    public static readonly DateTime MaximumDate = new(9999, 1, 1);

    public virtual ICollection<BalanceLedger> BalanceLedgers { get; set; } = new List<BalanceLedger>();
    public virtual ICollection<FieldEfficiency> FieldEfficiencies { get; set; } = new List<FieldEfficiency>();

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append(Name + " (");
        if (MaximumDate <= DateEnd)
            builder.Append("od " + DateStart.ToString("dd.MM.yy"));
        else
            builder.Append(DateStart.ToString("dd.MM.yy") + " - " + DateEnd.ToString("dd.MM.yy"));
        builder.Append(')');
        //builder.Append(Name + " (" + DateStart.ToString("dd.MM.yy") + " - ");
        //builder.Append(new DateTime(9999, 1, 1) < DateEnd ? "dzisiaj)" : DateEnd.ToString("dd.MM.yy") + ")");
        //builder.Append(" - ");
        //builder.Append(HasConcluded ? "Zakończony" : "W trakcie");
        return builder.ToString();
    }

    public string ToDebugString() =>
        $"{Id}: {Name} - ({DateStart} - {DateEnd})";

    //Database related methods
    public static void Validate() => _ = ValidateRetrieve();

    public static List<Season> ValidateRetrieve()
    {
        var context = new DatabaseContext();
        List<Season> allSeasons = new();
        allSeasons.AddRange(context.Seasons.ToList());

        //Check if there is at least 1 season
        if (allSeasons.Count == 0)
            throw new TableValidationException(nameof(DatabaseContext.Seasons), "Nie odnaleziono żadnych rekordów.");

        for (int i = 0; i < allSeasons.Count - 1; i++)
        {
            //Check if any Season has DateEnd earlier than DateStart, and if any of them break the minimum length of 1 day
            if (allSeasons[i].DateEnd < allSeasons[i].DateStart.AddDays(1).AddMicroseconds(-1))
                throw new TableValidationException(nameof(DatabaseContext.Seasons), "Wykryto sezon, w którym data rozpoczęcia znajduje się po dacie zakończenia i/lub nie spłenia wymagania długości min. 1 dnia.", allSeasons[i].ToDebugString(), "DateStart; DateEnd");
            //Check if any DateEnd and DateStart doesnt follow the pattern of having TimeOfDay property set to 23:59:99 and 00:00:00 respectively, except for the last Season
            if (allSeasons[i].DateEnd != allSeasons[i + 1].DateStart.AddMicroseconds(-1))
                throw new TableValidationException(nameof(DatabaseContext.Seasons), "Wykryto dwa sezony, których daty rozpoczęcia i zakończenia nie pokrywają się prawidłowo.", $"{allSeasons[i]}; {allSeasons[i + 1]}", "DateEnd; DateStart");
        }

        //Check if last Season has the MaximumDate on DateEnd. See MaximumDate doc to understand why using DateTime.MaxValue is ill-advised
        if (allSeasons[^1].DateEnd.Date < MaximumDate)
            throw new TableValidationException(nameof(DatabaseContext.Seasons), "Końcowy sezon musi posiadać MaximumDate jako datę zakończenia.");

        return allSeasons;
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

        //  This check doesn't matter here, as the final added entry has its DateEnd overriden to always be MaximumDate
        //  This check might become important if customer demands that you can create a Season inbetween other Seasons, instead
        //  of the hardcoded behaviour of always appending new Seasons after all other Seasons.
        //New season needs to have DateEnd greater then DateStart obviously
        //DateStart needs to be equal or smaller after adding 1day-1microsecond for entry to be valid - min. length of 1 day.
        //if (entry.DateEnd < entry.DateStart.AddDays(1).AddMicroseconds(-1))
        //    throw new InvalidRecordPropertyException("Data zakończenia", entry.DateEnd.ToShortDateString(), "Datą zakończenia sezonu może być data, która występuje przynajmniej 1 dzień po dacie rozpoczęcia sezonu.");

        //New season cannot start before the previous one started
        if (lastSeason.DateStart >= entry.DateStart.Date.AddDays(1).AddMicroseconds(-1))
            throw new InvalidRecordPropertyException("Data rozpoczęcia", entry.DateStart.ToShortDateString(), "Data rozpoczęcia nowego sezonu nie może być wcześniej od daty zakończenia obecnego sezonu.");

        lastSeason.DateEnd = entry.DateStart.Date.AddDays(1).AddMicroseconds(-1);
        Season entryValidated = new()
        {
            Name = entry.Name,
            DateStart = entry.DateStart.Date.AddDays(1),
            DateEnd = MaximumDate
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

        //New season needs to have DateEnd greater then DateStart obviously
        //DateStart needs to be equal or smaller after adding 1day-1microsecond for entry to be valid - min. length of 1 day.
        if (entry.DateEnd < entry.DateStart.AddDays(1).AddMicroseconds(-1))
            throw new InvalidRecordPropertyException("Data zakończenia", entry.DateEnd.ToShortDateString(), "Datą zakończenia sezonu może być data, która występuje przynajmniej 1 dzień po dacie rozpoczęcia sezonu.");

        //Find out if the edited Season has any chronological neighbours.
        //If it does, their respective DateTime property will also have to be changed, as to not leave out any gaps in timeline
        List<Season> allSeasonsOrdered = context.Seasons.OrderBy(season => season.DateStart).ToList();
        for (int i = allSeasonsOrdered.Count - 1; i >= 0; i--)
        {
            if (allSeasonsOrdered[i].Id == entry.Id)
            {
                previousSeason = i == 0 ? null : context.Seasons.Find(allSeasonsOrdered[i - 1].Id);
                nextSeason = i == allSeasonsOrdered.Count - 1 ? null : context.Seasons.Find(allSeasonsOrdered[i + 1].Id);
                break;
            }
        }

        //Date restraints depending on whether or not Season being edited has neighbouring Seasons
        if (previousSeason is not null)
        {
            if (entry.DateStart < previousSeason.DateStart.Date.AddDays(1))
                throw new InvalidRecordPropertyException("Data rozpoczęcia", entry.DateStart.Date.ToString(), "Zmień datę rozpoczęcia na późniejszą datę.");
            previousSeason.DateEnd = entry.DateStart.Date.AddMicroseconds(-1);
        }
        editedSeason.DateStart = entry.DateStart.Date;

        if (nextSeason is not null)
        {
            if (entry.DateEnd > nextSeason.DateEnd.Date.AddMicroseconds(-1))
                throw new InvalidRecordPropertyException("Data zakończenia", entry.DateEnd.Date.ToString(), "Zmień datę zakończenia na wcześniejszą datę.");
            editedSeason.DateEnd = entry.DateEnd.Date.AddDays(1).AddMicroseconds(-1);
            nextSeason.DateStart = entry.DateEnd.Date.AddDays(1);
        }
        context.SaveChanges();
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para>
    /// If the deleted <see cref="Season"/> is chronologically the last, the previous record's <see cref="DateEnd"/> is changed to <see cref="MaximumDate"/>.<br/>
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

        List<Season> allSeasonsOrdered = context.Seasons.OrderBy(season => season.DateStart).ToList();

        //Minimum of 1 season
        if (allSeasonsOrdered.Count <= 1)
            throw new RecordDeletionException("Sezony");

        //Find out if the deleted Season is the last Season.
        if (allSeasonsOrdered[^1].Id == seasonToDelete.Id)
        {
            Season? previousSeason = context.Seasons.Find(allSeasonsOrdered[^2].Id) ??
                throw new RecordDeletionException("Sezony", "Odnaleziono więcej niż 1 sezon, ale nie udało się odnaleźć drugiego sezonu licząc od końca.");
            previousSeason.DateEnd = MaximumDate;
        }
        //If it isn't, the next season is chosen to cover the timespan hole left after the entry is deleted
        else
        {
            for (int i = allSeasonsOrdered.Count - 2; i >= 0; i--)
            {
                if (allSeasonsOrdered[i].Id == entry.Id)
                {
                    Season? nextSeason = context.Seasons.Find(allSeasonsOrdered[i + 1].Id) ??
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
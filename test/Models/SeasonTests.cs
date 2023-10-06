using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;

namespace FarmOrganizerTests.Models
{
    public static class SeasonTestData
    {
        public static List<Season> GetThreeValidSeasons()
        {
            return new()
            {
                new()
                {
                    Id = 1,
                    Name = "A",
                    DateStart = new(2023, 1, 1, 0, 0, 0),
                    DateEnd = new(2023, 12, 31, 23, 59, 59)
                },
                new()
                {
                    Id = 2,
                    Name = "B",
                    DateStart = new(2024, 1, 1, 0, 0, 0),
                    DateEnd = new(2024, 12, 31, 23, 59, 59)
                },
                new()
                {
                    Id = 3,
                    Name = "C",
                    DateStart = new(2025, 1, 1, 0, 0, 0),
                    DateEnd = Season.MaximumDate
                }
            };
        }

        public static IEnumerable<object[]> GetInvalidSeasonRecordsToAdd()
        {
            yield return new object[]
            {
                new Season()
                {
                    Id = 1,
                    Name = string.Empty,
                    DateStart = new(2025, 2, 1, 12, 30, 30)
                }
            };
            yield return new object[]
            {
                new Season()
                {
                    Id = 1,
                    Name = "A",
                    DateStart = new(2025, 1, 1, 0, 0, 0)
                }
            };
            yield return new object[]
            {
                new Season()
                {
                    Id = 1,
                    Name = "A",
                    DateStart = new(2024, 12, 31, 23, 59, 59)
                }
            };
            yield return new object[]
            {
                new Season()
                {
                    Id = 1,
                    Name = "A",
                    DateStart = new(2023, 12, 31, 23, 59, 59)
                }
            };
        }

        public static IEnumerable<object[]> GetInvalidSeasonRecordPropertiesToEdit()
        {
            var previousSeasonDateEnd = new DateTime(2023, 12, 31, 23, 59, 59);
            var nextSeasonDateStart = new DateTime(2025, 1, 1, 0, 0, 0);
            yield return new object[]
            {
                new Season()
                {
                    Id = 2,
                    Name = string.Empty,
                    DateStart = new(2024, 1, 1, 0, 0, 0),
                    DateEnd = new(2024, 12, 31, 23, 59, 59)
                }
            };
            yield return new object[]
            {
                new Season()
                {
                    Id = 2,
                    Name = "B",
                    DateStart = previousSeasonDateEnd.AddSeconds(-1),
                    DateEnd = nextSeasonDateStart.AddSeconds(-1)
                }
            };
            yield return new object[]
            {
                new Season()
                {
                    Id = 2,
                    Name = "B",
                    DateStart = previousSeasonDateEnd.AddSeconds(1),
                    DateEnd = nextSeasonDateStart.AddSeconds(1)
                }
            };

        }

        public static IEnumerable<object[]> GetIncorrectDateSets()
        {
            var correctDateStart = new DateTime(2024, 1, 1, 0, 0, 0);
            var correctDateEnd = new DateTime(2024, 12, 31, 23, 59, 59);
            yield return new object[] { correctDateStart.AddSeconds(-1), correctDateEnd };
            yield return new object[] { correctDateStart, correctDateEnd.AddSeconds(1) };

            yield return new object[] { correctDateStart.AddSeconds(-2), correctDateEnd };
            yield return new object[] { correctDateStart, correctDateEnd.AddSeconds(2) };

            yield return new object[] { correctDateStart.AddSeconds(1), correctDateEnd };
            yield return new object[] { correctDateStart, correctDateEnd.AddSeconds(-1) };

            yield return new object[] { correctDateStart, correctDateStart };
            yield return new object[] { correctDateEnd, correctDateEnd };

            yield return new object[] { correctDateEnd, correctDateStart };
        }
    }

    public class SeasonTests
    {
        [Fact]
        public static void ValidateTheMinimumOfOneRecord()
        {
            var context = new Mock<DatabaseContext>();
            IList<Season> emptySeasons = new List<Season>();
            context.Setup(e => e.Seasons).ReturnsDbSet(emptySeasons);

            Assert.Throws<TableValidationException>(() => Season.Validate(context.Object));
        }

        [Fact]
        public static void ValidateAllSeasonsHaveNonEmptyName()
        {
            var context = new Mock<DatabaseContext>();
            IList<Season> seasons = new List<Season>()
            {
                new()
                {
                    Id = 1,
                    Name = "A",
                    DateStart = new(2023, 1, 1, 0, 0, 0),
                    DateEnd = new(2023, 12, 31, 23, 59, 59)
                },
                new()
                {
                    Id = 2,
                    Name = string.Empty,
                    DateStart = new(2024, 1, 1, 0, 0, 0),
                    DateEnd = new(2024, 12, 31, 23, 59, 59)
                },
                new()
                {
                    Id = 3,
                    Name = "C",
                    DateStart = new(2025, 1, 1, 0, 0, 0),
                    DateEnd = Season.MaximumDate
                }
            };
            context.Setup(e => e.Seasons).ReturnsDbSet(seasons);
        }

        [Theory]
        [MemberData(nameof(SeasonTestData.GetIncorrectDateSets), MemberType = typeof(SeasonTestData))]
        public static void ValidateIncorrectlySetupDates(DateTime dateStart, DateTime dateEnd)
        {
            var context = new Mock<DatabaseContext>();
            IList<Season> seasons = new List<Season>()
            {
                new()
                {
                    Id = 1,
                    Name = "A",
                    DateStart = new(2023, 1, 1, 0, 0, 0),
                    DateEnd = new(2023, 12, 31, 23, 59, 59)
                },
                new()
                {
                    Id = 2,
                    Name = "B",
                    DateStart = dateStart,
                    DateEnd = dateEnd
                },
                new()
                {
                    Id = 3,
                    Name = "C",
                    DateStart = new(2025, 1, 1, 0, 0, 0),
                    DateEnd = Season.MaximumDate
                }
            };
            context.Setup(e => e.Seasons).ReturnsDbSet(seasons);

            Assert.Throws<TableValidationException>(() => Season.Validate(context.Object));
        }

        [Fact]
        public static void ValidateLastSeasonHasMaximumDate()
        {
            var context = new Mock<DatabaseContext>();
            IList<Season> seasons = new List<Season>()
            {
                new()
                {
                    Id = 1,
                    Name = "A",
                    DateStart = new(2023, 1, 1, 0, 0, 0),
                    DateEnd = new(2023, 12, 31, 23, 59, 59)
                }
            };
            context.Setup(e => e.Seasons).ReturnsDbSet(seasons);

            Assert.Throws<TableValidationException>(() => Season.Validate(context.Object));
        }

        [Theory]
        [MemberData(nameof(SeasonTestData.GetInvalidSeasonRecordsToAdd), MemberType = typeof(SeasonTestData))]
        public static void AddInvalidSeason(Season invalidRecord)
        {
            var context = new Mock<DatabaseContext>();
            context.Setup(e => e.Seasons).ReturnsDbSet(SeasonTestData.GetThreeValidSeasons());

            Assert.Throws<InvalidRecordPropertyException>(() => Season.AddEntry(invalidRecord, context.Object));
        }

        [Fact]
        public static void EditNonexistentRecord()
        {
            var context = new Mock<DatabaseContext>();
            context.Setup(e => e.Seasons).ReturnsDbSet(SeasonTestData.GetThreeValidSeasons());
            var seasonWithNonexistentId = new Season()
            {
                Id = 69
            };

            Assert.Throws<NoRecordFoundException>(() => Season.EditEntry(seasonWithNonexistentId, context.Object));
        }

        [Theory]
        [MemberData(nameof(SeasonTestData.GetInvalidSeasonRecordPropertiesToEdit), MemberType = typeof(SeasonTestData))]
        public static void EditRecordToBeInvalid(Season invalidRecord)
        {
            var context = new Mock<DatabaseContext>();
            context.Setup(e => e.Seasons).ReturnsDbSet(SeasonTestData.GetThreeValidSeasons());

            Assert.Throws<InvalidRecordPropertyException>(() => Season.EditEntry(invalidRecord, context.Object));
        }

        [Fact]
        public static void DeleteLastSeason()
        {
            var context = new Mock<DatabaseContext>();
            IList<Season> oneSeason = new List<Season>()
            {
                new Season() { Id = 1, Name = "Im last One", DateStart = new(2023, 1, 1, 0, 0, 0), DateEnd = Season.MaximumDate }
            };
            context.Setup(e => e.Seasons).ReturnsDbSet(oneSeason);
            var seasonToDelete = new Season(){ Id = 1 };

            Assert.Throws<RecordDeletionException>(() => Season.DeleteEntry(seasonToDelete, context.Object));
        }
    }
}
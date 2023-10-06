using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmOrganizerTests.Models
{
    public class CostTypeTests
    {
        public static class CostTypeTestData
        {
            public static Mock<DatabaseContext> GetMockWithValidData()
            {
                var context = new Mock<DatabaseContext>();
                IList<CostType> costTypes = new List<CostType>()
                {
                    new()
                    {
                        Id = 1,
                        Name = "A",
                        IsExpense = true
                    },
                    new()
                    {
                        Id = 2,
                        Name = "B",
                        IsExpense = false
                    }
                };
                context.Setup<DbSet<CostType>>(e => e.CostTypes).ReturnsDbSet(costTypes);
                return context;
            }

            public static IEnumerable<object[]> GetInvalidRecordsToValidate()
            {
                yield return new object[]
                {
                    new List<CostType>()
                };
                yield return new object[]
                {
                    new List<CostType>()
                    {
                        new()
                        {
                            Id = 1,
                            Name = "A",
                            IsExpense = true
                        }
                    }
                };
                yield return new object[]
                {
                    new List<CostType>()
                    {
                        new()
                        {
                            Id = 1,
                            Name = "A",
                            IsExpense = false
                        }
                    }
                };
                yield return new object[]
                {
                    new List<CostType>()
                    {
                        new()
                        {
                            Id = 1,
                            Name = "A",
                            IsExpense = true
                        },
                        new()
                        {
                            Id = 1,
                            Name = "B",
                            IsExpense = true
                        }
                    }
                };
                yield return new object[]
                {
                    new List<CostType>()
                    {
                        new()
                        {
                            Id = 1,
                            Name = "A",
                            IsExpense = false
                        },
                        new()
                        {
                            Id = 1,
                            Name = "B",
                            IsExpense = false
                        }
                    }
                };
                yield return new object[]
                {
                    new List<CostType>()
                    {
                        new()
                        {
                            Id = 1,
                            Name = string.Empty,
                            IsExpense = true
                        },
                        new()
                        {
                            Id = 1,
                            Name = "B",
                            IsExpense = false
                        }
                    }
                };
            }

            public static IEnumerable<object[]> GetInvalidRecordsToEdit()
            {
                yield return new object[]
                {
                    new CostType()
                    {
                        Id = 1,
                        Name = "A",
                        IsExpense = false
                    }
                };
                yield return new object[]
                {
                    new CostType()
                    {
                        Id = 2,
                        Name = "B",
                        IsExpense = true
                    }
                };
                yield return new object[]
                {
                    new CostType()
                    {
                        Id = 1,
                        Name = string.Empty,
                        IsExpense = false
                    }
                };

            }
        }

        //Validation:
        // - at least one record exists
        // - at least one of each IsExpense record exists
        // - all cost types have names
        //Adding:
        // - name isnt empty
        //Editing:
        // - namw isnt empty
        // - applying edit wont remove last IsExpense unique record
        //Removing:
        // - removing wont remove last IsExpense unique record
        [Theory]
        [MemberData(nameof(CostTypeTestData.GetInvalidRecordsToValidate), MemberType = typeof(CostTypeTestData))]
        public static void ValidationThrowsTest(List<CostType> recordsToMock)
        {
            var context = new Mock<DatabaseContext>();
            context.Setup<DbSet<CostType>>(e => e.CostTypes).ReturnsDbSet(recordsToMock);

            Assert.Throws<TableValidationException>(() => CostType.Validate(context.Object));
        }

        [Fact]
        public static void AddingInvalidRecords()
        {
            var context = CostTypeTestData.GetMockWithValidData();
            var invalidCost = new CostType()
            {
                Name = string.Empty,
                IsExpense = true
            };

            Assert.Throws<InvalidRecordPropertyException>(() => CostType.AddEntry(invalidCost, context.Object));
        }

        [Fact]
        public static void EditNonexistentRecord()
        {
            var context = CostTypeTestData.GetMockWithValidData();
            var nonexistentRecord = new CostType() { Id = 69 };
            Assert.Throws<NoRecordFoundException>(() => CostType.EditEntry(nonexistentRecord, context.Object));
        }

        [Theory]
        [MemberData(nameof(CostTypeTestData.GetInvalidRecordsToEdit), MemberType = typeof(CostTypeTestData))]
        public static void EditRecordsToBeInvalid(CostType updateEntry)
        {
            var context = CostTypeTestData.GetMockWithValidData();
            Assert.Throws<InvalidRecordPropertyException>(() => CostType.EditEntry(updateEntry, context.Object));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public static void DeleteLastCostTypeWithUniqueIsExpenseProperty(int recordId)
        {
            var context = CostTypeTestData.GetMockWithValidData();
            Assert.Throws<RecordDeletionException>(() => CostType.DeleteEntry(recordId, context.Object));
        }
    }
}
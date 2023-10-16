using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmOrganizerTests.Models
{
    public class CropFieldTests
    {
        public static class CropFieldTestData
        {
            public static Mock<DatabaseContext> GetMockWithValidData()
            {
                var context = new Mock<DatabaseContext>();
                IList<CropField> records = new List<CropField>()
                {
                    new CropField(){ Id = 1, Name = "A", Hectares = 1.55m },
                    new CropField(){ Id = 2, Name = "B", Hectares = 0.45m }
                };
                context.Setup<DbSet<CropField>>(e => e.CropFields).ReturnsDbSet(records);
                return context;
            }

            public static IEnumerable<object[]> GetInvalidRecordsToValidate()
            {
                yield return new object[]
                {
                    new List<CropField>()
                };
                yield return new object[]
                {
                    new List<CropField>()
                    {
                        new CropField(){ Id = 1, Name = "A", Hectares = 1.55m },
                        new CropField(){ Id = 2, Name = "B", Hectares = 0m }
                    }
                };
                yield return new object[]
                {
                    new List<CropField>()
                    {
                        new CropField(){ Id = 1, Name = "A", Hectares = 1.55m },
                        new CropField(){ Id = 2, Name = string.Empty, Hectares = 0.45m }
                    }
                };
                yield return new object[]
                {
                    new List<CropField>()
                    {
                        new CropField(){ Id = 1, Name = "A", Hectares = 1.55m },
                        new CropField(){ Id = 2, Name = "B", Hectares = -0.01m }
                    }
                };
            }

            public static IEnumerable<object[]> GetInvalidRecordsToAdd()
            {
                yield return new object[]
                {
                    new CropField(){ Name = "B", Hectares = 0m }
                };
                yield return new object[]
                {
                    new CropField(){ Name = string.Empty, Hectares = 0.45m }
                };
                yield return new object[]
                {
                    new CropField(){ Name = "B", Hectares = -0.01m }
                };
            }

            public static IEnumerable<object[]> GetInvalidRecordsToEdit()
            {
                yield return new object[]
                {
                    new CropField(){ Id = 1, Name = "B", Hectares = 0m }
                };
                yield return new object[]
                {
                    new CropField(){ Id = 1, Name = string.Empty, Hectares = 0.45m }
                };
                yield return new object[]
                {
                    new CropField(){ Id = 1, Name = "B", Hectares = -0.01m }
                };
            }
        }

        [Theory]
        [InlineData("", 1.23)]
        [InlineData("NotEmpty", 0)]
        [InlineData("NotEmpty", -1)]
        public static void TestConstructorThrows(string name, decimal hectares)
        {
            Assert.Throws<ArgumentException>(() => new CropField(name, hectares));
        }

        [Theory]
        [InlineData("a", 0.01)]
        [InlineData("b", 1.234)]
        public static void TestConstructorDoesNotThrow(string name, decimal hectares)
        {
            _ = new CropField(name, hectares);
        }

        [Theory]
        [MemberData(nameof(CropFieldTestData.GetInvalidRecordsToValidate), MemberType = typeof(CropFieldTestData))]
        public static void ValidationThrowsTest(List<CropField> recordsToMock)
        {
            var context = new Mock<DatabaseContext>();
            context.Setup<DbSet<CropField>>(e => e.CropFields).ReturnsDbSet(recordsToMock);

            Assert.Throws<TableValidationException>(() => CropField.Validate(context.Object));
        }

        [Theory]
        [MemberData(nameof(CropFieldTestData.GetInvalidRecordsToAdd), MemberType = typeof(CropFieldTestData))]
        public static void AddingInvalidRecords(CropField invalidRecord)
        {
            var context = CropFieldTestData.GetMockWithValidData();

            Assert.Throws<InvalidRecordPropertyException>(() => CropField.AddEntry(invalidRecord, context.Object));
        }

        [Fact]
        public static void EditNonexistentRecord()
        {
            var context = CropFieldTestData.GetMockWithValidData();
            var nonexistentRecord = new CropField() { Id = 69 };
            Assert.Throws<NoRecordFoundException>(() => CropField.EditEntry(nonexistentRecord, context.Object));
        }

        [Theory]
        [MemberData(nameof(CropFieldTestData.GetInvalidRecordsToEdit), MemberType = typeof(CropFieldTestData))]
        public static void EditRecordsToBeInvalid(CropField updateEntry)
        {
            var context = CropFieldTestData.GetMockWithValidData();
            Assert.Throws<InvalidRecordPropertyException>(() => CropField.EditEntry(updateEntry, context.Object));
        }

        [Fact]
        public static void DeleteLastRecord()
        {
            var context = new Mock<DatabaseContext>();
            context.Setup<DbSet<CropField>>(e => e.CropFields).ReturnsDbSet(new List<CropField>() { new() { Id = 1 } });
            Assert.Throws<RecordDeletionException>(() => CropField.DeleteEntry(1, context.Object));
        }
    }
}
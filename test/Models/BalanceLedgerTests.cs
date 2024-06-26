﻿using FarmOrganizer.Database;
using FarmOrganizer.Exceptions;
using FarmOrganizer.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmOrganizerTests.Models
{
    public class BalanceLedgerTests
    {
        public static class BalanceLedgerTestData
        {
            public static IEnumerable<object[]> GetInvalidRecordsToValidate()
            {
                yield return new object[]
                {
                    new List<BalanceLedger>()
                    {
                        new()
                        {
                            BalanceChange = -0.1m,
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(BalanceLedgerTestData.GetInvalidRecordsToValidate), MemberType = typeof(BalanceLedgerTestData))]
        public static void ValidationThrowsTest(List<BalanceLedger> recordsToMock)
        {
            var context = new Mock<DatabaseContext>();
            context.Setup<DbSet<BalanceLedger>>(e => e.BalanceLedgers).ReturnsDbSet(recordsToMock);

            Assert.Throws<TableValidationException>(() => BalanceLedger.Validate(context.Object));
        }
    }
}
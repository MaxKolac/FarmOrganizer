using FarmOrganizer.Database;
using FarmOrganizer.Models;

namespace FarmOrganizerTests.Database 
{
    public class IDatabaseAccesibleTests 
    {
        [Fact]
        public static void PassingSameContextIntoManyMethods()
        {
            var context = new Mock<DatabaseContext>();
            IList<BalanceLedger> ledgers = new List<BalanceLedger>()
            {
                new(), new(), new()
            };
            IList<CostType> costTypes = new List<CostType>()
            {
                new CostType()
                {
                     Id = 1, Name = "A", IsExpense = true
                },
                new CostType()
                {
                    Id = 2, Name = "B", IsExpense = false
                },
                new CostType()
                {
                    Id = 3, Name = "C", IsExpense = false
                }
            };
            context.Setup(e => e.BalanceLedgers).ReturnsDbSet(ledgers);
            context.Setup(e => e.CostTypes).ReturnsDbSet(costTypes);

            BalanceLedger.RetrieveAll(context.Object);
            
            Assert.NotNull(context.Object);

            CostType.BuildCostTypeGroups(context.Object);

            Assert.NotNull(context.Object);
        }

        [Fact]
        public static void ConncetionsWithinContextBeingPassedAroundRemain()
        {
            var contextMock = new Mock<DatabaseContext>();
            CostType costA = new() { Id = 1, Name = "A", IsExpense = true };
            CostType costB = new() { Id = 2, Name = "B", IsExpense = false };
            IList<CostType> costs = new List<CostType>()
            {
                costA, costB
            };
            IList<BalanceLedger> ledgers = new List<BalanceLedger>()
            {
                new() { Id = 1, IdCostTypeNavigation = costA },
                new() { Id = 2, IdCostTypeNavigation = costB },
            };
            contextMock.Setup(e => e.CostTypes).ReturnsDbSet(costs);
            contextMock.Setup(e => e.BalanceLedgers).ReturnsDbSet(ledgers);

            List<BalanceLedger> allEntries = contextMock.Object.BalanceLedgers.ToList();

            Assert.Same(allEntries[0].IdCostTypeNavigation, costA);
            Assert.Same(allEntries[1].IdCostTypeNavigation, costB);
        }
    }

}

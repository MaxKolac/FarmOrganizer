using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels.Helpers
{
    public class CostTypeGroup : List<CostType>
    {
        public string Name { get; private set; }

        public CostTypeGroup(string name, List<CostType> costTypes) : base(costTypes)
        {
            Name = name;
        }
    }
}

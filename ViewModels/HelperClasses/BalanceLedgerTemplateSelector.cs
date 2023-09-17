using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels.HelperClasses
{
    public partial class BalanceLedgerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InvalidTemplate { get; set; }
        public DataTemplate ValidTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((BalanceLedger)item).IdCostTypeNavigation.IsExpense ? ValidTemplate : InvalidTemplate;
        }
    }
}

using FarmOrganizer.Models;

namespace FarmOrganizer.ViewModels.HelperClasses
{
    /// <summary>
    /// DataTemplateSelectors do not work as of current .NET MAUI version.<br/>
    /// See commit 44835aa395b80c65137d7977dd92cc6e60cff480 or <see cref="Converters.CostTypeToColorConverter"/> for a workaround.
    /// </summary>
    [Obsolete("Class does not work, see the XamlDoc for a workaround.")]
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

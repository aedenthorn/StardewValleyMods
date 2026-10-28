using StardewValley;

namespace ShipAbsolutelyEverything
{
    public partial class ModEntry
    {

        private static bool ShouldIncludeForShipping(Item item)
        {
            return item is Object obj && !Config.ExcludedItems.Contains(obj.ItemId) && !string.IsNullOrWhiteSpace(obj.Type) && Config.IncludeTypes.Contains(obj.Type) && !obj.bigCraftable.Value && Game1.objectData.TryGetValue(obj.ItemId, out var data) && !data.ExcludeFromShippingCollection && obj.canBeShipped();
        }
    }
}
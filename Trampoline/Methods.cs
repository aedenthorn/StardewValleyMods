using StardewValley;
using StardewValley.Objects;

namespace Trampoline
{
	public partial class ModEntry
	{
		private static bool IsOnTrampoline(Farmer farmer = null)
		{
			farmer ??= Game1.player;
			return farmer.IsSitting() && farmer.sittingFurniture is Furniture furniture && furniture.modData.ContainsKey(trampolineKey);
		}
	}
}

using StardewValley;
using StardewValley.Objects;

namespace OverworldChests
{
	public partial class ModEntry
	{
		public class Chest_draw_Patch
		{
			public static bool Prefix(Chest __instance)
			{
				if (!Config.ModEnabled || !__instance.modData.ContainsKey(modKey) || !__instance.Location.objects.ContainsKey(__instance.TileLocation) || (__instance.Items.Count > 0 && __instance.Items[0] is not null))
					return true;

				SMonitor.Log($"Removing chest at {__instance.TileLocation}");
				__instance.Location.objects.Remove(__instance.TileLocation);
				return false;
			}
		}

		public class Chest_showMenu_Patch
		{
			public static void Postfix(Chest __instance)
			{
				if (!Config.ModEnabled || !__instance.modData.ContainsKey(modKey) || !__instance.modData.ContainsKey(modCoinKey))
					return;

				Game1.player.Money += int.Parse(__instance.modData[modCoinKey]);
				__instance.modData.Remove(modCoinKey);
			}
		}
	}
}

using System.Collections.Generic;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;

namespace NapalmMummies
{
	public partial class ModEntry
	{
		public class Mummy_takeDamage_Patch
		{
			public static void Postfix(Mummy __instance, Farmer who)
			{
				if (!Config.ModEnabled || __instance.reviveTimer.Value != 10000)
					return;

				List<Ring> rings = new();

				if(who.leftRing.Value is CombinedRing)
				{
					rings.AddRange((who.leftRing.Value as CombinedRing).combinedRings);
				}
				else
				{
					rings.Add(who.leftRing.Value);
				}
				if(who.rightRing.Value is CombinedRing)
				{
					rings.AddRange((who.rightRing.Value as CombinedRing).combinedRings);
				}
				else
				{
					rings.Add(who.rightRing.Value);
				}
				if(rings.Exists(r => r is not null && r.ItemId == "811"))
				{
					__instance.currentLocation.explode(__instance.Tile, 2, who, false);
				}
			}
		}

		public class Ring_onMonsterSlay_Patch
		{
			public static bool Prefix(Ring __instance, Monster monster)
			{
				if (!Config.ModEnabled || __instance.ItemId != "811" || monster is not Mummy)
					return true;

				return false;
			}
		}
	}
}

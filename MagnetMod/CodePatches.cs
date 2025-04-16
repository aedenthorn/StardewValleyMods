using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace EnhancedLootMagnet
{
	public partial class ModEntry
	{
		class Debris_playerInRange_Patch
		{
			public static bool Prefix(Debris __instance, Vector2 position, Farmer farmer, ref bool __result)
			{
				if (!Config.ModEnabled)
					return true;

				if (__instance.isEssentialItem())
				{
					__result = true;
				}
				else
				{
					float appliedMagneticRadius = Config.RangeMultiplier > 0 ? farmer.GetAppliedMagneticRadius() * Config.RangeMultiplier : float.MaxValue;

					__result = (Math.Abs(position.X + 32f - farmer.StandingPixel.X) <= (float)appliedMagneticRadius) && (Math.Abs(position.Y + 32f - farmer.StandingPixel.Y) <= (float)appliedMagneticRadius);
				}
				return false;
			}
		}

		class Debris_updateChunks_Patch
		{
			private static int updateCounter = 0;

			public static void Postfix(Debris __instance, GameTime time, GameLocation location)
			{
				if (!Config.ModEnabled)
					return;

				updateCounter++;
				if (updateCounter < Config.SpeedMultiplier)
				{
					__instance.updateChunks(time, location);
				}
				updateCounter = 0;
			}
		}
	}
}

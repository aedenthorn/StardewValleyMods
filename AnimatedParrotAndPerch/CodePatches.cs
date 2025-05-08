using Microsoft.Xna.Framework;
using StardewValley;
using Object = StardewValley.Object;

namespace AnimatedParrotAndPerch
{
	public partial class ModEntry
	{
		public class Object_placementAction_Patch
		{
			public static void Postfix(Object __instance, GameLocation location)
			{
				if (__instance.bigCraftable.Value && IsPerch(__instance))
				{
					ShowParrots(location);
				}
			}
		}

		public class Object_performRemoveAction_Patch
		{
			public static void Postfix(Object __instance)
			{
				if (__instance.bigCraftable.Value && IsPerch(__instance))
				{
					Game1.playSound("parrot");
					ShowParrots(__instance.Location, __instance);
				}
			}
		}

		public class Object_checkForAction_Patch
		{
			public static void Prefix(Object __instance, Farmer who, bool justCheckingForActivity)
			{
				if (__instance.bigCraftable.Value && IsPerch(__instance) && !justCheckingForActivity)
				{
					TemporaryAnimatedSprite sprite = null;

					foreach (TemporaryAnimatedSprite s in who.currentLocation.TemporarySprites)
					{
						if (s is PerchParrot && (s as PerchParrot).tile == __instance.TileLocation)
						{
							sprite = s;
							break;
						}
					}
					if (sprite is not null && sprite is PerchParrot)
					{
						context.Monitor.Log($"Animating perch parrot for tile {__instance.TileLocation}");
						if (sprite.shakeIntensity == 0f)
						{
							(sprite as PerchParrot).doAction();
						}
						if (who.CurrentItem is Object && (who.CurrentItem as Object).Type.Contains("Seed"))
						{
							if (Game1.random.NextDouble() < Config.DropGiftChance)
							{
								if (advancedLootFrameworkApi != null)
								{
									who.currentLocation.debris.Add(new Debris(GetRandomParrotGift(who.CurrentItem as Object), __instance.TileLocation * 64f + new Vector2(32f, -32f)));
								}
								else
								{
									who.currentLocation.debris.Add(new Debris(new Object(fertilizers[Game1.random.Next(fertilizers.Length)], 1), __instance.TileLocation * 64f + new Vector2(32f, -32f)));
								}
							}
							context.Monitor.Log($"giving seed to {__instance.TileLocation}");
							who.reduceActiveItemByOne();
						}
					}
				}
			}
		}
	}
}

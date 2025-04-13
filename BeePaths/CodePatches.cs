using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace BeePaths
{
	public partial class ModEntry
	{
		public class OnValueAdded_Patch
		{
			public static void Postfix(Vector2 key, Object value)
			{
				if (!Config.ModEnabled || !Context.IsWorldReady)
					return;

				PostfixCore(key, value.Location?.NameOrUniqueName, value.Name, true);
			}

			public static void PostfixCore(Vector2 key, string locationNameOrUniqueName, string name, bool shouldBroadcast)
			{
				if (!Config.ModEnabled || !Context.IsWorldReady)
					return;

				GameLocation location = Game1.getLocationFromName(locationNameOrUniqueName);

				if (location is not null)
				{
					if (name.Equals("Bee House"))
					{
						if (!hives.TryGetValue(location.NameOrUniqueName, out Dictionary<Vector2, HiveData> dictionary))
						{
							dictionary = new();
						}
						AddHiveDataToDictionary(location, key, dictionary);
						AddDictionaryToHives(location.NameOrUniqueName, dictionary);
						if (shouldBroadcast)
						{
							SHelper.Multiplayer.SendMessage((key, locationNameOrUniqueName, name), "InvokeMethod.OnValueAdded_Patch.PostfixCore", modIDs: new[] { context.ModManifest.UniqueID });
						}
					}
				}
			}
		}

		public class OnValueRemoved_Patch
		{
			public static void Prefix(Vector2 key, Object value)
			{
				if (!Config.ModEnabled || !Context.IsWorldReady)
					return;

				PrefixCore(key, value.Location?.NameOrUniqueName, value.Name, true);
			}

			public static void PrefixCore(Vector2 key, string locationNameOrUniqueName, string name, bool shouldBroadcast)
			{
				if (!Config.ModEnabled || !Context.IsWorldReady)
					return;

				GameLocation location = Game1.getLocationFromName(locationNameOrUniqueName);

				if (location is not null)
				{
					if (name.Equals("Bee House"))
					{
						if (hives.TryGetValue(location.NameOrUniqueName, out Dictionary<Vector2, HiveData> dictionary))
						{
							dictionary.Remove(key);
							if (!dictionary.Any())
							{
								hives.Remove(location.NameOrUniqueName);
							}
						}
						if (shouldBroadcast)
						{
							SHelper.Multiplayer.SendMessage((key, locationNameOrUniqueName, name), "InvokeMethod.OnValueRemoved_Patch.PrefixCore", modIDs: new[] { context.ModManifest.UniqueID });
						}
					}
					else if (name.Equals("Garden Pot"))
					{
						if (hives.TryGetValue(location.NameOrUniqueName, out Dictionary<Vector2, HiveData> dictionary))
						{
							UpdateDictionaryWhenCropRemoveAtTile(location, key, dictionary);
						}
					}
				}
			}
		}

		public class DestroyCrop_Patch
		{
			public static void Prefix(HoeDirt __instance, ref Crop __state)
			{
				if (!Config.ModEnabled || !Context.IsWorldReady)
					return;

				__state = __instance.crop;
			}

			public static void Postfix(Crop __state)
			{
				if (!Config.ModEnabled || !Context.IsWorldReady)
					return;

				if (__state is not null)
				{
					PostfixCore(__state.tilePosition, __state.currentLocation?.NameOrUniqueName, true);
				}
			}

			public static void PostfixCore(Vector2 tilePosition, string locationNameOrUniqueName, bool shouldBroadcast)
			{
				if (!Config.ModEnabled || !Context.IsWorldReady || !hives.TryGetValue(locationNameOrUniqueName, out Dictionary<Vector2, HiveData> dictionary) || !dictionary.Any())
					return;

				GameLocation location = Game1.getLocationFromName(locationNameOrUniqueName);

				if (location is not null)
				{
					UpdateDictionaryWhenCropRemoveAtTile(location, tilePosition, dictionary);
					if (shouldBroadcast)
					{
						SHelper.Multiplayer.SendMessage((tilePosition, locationNameOrUniqueName), "InvokeMethod.DestroyCrop_Patch.PostfixCore", modIDs: new[] { context.ModManifest.UniqueID });
					}
				}
			}
		}

		public class FindCloseFlower_Patch
		{
			public static bool Prefix(GameLocation location, Vector2 startTileLocation, ref int range, Func<Crop, bool> additional_check, ref Crop __result)
			{
				if (!Config.ModEnabled || !Config.FixFlowerFind)
					return true;

				float closestDistance = float.MaxValue;
				range = Config.BeeRange;

				void CheckTile(Vector2 key, HoeDirt hoeDirt, int range, ref float closestDistance, ref Crop __result)
				{
					Crop crop = hoeDirt.crop;

					if (crop != null)
					{
						ParsedItemData data = ItemRegistry.GetData(crop.indexOfHarvest.Value);

						if (data != null && data.Category == -80 && crop.currentPhase.Value >= crop.phaseDays.Count - 1 && !crop.dead.Value && (additional_check == null || additional_check(crop)))
						{
							float distance = Vector2.Distance(startTileLocation, key);
							if (distance <= range && distance < closestDistance)
							{
								closestDistance = distance;
								__result = crop;
							}
						}
					}
				}

				foreach (var kvp in location.terrainFeatures.Pairs)
				{
					if (kvp.Value is HoeDirt hoeDirt)
					{
						CheckTile(kvp.Key, hoeDirt, range, ref closestDistance, ref __result);
					}
				}
				foreach (var kvp in location.objects.Pairs)
				{
					if (kvp.Value is IndoorPot pot && pot.hoeDirt.Value is HoeDirt hoeDirt)
					{
						CheckTile(kvp.Key, hoeDirt, range, ref closestDistance, ref __result);
					}
				}
				return false;
			}
		}
	}
}

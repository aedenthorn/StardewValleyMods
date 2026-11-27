using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace WateringCanTweaks
{
	public partial class ModEntry
	{
		public class Tool_draw_Patch
		{
			public static bool Prefix(Tool __instance)
			{
				return !Config.ModEnabled || !Config.WaterAdjacentTiles || __instance is not WateringCan || __instance.UpgradeLevel == 0;
			}
		}

		public class WateringCan_DoFunction_Patch
		{
			public static bool Prefix(WateringCan __instance, GameLocation location, int x, int y, int power, Farmer who)
			{
				if (!Config.ModEnabled || !Config.WaterAdjacentTiles || Game1.currentLocation.CanRefillWateringCanOnTile(x / Game1.tileSize, y / Game1.tileSize) || who.toolPower.Value == 0)
					return true;

				IntPtr ptr = AccessTools.Method(typeof(Tool), nameof(Tool.DoFunction)).MethodHandle.GetFunctionPointer();
				Action<GameLocation, int, int, int, Farmer> baseMethod = (Action<GameLocation, int, int, int, Farmer>)Activator.CreateInstance(typeof(Action<GameLocation, int, int, int, Farmer>), __instance, ptr);

				baseMethod(location, x, y, power, who);
				who.stopJittering();
				return false;
			}

			public static void Postfix(WateringCan __instance, int x, int y)
			{
				if (!Config.ModEnabled || !Game1.currentLocation.CanRefillWateringCanOnTile(x / Game1.tileSize, y / Game1.tileSize))
					return;

				typeof(WateringCan).GetMethod("OnUpgradeLevelChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
				__instance.waterCanMax = Math.Max(1, (int)(__instance.waterCanMax * Config.VolumeMultiplier));
				__instance.WaterLeft = __instance.waterCanMax;
				SMonitor.Log($"Filled watering can to {__instance.waterCanMax}");
			}
		}

		public class Farmer_canStrafeForToolUse_Patch
		{
			public static void Postfix(Farmer __instance, ref bool __result)
			{
				if (!Config.ModEnabled || !Config.WaterAdjacentTiles || __instance.CurrentTool is not WateringCan)
					return;

				__result = Config.StrafeWhileWatering;
			}
		}

		public class Farmer_toolPowerIncrease_Patch
		{
			public static bool Prefix(Farmer __instance, ref int ___toolPitchAccumulator)
			{
				if (!Config.ModEnabled || !Config.WaterAdjacentTiles || __instance.CurrentTool is not WateringCan wateringCan || __instance.CurrentTool.UpgradeLevel == 0)
					return true;

				if (wateringCan.WaterLeft > 0 || __instance.hasWateringCanEnchantment)
				{
					int tileCount = 0;

					SMonitor.Log($"Tool power {__instance.toolPower}");
					switch (__instance.toolPower.Value)
					{
						case 0:
							___toolPitchAccumulator = 0;
							tileCount = (int)(3 * Config.WateringTileMultiplier);
							break;
						case 1:
							tileCount = (int)(2 * Config.WateringTileMultiplier);
							break;
						case 2:
							tileCount = (int)(4 * Config.WateringTileMultiplier);
							break;
						case 3:
							tileCount = (int)((!wateringCan.hasEnchantmentOfType<ReachingToolEnchantment>() ? 9 : 16) * Config.WateringTileMultiplier);
							break;
					}

					Vector2 startTile = new((int)__instance.GetToolLocation().X / Game1.tileSize, (int)__instance.GetToolLocation().Y / Game1.tileSize);
					(HoeDirt hoeDirt, Type type) = GetHoeDirt(__instance.currentLocation, startTile);

					SMonitor.Log($"Trying to water tiles starting at {startTile}");
					if (wateringCan.UpgradeLevel > __instance.toolPower.Value && hoeDirt is not null)
					{
						int wateredTiles = 0;
						List<Vector2> tiles = new();
						bool empty = false;

						SMonitor.Log($"Trying to water {tileCount} tiles");
						if (hoeDirt.state.Value == 0)
						{
							empty = !WaterHoeDirt(__instance, startTile, type);
							wateredTiles++;

						}
						WaterTiles(__instance, tiles, startTile, type);
						tiles.Sort(delegate(Vector2 a, Vector2 b)
						{
							return Vector2.Distance(startTile, a).CompareTo(Vector2.Distance(startTile, b));
						});
						foreach (Vector2 tile in tiles)
						{
							if (empty || wateredTiles >= tileCount)
								break;

							hoeDirt = GetHoeDirt(__instance.currentLocation, tile, type).Item1;
							if (hoeDirt.state.Value == 0)
							{
								empty = !WaterHoeDirt(__instance, tile, type);
								wateredTiles++;
							}
						}
						if (wateredTiles > 0)
						{
							if (__instance.ShouldHandleAnimationSound())
							{
								__instance.currentLocation.localSound("wateringCan");
							}
							SMonitor.Log($"watered {wateredTiles} tiles");
						}
						if (!wateringCan.IsEfficient)
						{
							float oldStamina = __instance.Stamina;
							float staminaUsed = ((2 * (__instance.toolPower.Value + 1)) - __instance.FarmingLevel * 0.1f) * Config.StaminaUseMultiplier;

							__instance.Stamina = Math.Max(0, __instance.Stamina - staminaUsed);
							__instance.checkForExhaustion(oldStamina);
							SMonitor.Log($"Used {staminaUsed} stamina");
						}
						SMonitor.Log($"Increasing tool power to {__instance.toolPower.Value + 1}");
						__instance.toolPower.Value++;
						if (Config.AutoEndWateringAnimation && (wateredTiles <= 0 || __instance.toolPower.Value >= wateringCan.UpgradeLevel || wateringCan.WaterLeft <= 0 || __instance.Stamina <= 0))
						{
							__instance.EndUsingTool();
							return false;
						}
					}
					else
					{
						__instance.EndUsingTool();
						return false;
					}
				}
				return false;
			}

			private static void WaterTiles(Farmer farmer, List<Vector2> tiles, Vector2 startTile, Type type)
			{
				List<Vector2> adjacents = Utility.getAdjacentTileLocations(startTile);

				for (int i = adjacents.Count - 1; i >= 0; i--)
				{
					HoeDirt dirt = GetHoeDirt(farmer.currentLocation, adjacents[i], type).Item1;

					if (tiles.Contains(adjacents[i]) || dirt is null)
					{
						adjacents.RemoveAt(i);
					}
					else
					{
						tiles.Add(adjacents[i]);
					}
				}
				foreach (Vector2 tile in adjacents)
				{
					WaterTiles(farmer, tiles, tile, type);
				}
			}

			private static bool WaterHoeDirt(Farmer farmer, Vector2 tile, Type type)
			{
				if ((type == null || type == typeof(HoeDirt)) && farmer.currentLocation.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && feature is HoeDirt hoeDirt)
				{
					hoeDirt.state.Value = 1;
				}
				else if ((type == null || type == typeof(IndoorPot)) && CompatibilityUtility.IsConnectedGardenPotsLoaded && farmer.currentLocation.objects.TryGetValue(tile, out Object obj) && obj is IndoorPot indoorPot)
				{
					indoorPot.hoeDirt.Value.state.Value = 1;
					indoorPot.showNextIndex.Value = true;
				}
				SMonitor.Log($"watered tile {tile}");
				if (farmer.CurrentTool is WateringCan wateringCan && !wateringCan.IsBottomless && !farmer.hasWateringCanEnchantment)
				{
					wateringCan.WaterLeft--;
					if (wateringCan.WaterLeft <= 0)
					{
						SMonitor.Log("Watering can empty");
						return false;
					}
				}
				return true;
			}
		}

		private static (HoeDirt, Type) GetHoeDirt(GameLocation location, Vector2 tile, Type type = null)
		{
			if ((type == null || type == typeof(HoeDirt)) && location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature) && feature is HoeDirt hoeDirt)
			{
				return (hoeDirt, typeof(HoeDirt));
			}
			else if ((type == null || type == typeof(IndoorPot)) && CompatibilityUtility.IsConnectedGardenPotsLoaded && location.objects.TryGetValue(tile, out Object obj) && obj is IndoorPot indoorPot)
			{
				return (indoorPot.hoeDirt.Value, typeof(IndoorPot));
			}
			return (null, null);
		}
	}
}

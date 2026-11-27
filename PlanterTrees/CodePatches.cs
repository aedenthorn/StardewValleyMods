using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace PlanterTrees
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
		private static bool isPlanterTree;
        [HarmonyPatch(typeof(Tree), nameof(Tree.draw))]
        public class Tree_draw_Patch
        {
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling Tree.draw");
				var codes = new List<CodeInstruction>(instructions);
				for (int i = 0; i < codes.Count; i++)
				{
					if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == typeof(Vector2).GetField(nameof(Vector2.Y)))
					{
						SMonitor.Log("replacing Y position with method");
						codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(ModEntry.GetCurrentTreeY))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                    if ( i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_R4  && (float)codes[i].operand == 0.0001f && codes[i + 1].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+1].operand == typeof(SpriteBatch).GetMethod(nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }) )
					{
						SMonitor.Log("replacing seed layerDepth with method");
						codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(ModEntry.GetCurrentTreeY))));
						codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
						i+= 2;
					}
				}

				return codes.AsEnumerable();
			}
		}
		[HarmonyPatch(typeof(FruitTree), nameof(FruitTree.draw))]
        public class FruitTree_draw_Patch
		{
            public static void Prefix(FruitTree __instance, SpriteBatch spriteBatch)
            {
				isPlanterTree = false;
				if (!Config.EnableMod || !Game1.currentLocation.objects.TryGetValue(__instance.Tile, out Object obj) || obj is not IndoorPot)
                    return;
				isPlanterTree = true;
			}
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				SMonitor.Log($"Transpiling FruitTree.draw");
				var codes = new List<CodeInstruction>(instructions);
				for (int i = 0; i < codes.Count; i++)
				{
					if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == typeof(Vector2).GetField(nameof(Vector2.Y)))
					{
						SMonitor.Log("replacing Y position with method");
						codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(ModEntry.GetCurrentTreeY))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

				return codes.AsEnumerable();
			}
		}

		public static float GetCurrentTreeY(float yTile, TerrainFeature tf)
        {
            if (!Config.EnableMod || !tf.Location.objects.TryGetValue(tf.Tile, out Object obj) || obj is not IndoorPot)
                return yTile;
            return yTile -0.4f;
		}
		public static float GetSeedOffset(Vector2 tileLocation)
        {
			
			return isPlanterTree ? (tileLocation.Y * 64 + 64) / 10000f : 0.0001f;
        }

        [HarmonyPatch(typeof(Utility), nameof(Utility.playerCanPlaceItemHere))]
        public class Utility_playerCanPlaceItemHere_Patch
        {
            public static bool Prefix(Item item, GameLocation location, int x, int y, Farmer f, ref bool __result)
            {
				Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
                if (!Config.EnableMod || location.terrainFeatures.ContainsKey(placementTile) || !CanPlaceTreeHere(location, placementTile) || !typeof(Object).IsAssignableTo(item.GetType()))
                    return true;
				if ((item as Object).isSapling())
				{
						__result = true;
					return false;
				}
				switch (item.ParentSheetIndex)
                {
					case 309:
					case 310:
					case 311:
					case 897:
					case 292:
						break;
					default:
						return true;
				}

				__result = true;
				return false;
			}
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
            {
				Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
                if (!Config.EnableMod || location.terrainFeatures.ContainsKey(placementTile))
                    return true;
				if (!CanPlaceTreeHere(location, placementTile))
					return true;
				if (__instance.isSapling() && __instance.ParentSheetIndex != 251)
				{
					location.playSound("dirtyHit");
					DelayedAction.playSoundAfterDelay("coin", 100);
					bool actAsGreenhouse = location.IsGreenhouse || ((__instance.ParentSheetIndex == 69 || __instance.ParentSheetIndex == 835) && location is IslandWest);
					location.terrainFeatures.Add(placementTile, new FruitTree(__instance.ItemId, 0)
					{
						GreenHouseTileTree = location.IsGreenhouse && location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Stone")
					});
					return false;
					for (int i = 0; i < 29; i++)
					{
						//location.terrainFeatures[placementTile].dayUpdate(location, placementTile);
					}
				}
				location.terrainFeatures.Add(placementTile, new Tree(Tree.ResolveTreeTypeFromSeed(__instance.QualifiedItemId), 0));
				location.playSound("dirtyHit");
				__result = true;
				return false;
			}
        }
        [HarmonyPatch(typeof(Object), nameof(Object.performToolAction))]
        public class Object_performToolAction_Patch
		{
            public static bool Prefix(Object __instance, ref bool __result)
            {
                if (!Config.EnableMod || __instance is not IndoorPot || !__instance.Location.terrainFeatures.TryGetValue(__instance.TileLocation, out TerrainFeature f) || f is not Tree)
                    return true;
				__result = false;
				return false;
			}
        }
        [HarmonyPatch(typeof(Tree), nameof(Tree.dayUpdate))]
        public class Tree_dayUpdate_Patch
		{
            public static void Postfix(Tree __instance)
            {
                if (!Config.EnableMod)
                    return;
				if(CanPlaceTreeHere(__instance.Location, __instance.Tile))
                {
					if (__instance.treeType.Value == "8")
					{
						if (Game1.random.NextDouble() < 0.15 || (__instance.fertilized.Value && Game1.random.NextDouble() < 0.6))
						{
							__instance.growthStage.Value++;
						}
					}
					else if (Game1.random.NextDouble() < 0.2 || __instance.fertilized.Value)
					{
						__instance.growthStage.Value++;
					}
				}
			}
        }
        [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.IsGrowthBlocked))]
        public class FruitTree_IsGrowthBlocked_Patch
		{
            public static void Postfix(Tree __instance, Vector2 tileLocation, GameLocation environment, ref bool __result)
            {
                if (!Config.EnableMod || !__result)
                    return;
				__result = !CanPlaceTreeHere(environment, tileLocation);
			}
        }
		private static bool CanPlaceTreeHere(GameLocation location, Vector2 placementTile)
		{
			if (!location.objects.TryGetValue(placementTile, out Object obj) || obj is not IndoorPot || (obj as IndoorPot).hoeDirt.Value.crop != null)
				return false;

			if (!Config.RequiresSurroundingPots)
				return true;
			foreach (Vector2 tile in Utility.getSurroundingTileLocationsArray(placementTile))
			{
				if (!location.objects.TryGetValue(tile, out obj) || obj is not IndoorPot)
					return false;
			}
			return true;
		}
	}
}
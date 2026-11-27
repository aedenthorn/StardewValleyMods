using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.LocationContexts;
using StardewValley.GameData.WildTrees;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace GiantTrees
{
	public partial class ModEntry
	{
		[HarmonyPatch(typeof(Tree), nameof(Tree.draw))]
		public class Tree_draw_Patch
        {
            public static bool Prefix(Tree __instance, SpriteBatch spriteBatch, List<Leaf> ___leaves)
            {
				if (Config.ModEnabled && __instance.growthStage.Value >= 5 && __instance.modData.TryGetValue(modKey, out var str) && IsGiantTree(__instance, str))
                {
                    if(str == "0")
                    {

                        if (__instance.isTemporarilyInvisible)
                        {
                            return false;
                        }
                        Vector2 tileLocation = __instance.Tile;
                        float baseSortPosition = (float)__instance.getBoundingBox().Bottom;
                        WildTreeData data;
                        if (__instance.texture.Value == null || !Tree.TryGetData(__instance.treeType.Value, out data))
                        {
                            IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(O)");
                            spriteBatch.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((__instance.shakeTimer > 0f) ? ((float)Math.Sin(6.283185307179586 / (double)__instance.shakeTimer) * 3f) : 0f), tileLocation.Y * 64f)), new Rectangle?(itemType.GetErrorSourceRect()), Color.White * __instance.alpha, 0f, Vector2.Zero, 8f, __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 1f) / 10000f);
                            return false;
                        }
                        if (__instance.growthStage.Value < 5)
                        {
                            Rectangle sourceRect;
                            switch (__instance.growthStage.Value)
                            {
                                case 0:
                                    sourceRect = new Rectangle(32, 128, 16, 16);
                                    break;
                                case 1:
                                    sourceRect = new Rectangle(0, 128, 16, 16);
                                    break;
                                case 2:
                                    sourceRect = new Rectangle(16, 128, 16, 16);
                                    break;
                                default:
                                    sourceRect = new Rectangle(0, 96, 16, 32);
                                    break;
                            }
                            spriteBatch.Draw(__instance.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - (float)(sourceRect.Height * 4 - 64) + (float)((__instance.growthStage.Value >= 3) ? 128 : 64))), new Rectangle?(sourceRect), __instance.fertilized.Value ? Color.HotPink : Color.White, __instance.shakeRotation, new Vector2(8f, (float)((__instance.growthStage.Value >= 3) ? 32 : 16)), 8f, __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.growthStage.Value == 0) ? 0.0001f : (baseSortPosition / 10000f));
                        }
                        else
                        {
                            if (!__instance.stump.Value || __instance.falling.Value)
                            {
                                if (__instance.IsLeafy())
                                {
                                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 51f, tileLocation.Y * 64f - 16f)), new Rectangle?(Tree.shadowSourceRect), Color.White * (1.5707964f - Math.Abs(__instance.shakeRotation)), 0f, Vector2.Zero, 8f, __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
                                }
                                else
                                {
                                    spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 51f, tileLocation.Y * 64f - 16f)), new Rectangle?(new Rectangle(469, 298, 42, 31)), Color.White * (1.5707964f - Math.Abs(__instance.shakeRotation)), 0f, Vector2.Zero, 8f, __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
                                }
                                Rectangle source_rect = Tree.treeTopSourceRect;
                                if ((data.UseAlternateSpriteWhenSeedReady && __instance.hasSeed.Value) || (data.UseAlternateSpriteWhenNotShaken && !__instance.wasShakenToday.Value))
                                {
                                    source_rect.X = 48;
                                }
                                else
                                {
                                    source_rect.X = 0;
                                }
                                if (__instance.hasMoss.Value)
                                {
                                    source_rect.X = 96;
                                }
                                spriteBatch.Draw(__instance.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), new Rectangle?(source_rect), Color.White * __instance.alpha, __instance.shakeRotation, new Vector2(20f, 88f), 8f, __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 2f) / 10000f - tileLocation.X / 1000000f);
                            }
                            Rectangle stumpSource = Tree.stumpSourceRect;
                            if (__instance.hasMoss.Value)
                            {
                                stumpSource.X += 96;
                            }
                            if (__instance.health.Value >= 1f || (!__instance.falling.Value && __instance.health.Value > -99f))
                            {
                                spriteBatch.Draw(__instance.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((__instance.shakeTimer > 0f) ? ((float)Math.Sin(6.283185307179586 / (double)__instance.shakeTimer) * 3f) : 0f), tileLocation.Y * 64f - 64f)), new Rectangle?(stumpSource), Color.White * __instance.alpha, 0f, new Vector2(0, 8), 8f, __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, baseSortPosition / 10000f);
                            }
                            if (__instance.stump.Value && __instance.health.Value < 4f && __instance.health.Value > -99f)
                            {
                                spriteBatch.Draw(__instance.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((__instance.shakeTimer > 0f) ? ((float)Math.Sin(6.283185307179586 / (double)__instance.shakeTimer) * 3f) : 0f), tileLocation.Y * 64f)), new Rectangle?(new Rectangle(Math.Min(2, (int)(3f - __instance.health.Value)) * 16, 144, 16, 16)), Color.White * __instance.alpha, 0f, new Vector2(0, 0), 8f, __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 1f) / 10000f);
                            }
                        }
                        foreach (Leaf i in ___leaves)
                        {
                            spriteBatch.Draw(__instance.texture.Value, Game1.GlobalToLocal(Game1.viewport, i.position), new Rectangle?(new Rectangle(16 + i.type % 2 * 8, 112 + i.type / 2 * 8, 8, 8)), Color.White, i.rotation, new Vector2(0, 8), 8f, SpriteEffects.None, baseSortPosition / 10000f + 0.01f);
                        }
                    }
					return false;
                } 
                return true;
			}
		}
		[HarmonyPatch(typeof(Tree), nameof(Tree.shake))]
		public class Tree_shake_Patch
        {
            public static bool Prefix(Tree __instance, bool doEvenIfStillShaking)
			{
				if (Config.ModEnabled && __instance.growthStage.Value >= 5 && __instance.modData.TryGetValue(modKey, out var str) && IsGiantTree(__instance, str) && str != "0")
                {
                    Tree tree = GetMainTree(str, __instance.Tile, __instance.Location);
                    if (tree != null)
                    {
                        tree.shake(tree.Tile, doEvenIfStillShaking);
                        return false;
                    }
                }
                return true;
			}
        }

		[HarmonyPatch(typeof(Tree), nameof(Tree.dayUpdate))]
		public class Tree_dayUpdate_Patch
        {
            public static bool Prefix(Tree __instance, ref int __state)
			{
                if (!Config.ModEnabled)
                    return true;

                if(__instance.growthStage.Value < 5)
                {
                    __state = __instance.growthStage.Value;
                    return true;
                }
                else if (__instance.modData.TryGetValue(modKey, out var str) && IsGiantTree(__instance, str))
                {
                    return str == "0";
                }
                return true;
			}
            public static void Postfix(Tree __instance, int __state)
			{
                if (__state > 0 && __instance.growthStage.Value >= 5)
                {
                    var tileLocation = __instance.Tile;
                    var array = new Vector2[]
                    {
                        new Vector2(-1f, -1f) + tileLocation,
                        new Vector2(0f, -1f) + tileLocation,
                        new Vector2(1f, -1f) + tileLocation,
                        new Vector2(-1f, 0f) + tileLocation,
                        new Vector2(0f, 0f) + tileLocation,
                        new Vector2(1f, 0f) + tileLocation,
                        new Vector2(-1f, 1f) + tileLocation,
                        new Vector2(0f, 1f) + tileLocation,
                        new Vector2(1f, 1f) + tileLocation
                    };
                    bool[] bools = new bool[9];
                    for(int i = 0; i < array.Length; i++)
                    {
                        bools[i] = (__instance.Location.terrainFeatures.TryGetValue(array[i], out var tf) && tf is Tree tree && tree.treeType.Value == __instance.treeType.Value && tree.growthStage.Value >= 5);
                    }
                    if (Config.GiantChance < Game1.random.NextDouble())
                        return;
                    if (bools[0] && bools[1] && bools[3])
                    {
                        CreateGiantTree(__instance.Location, new Vector2[]{array[0], array[1], array[3], array[4]});
                        SMonitor.Log($"Created giant tree at {array[0]}");
                    }
                    else if (bools[1] && bools[2] && bools[5])
                    {
                        CreateGiantTree(__instance.Location, new Vector2[]{array[1], array[2], array[4], array[5]});
                        SMonitor.Log($"Created giant tree at {array[1]}");
                    }
                    else if (bools[3] && bools[6] && bools[7])
                    {
                        CreateGiantTree(__instance.Location, new Vector2[]{array[3], array[4], array[6], array[7]});
                        SMonitor.Log($"Created giant tree at {array[3]}");
                    }
                    else if (bools[5] && bools[7] && bools[8])
                    {
                        CreateGiantTree(__instance.Location, new Vector2[]{array[4], array[5], array[7], array[8]});
                        SMonitor.Log($"Created giant tree at {array[4]}");
                    }
                }
			}
        }

		[HarmonyPatch(typeof(Tree), nameof(Tree.onGreenRainDay))]
		public class Tree_onGreenRainDay_Patch
        {
            public static bool Prefix(Tree __instance)
			{
				return !Config.ModEnabled || __instance.growthStage.Value < 5 || !__instance.modData.TryGetValue(modKey, out var str) || !IsGiantTree(__instance, str) || str == "0";
			}
        }
		[HarmonyPatch(typeof(Tree), nameof(Tree.instantDestroy))]
		public class Tree_instantDestroy_Patch
        {
            public static bool Prefix(Tree __instance, Vector2 tileLocation)
			{
                if (Config.ModEnabled && __instance.growthStage.Value >= 5 && __instance.modData.TryGetValue(modKey, out var str) && IsGiantTree(__instance, str))
                {
                    foreach(var tile in GetTreeTiles(__instance.Tile, str))
                    {
                        (__instance.Location.terrainFeatures[tile] as Tree)?.instantDestroy(tile);
                    }
                    return false;
                }
                return true;
            }
        }
		[HarmonyPatch(typeof(Tree), nameof(Tree.performToolAction))]
		public class Tree_performToolAction_Patch
        {
            public static bool Prefix(Tree __instance, Tool t, int explosion, Vector2 tileLocation)
			{
                if (Config.ModEnabled && __instance.growthStage.Value >= 5 && __instance.modData.TryGetValue(modKey, out var str) && IsGiantTree(__instance, str) && str != "0")
                {
                    Tree tree = GetMainTree(str, __instance.Tile, __instance.Location);
                    if (tree != null)
                    {
                        tree.performToolAction(t, explosion, tree.Tile);
                        return false;
                    }
                }
                return true;
            }
        }
		[HarmonyPatch(typeof(Tree), nameof(Tree.isActionable))]
		public class Tree_isActionable_Patch
        {
            public static bool Prefix(Tree __instance, ref bool __result)
			{
                if (Config.ModEnabled && __instance.growthStage.Value >= 5 && __instance.modData.TryGetValue(modKey, out var str) && IsGiantTree(__instance, str) && str != "0")
                {
                    Tree tree = GetMainTree(str, __instance.Tile, __instance.Location);
                    if (tree != null)
                    {
                        __result = tree.isActionable();
                        return false;
                    }
                }
                return true;
            }
        }
		[HarmonyPatch(typeof(Tree), nameof(Tree.tickUpdate))]
		public class Tree_tickUpdate_Patch
        {
            public static void Postfix(Tree __instance, bool __result)
			{
                if (Config.ModEnabled && __result && __instance.growthStage.Value >= 5 && __instance.modData.TryGetValue(modKey, out var str) && IsGiantTree(__instance, str) && str == "0")
                {
                    foreach (var tile in GetOtherTreeTiles(__instance.Tile))
                    {
                        if(__instance.Location.terrainFeatures.TryGetValue(tile, out var tf) && tf is Tree tree)
                            tree.health.Value = -100f;
                    }
                }
            }
        }
		[HarmonyPatch(typeof(Tree), nameof(Tree.IsGrowthBlockedByNearbyTree))]
		public class Tree_IsGrowthBlockedByNearbyTree_Patch
        {
            public static bool Prefix(Tree __instance, ref bool __result)
			{
                if (!Config.ModEnabled)
                    return true;
                GameLocation location = __instance.Location;
                Vector2 tile = __instance.Tile;
                Rectangle growthRect = new Rectangle((int)((tile.X - 1f) * 64f), (int)((tile.Y - 1f) * 64f), 192, 192);
                foreach (KeyValuePair<Vector2, TerrainFeature> other in location.terrainFeatures.Pairs)
                {
                    if (other.Key != tile)
                    {
                        Tree otherTree = other.Value as Tree;
                        if (otherTree != null && otherTree.treeType.Value != __instance.treeType.Value && otherTree.growthStage.Value >= 5 && otherTree.getBoundingBox().Intersects(growthRect))
                        {
                            __result = true;
                            break;
                        }
                    }
                }
                return false;
            }
        }
		[HarmonyPatch(typeof(Tree), "performTreeFall")]
		public class Tree_performTreeFall_Patch
        {
            public static void Postfix(Tree __instance)
			{
                if (Config.ModEnabled && __instance.growthStage.Value >= 5 && __instance.modData.TryGetValue(modKey, out var str) && IsGiantTree(__instance, str) && str == "0")
                {
                    foreach (var tile in GetOtherTreeTiles(__instance.Tile))
                    {
                        if(__instance.Location.terrainFeatures.TryGetValue(tile, out var tf) && tf is Tree tree)
                            AccessTools.Method(typeof(Tree), "performTreeFall").Invoke(tree, new object[] { null, 0, tile });
                    }
                }
            }
        }
    }
}

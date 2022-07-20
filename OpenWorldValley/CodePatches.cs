using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;

namespace OpenWorldValley
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawBackground))]
        public class GameLocation_drawBackground_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !mapDict.TryGetValue(__instance.Name, out Dictionary<string, int[]> adjDict))
                    return;
                foreach(var key in adjDict.Keys)
                {
                    GameLocation loc = Game1.getLocationFromName(key);
                    Location offset = new Location(adjDict[key][0], adjDict[key][1]) * 64;
                    Vector2 offsetTile = new Vector2(adjDict[key][0], adjDict[key][1]);
                    Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                    DrawLayer(loc.Map.GetLayer("Back"), Game1.mapDisplayDevice, Game1.viewport, offset, false, 4);
                    Vector2 tile = default(Vector2);
                    for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
                    {
                        for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
                        {
                            tile.X = x;
                            tile.Y = y;
                            if (loc.terrainFeatures.TryGetValue(tile, out TerrainFeature feat) && feat is Flooring)
                            {
                                feat.draw(Game1.spriteBatch, tile + offsetTile);
                            }
                        }
                    }
                    DrawLayer(loc.Map.GetLayer("Buildings"), Game1.mapDisplayDevice, Game1.viewport, offset, false, 4);
                    var viewport = Game1.viewport;
                    Game1.viewport = new xTile.Dimensions.Rectangle(viewport.X - offset.X, viewport.Y - offset.Y, viewport.Width, viewport.Height);
                    loc.draw(Game1.spriteBatch);
                    Game1.viewport = viewport;
                    DrawLayer(loc.Map.GetLayer("Front"), Game1.mapDisplayDevice, Game1.viewport, offset, false, 4);
                    Game1.mapDisplayDevice.EndScene();
                    Game1.spriteBatch.End();
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                    for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
                    {
                        for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
                        {
                            tile.X = x;
                            tile.Y = y;
                            if (loc.terrainFeatures.TryGetValue(tile - offsetTile, out TerrainFeature feat) && !(feat is Flooring))
                            {
                                feat.draw(Game1.spriteBatch, tile);
                            }
                        }
                    }
                    var af = loc.Map.GetLayer("AlwaysFront");
                    if (af != null)
                    {
                        DrawLayer(af, Game1.mapDisplayDevice, Game1.viewport, offset, false, 4);
                    }
                    Game1.mapDisplayDevice.EndScene();
                }
            }

            private static void DrawLayer(Layer layer, IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, Location displayOffset, bool v1, int pixelZoom)
            {
                int tileWidth = pixelZoom * 16;
                int tileHeight = pixelZoom * 16;
                Location tileInternalOffset = new Location(Wrap(mapViewport.X, tileWidth), Wrap(mapViewport.Y, tileHeight));
                int tileXMin = (mapViewport.X >= 0) ? (mapViewport.X / tileWidth) : ((mapViewport.X - tileWidth + 1) / tileWidth);
                int tileYMin = (mapViewport.Y >= 0) ? (mapViewport.Y / tileHeight) : ((mapViewport.Y - tileHeight + 1) / tileHeight);
                if (tileXMin < 0)
                {
                    displayOffset.X -= tileXMin * tileWidth;
                    tileXMin = 0;
                }
                if (tileYMin < 0)
                {
                    displayOffset.Y -= tileYMin * tileHeight;
                    tileYMin = 0;
                }
                int tileColumns = 1 + (mapViewport.Size.Width - 1) / tileWidth;
                int tileRows = 1 + (mapViewport.Size.Height - 1) / tileHeight;
                if (tileInternalOffset.X != 0)
                {
                    tileColumns++;
                }
                if (tileInternalOffset.Y != 0)
                {
                    tileRows++;
                }
                Location tileLocation = displayOffset - tileInternalOffset;
                int offset = 0;
                tileLocation.Y = displayOffset.Y - tileInternalOffset.Y - tileYMin * 64;
                for (int tileY = 0; tileY < layer.LayerSize.Height; tileY++)
                {
                    tileLocation.X = displayOffset.X - tileInternalOffset.X - tileXMin * 64;
                    for (int tileX = 0; tileX < layer.LayerSize.Width; tileX++)
                    {
                        Tile tile = layer.Tiles[tileX, tileY];
                        if (tile != null)
                        {
                            displayDevice.DrawTile(tile, tileLocation, (tileY * (16 * pixelZoom) + 16 * pixelZoom + offset) / 10000f);
                        }
                        tileLocation.X += tileWidth;
                    }
                    tileLocation.Y += tileHeight;
                }
            }

            private static int Wrap(int value, int span)
            {
                value %= span;
                if (value < 0)
                {
                    value += span;
                }
                return value;
            }
        }
        
        [HarmonyPatch(typeof(Game1), nameof(Game1.UpdateViewPort))]
        public class Game1_UpdateViewPort_Patch
        {
            public static void Prefix(ref bool overrideFreeze)
            {
                if (!Config.ModEnabled)
                    return;
                overrideFreeze = true;
                Game1.forceSnapOnNextViewportUpdate = true;
                Game1.currentLocation.forceViewportPlayerFollow = true;
            }
        }
        
        [HarmonyPatch(typeof(Game1), nameof(Game1.isOutdoorMapSmallerThanViewport))]
        public class Game1_isOutdoorMapSmallerThanViewport_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                return !Config.ModEnabled;
            }
        }
        
        [HarmonyPatch(typeof(ScreenFade), nameof(ScreenFade.FadeScreenToBlack))]
        public class ScreenFade_FadeScreenToBlack_Patch
        {
            public static bool Prefix(ScreenFade __instance)
            {
                if (!Config.ModEnabled || !Game1.isWarping)
                    return true;


                AccessTools.FieldRefAccess<ScreenFade, Func<bool>>(__instance, "onFadeToBlackComplete")();
                return false;
            }
        }
        
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingWithWarp))]
        public class GameLocation_isCollidingWithWarp_Patch
        {
            public static void Postfix(GameLocation __instance, Character character, ref Warp __result)
            {
                if (!Config.ModEnabled || character != Game1.player || __result == null)
                    return;
                GameLocation target = Game1.getLocationFromName(__result.TargetName);
                if (target is null)
                    return;
                if (__result.X <= 0 && character.FacingDirection != 3 || __result.Y <= 0 && character.FacingDirection != 0 || __result.X >= __instance.Map.GetLayer("Back").LayerWidth && character.FacingDirection != 1 || __result.Y >= __instance.Map.GetLayer("Back").LayerHeight&& character.FacingDirection != 2)
                    __result = null;
            }
        }
        
        [HarmonyPatch(typeof(Game1), "onFadeToBlackComplete")]
        public class Game1_onFadeToBlackComplete_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log("Patching Game1.onFadeToBlackComplete");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.PropertySetter(typeof(Character), nameof(Character.Position)) && codes[i + 3].opcode == OpCodes.Ldstr && (string)codes[i + 3].operand == "UndergroundMine")
                    {
                        SMonitor.Log("Intercepting warped position setting");
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetWarpedPosition))));
                        i++;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        private static Vector2 GetWarpedPosition(Vector2 position)
        {
            if (!Config.ModEnabled || !Game1.currentLocation.IsOutdoors)
                return position;
            var offset = 64;
            if (position.X == 0)
                position.X -= offset;
            else if (position.Y == 0)
                position.Y -= offset;
            else if (position.X == Game1.currentLocation.Map.GetLayer("Back").LayerWidth * 64 - 64)
                position.X += offset;
            else if (position.Y == Game1.currentLocation.Map.GetLayer("Back").LayerHeight * 64)
                position.Y += offset;
            var x = Game1.player.position.X % 64;
            var y = Game1.player.position.Y % 64 - 16;
            position += new Vector2(x, y);
            return position;
        }
    }
}
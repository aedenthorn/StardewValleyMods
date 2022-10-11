using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PersonalTravellingCart
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Horse), nameof(Horse.update))]
        public class Horse_update_Patch
        {
            public static void Postfix(Horse __instance)
            {
                if (!Config.ModEnabled || string.IsNullOrEmpty(Config.CurrentCart) || __instance.Name.StartsWith("tractor/") || !cartDict.TryGetValue(Config.CurrentCart, out PersonalCartData data) || !__instance.mounting.Value || __instance.rider is null)
                    return;
                __instance.rider.faceDirection(__instance.FacingDirection);

            }
        }
        [HarmonyPatch(typeof(Horse), nameof(Horse.draw))]
        public class Horse_draw_Patch
        {
            public static void Prefix(Horse __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance.getOwner() is null || __instance.Name.StartsWith("tractor/") || string.IsNullOrEmpty(Config.CurrentCart) || !cartDict.TryGetValue(Config.CurrentCart, out PersonalCartData data) || __instance.currentLocation != Game1.currentLocation)
                    return;

                DirectionData directionData = GetDirectionData(data, __instance.FacingDirection);
                Vector2 cartPosition = __instance.Position + directionData.cartOffset;

                Rectangle backRect = directionData.backRect;
                Rectangle frontRect = directionData.frontRect;


                if(directionData.frames > 0)
                {
                    int frame = ((__instance.FacingDirection % 2 == 0) ? (int)__instance.Position.Y : (int)__instance.Position.X) / directionData.frameRate % directionData.frames;
                    backRect.Location += new Point(backRect.Width * frame, 0);
                    frontRect.Location += new Point(frontRect.Width * frame, 0);
                }

                b.Draw(data.spriteSheet, Game1.GlobalToLocal(cartPosition), backRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (__instance.Position.Y + 64f) / 10000f - 0.005f);
                b.Draw(data.spriteSheet, Game1.GlobalToLocal(cartPosition), frontRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (__instance.Position.Y + 64f) / 10000f + 0.0001f);
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static bool Prefix(Farmer __instance, SpriteBatch b, ref float[] __state)
            {
                if (!Config.ModEnabled || string.IsNullOrEmpty(Config.CurrentCart) || !cartDict.TryGetValue(Config.CurrentCart, out PersonalCartData data))
                    return true;
                if (!__instance.isRidingHorse())
                {
                    if (Game1.isWarping && Game1.locationRequest.Name == locationName.Value)
                    {
                        Game1.displayFarmer = false;
                        return false;
                    }
                    return true;
                }
                if (__instance.mount.Name.StartsWith("tractor/"))
                    return true;
                __state = new float[] {
                    __instance.xOffset,
                    __instance.yOffset
                };
                DirectionData directionData = GetDirectionData(data, __instance.FacingDirection);
                __instance.xOffset += directionData.playerOffset.X;
                __instance.yOffset += directionData.playerOffset.Y;
                return true;
            }
            public static void Postfix(Farmer __instance, SpriteBatch b, float[] __state)
            {
                if (!Config.ModEnabled || __state is null)
                    return;
                __instance.xOffset = __state[0];
                __instance.yOffset = __state[1];
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
        public class Game1_loadForNewGame_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;

                cartDict = SHelper.GameContent.Load<Dictionary<string, PersonalCartData>>(dataPath);
                foreach (var key in cartDict.Keys.ToArray())
                {
                    if (string.IsNullOrEmpty(Config.CurrentCart) || Config.CurrentCart == "_default")
                    {
                        Config.CurrentCart = key;
                        SHelper.WriteConfig(Config);
                    }
                    if (!string.IsNullOrEmpty(cartDict[key].spriteSheetPath))
                    {
                        cartDict[key].spriteSheet = SHelper.GameContent.Load<Texture2D>(cartDict[key].spriteSheetPath);
                    }
                    else
                    {
                        cartDict[key].spriteSheet = SHelper.ModContent.Load<Texture2D>("assets/cart.png");
                    }
                }
                if (string.IsNullOrEmpty(Config.CurrentCart) || !cartDict.ContainsKey(Config.CurrentCart))
                {
                    Config.CurrentCart = "_default";
                    SHelper.WriteConfig(Config);
                    cartDict["_default"] = new PersonalCartData() { spriteSheet = SHelper.ModContent.Load<Texture2D>("assets/cart.png") };
                }
                SMonitor.Log($"Loaded {cartDict.Count} carts, using {Config.CurrentCart}");

                string mapAssetKey;
                if (Config.CurrentCart == null || !cartDict.TryGetValue(Config.CurrentCart, out PersonalCartData data) || data.mapPath is null)
                {
                    mapAssetKey = SHelper.ModContent.GetInternalAssetName("assets/Cart.tmx").BaseName;
                }
                else
                {
                    mapAssetKey = data.mapPath;
                }
                locationName.Value = "PersonalCart" + Math.Abs(Game1.player.UniqueMultiplayerID);
                DecoratableLocation location = new DecoratableLocation(mapAssetKey, locationName.Value) { IsOutdoors = false, IsFarm = false, IsGreenhouse = false };
                location.ReadWallpaperAndFloorTileData();
                //location.Map.Properties.Add("WallIDs", "Wall");
                //location.Map.Properties.Add("FloorIDs", "Floor");
                Game1.locations.Add(location);
                SHelper.GameContent.InvalidateCache("Data/Locations");
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.canGrabSomethingFromHere))]
        public class Utility_canGrabSomethingFromHere_Patch
        {
            public static bool Prefix(int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !Game1.player.isRidingHorse() || Game1.player.mount.Name.StartsWith("tractor/") || string.IsNullOrEmpty(Config.CurrentCart) || !cartDict.TryGetValue(Config.CurrentCart, out PersonalCartData data) || !IsMouseInBoundingBox(data))
                    return true;
                Game1.mouseCursor = 2;
                __result = true;
                return false;
            }

        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !who.isRidingHorse() || Game1.player.mount.Name.StartsWith("tractor/") || string.IsNullOrEmpty(Config.CurrentCart) || !cartDict.TryGetValue(Config.CurrentCart, out PersonalCartData data) ||  !IsMouseInBoundingBox(data))
                    return true;
                who.mount.dismount(false);
                who.modData[lastLocationKey] = who.currentLocation.Name;
                who.modData[lastXKey] = who.getTileLocationPoint().X + "";
                who.modData[lastYKey] = who.getTileLocationPoint().Y + "";
                SMonitor.Log($"Warping to Cart");
                Game1.warpFarmer(locationName.Value, 6, 6, false);
                __result = true;
                return false;
            }

        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawBackground))]
        public class GameLocation_drawBackground_Patch
        {
            public static Tile lastTile;
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled || __instance.Name != locationName.Value || !Game1.player.modData.TryGetValue(lastLocationKey, out string lastLoc))
                    return;
                var playerTile = __instance.Map.GetLayer("Back").PickTile(new Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y), Game1.viewport.Size);
                if (playerTile is not null && playerTile != lastTile)
                {
                    if(playerTile.Properties.ContainsKey("Leave"))
                    {
                        WarpOutOfCart(Game1.player);
                    }
                }
                lastTile = playerTile;
                GameLocation loc = Game1.getLocationFromName(lastLoc);
                int lastX = int.Parse(Game1.player.modData[lastXKey]) * 64 - __instance.Map.GetLayer("Back").LayerWidth * 32;
                int lastY = int.Parse(Game1.player.modData[lastYKey]) * 64 - __instance.Map.GetLayer("Back").LayerHeight * 32;
                Location offset = new Location(-lastX, -lastY);
                Vector2 offsetTile = new Vector2(-lastX, -lastY) / 64f;
                Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                DrawLayer(loc.Map.GetLayer("Back"), Game1.mapDisplayDevice, Game1.viewport, offset, false, 4);
                Vector2 tile = default(Vector2);
                for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
                {
                    for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
                    {
                        tile.X = x;
                        tile.Y = y;
                        if (loc.terrainFeatures.TryGetValue(tile - offsetTile, out TerrainFeature feat) && (feat is Flooring || feat is HoeDirt))
                        {
                            feat.draw(Game1.spriteBatch, tile);
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
                        if (loc.terrainFeatures.TryGetValue(tile - offsetTile, out TerrainFeature feat) && feat is not Flooring && feat is not HoeDirt)
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
    }
}
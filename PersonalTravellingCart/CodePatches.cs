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
                    if (!string.IsNullOrEmpty(cartDict[key].spriteSheetPath))
                    {
                        cartDict[key].spriteSheet = SHelper.GameContent.Load<Texture2D>(cartDict[key].spriteSheetPath);
                    }
                    else
                    {
                        cartDict[key].spriteSheet = SHelper.ModContent.Load<Texture2D>("assets/cart.png");
                    }
                }
                SMonitor.Log($"Loaded {cartDict.Count} custom carts");

                cartDict["_default"] = new PersonalCartData() { spriteSheet = SHelper.ModContent.Load<Texture2D>("assets/cart.png") };

                var mapAssetKey = SHelper.ModContent.GetInternalAssetName("assets/Cart.tmx").BaseName;
                if (Game1.player.modData.TryGetValue(cartKey, out string which) && cartDict.TryGetValue(which, out PersonalCartData data) && data.mapPath is not null)
                {
                    mapAssetKey = data.mapPath;
                }
                if(Config.ThisPlayerCartLocationName is null)
                {
                    Config.ThisPlayerCartLocationName = locPrefix + Guid.NewGuid().ToString("N");
                    SHelper.WriteConfig(Config);
                }
                SMonitor.Log($"adding location {Config.ThisPlayerCartLocationName}");
                DecoratableLocation location = new DecoratableLocation(mapAssetKey, Config.ThisPlayerCartLocationName) { IsOutdoors = false, IsFarm = false, IsGreenhouse = false };
                location.ReadWallpaperAndFloorTileData();
                //location.Map.Properties.Add("WallIDs", "Wall");
                //location.Map.Properties.Add("FloorIDs", "Floor");
                Game1.locations.Add(location);
                SHelper.GameContent.InvalidateCache("Data/Locations");
            }
        }
        [HarmonyPatch(typeof(Horse), nameof(Horse.update))]
        public class Horse_update_Patch
        {
            public static void Postfix(Horse __instance)
            {
                if (!Config.ModEnabled || __instance.Name.StartsWith("tractor/") || !__instance.mounting.Value || __instance.rider is null)
                    return;
                __instance.rider.faceDirection(__instance.FacingDirection);

            }
        }
        [HarmonyPatch(typeof(Horse), nameof(Horse.draw))]
        public class Horse_draw_Patch
        {
            public static void Prefix(Horse __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance.getOwner() is null || __instance.Name.StartsWith("tractor/") || __instance.currentLocation != Game1.currentLocation)
                    return;

                if (!__instance.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                {
                    which = "_default";
                    __instance.modData[cartKey] = which;
                }

                DirectionData directionData = GetDirectionData(cartDict[which], __instance.FacingDirection);
                Vector2 cartPosition = __instance.Position + directionData.cartOffset;

                Rectangle backRect = directionData.backRect;
                Rectangle frontRect = directionData.frontRect;


                if (directionData.frames > 0)
                {
                    int frame = ((__instance.FacingDirection % 2 == 0) ? (int)__instance.Position.Y : (int)__instance.Position.X) / directionData.frameRate % directionData.frames;
                    backRect.Location += new Point(backRect.Width * frame, 0);
                    frontRect.Location += new Point(frontRect.Width * frame, 0);
                }

                b.Draw(cartDict[which].spriteSheet, Game1.GlobalToLocal(cartPosition), backRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (__instance.Position.Y + 64f) / 10000f - 0.005f);
                b.Draw(cartDict[which].spriteSheet, Game1.GlobalToLocal(cartPosition), frontRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (__instance.Position.Y + 64f) / 10000f + 0.0001f);
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static bool Prefix(Farmer __instance, SpriteBatch b, ref float[] __state)
            {
                if (!Config.ModEnabled)
                    return true;
                if (!__instance.isRidingHorse())
                {
                    if (Game1.isWarping && Game1.locationRequest.Name.StartsWith(locPrefix))
                    {
                        Game1.displayFarmer = false;
                        return false;
                    }
                    return true;
                }
                if (__instance.mount.Name.StartsWith("tractor/"))
                    return true;

                if (!__instance.mount.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                {
                    which = "_default";
                    __instance.mount.modData[cartKey] = which;
                }
                __state = new float[] {
                    __instance.xOffset,
                    __instance.yOffset
                };
                DirectionData directionData = GetDirectionData(cartDict[which], __instance.FacingDirection);
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
        [HarmonyPatch(typeof(Utility), nameof(Utility.canGrabSomethingFromHere))]
        public class Utility_canGrabSomethingFromHere_Patch
        {
            public static bool Prefix(int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || Game1.currentLocation is null)
                    return true;
                foreach(var c in Game1.currentLocation.characters)
                {
                    if(c is Horse && !c.Name.StartsWith("tractor/"))
                    {
                        Farmer owner = (c as Horse).getOwner();
                        if (owner is null)
                            continue;
                        if (!owner.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                        {
                            which = "_default";
                        }
                        if (IsMouseInBoundingBox(c, cartDict[which]))
                        {
                            Game1.mouseCursor = 2;
                            __result = true;
                            return false;
                        }
                    }
                }
                foreach(var farmer in Game1.currentLocation.farmers)
                {
                    if (!farmer.isRidingHorse())
                        continue;
                    if (!farmer.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                    {
                        which = "_default";
                    }
                    if (IsMouseInBoundingBox(farmer, cartDict[which]))
                    {
                        Game1.mouseCursor = 2;
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                        return true;

                foreach (var c in Game1.currentLocation.characters)
                {
                    if (c is Horse && !c.Name.StartsWith("tractor/") && (c as Horse).ownerId.Value != 0)
                    {
                        if ((c as Horse).getOwner()?.modData.TryGetValue(cartKey, out string which) != true || !cartDict.ContainsKey(which))
                        {
                            which = "_default";
                        }
                        if (IsMouseInBoundingBox(c, cartDict[which]))
                        {
                            Farmer owner = (c as Horse).getOwner();
                            if (owner is null)
                            {
                                SMonitor.Log($"horse's owner is not online");
                                return true;
                            }
                            if (who.mount is not null)
                                who.mount.dismount(false);

                            if(!owner.modData.TryGetValue(locKey, out string locName))
                            {
                                SMonitor.Log($"horse's owner has no location");
                                return true;
                            }

                            var location = Game1.getLocationFromName(locName);
                            if(location is null)
                            {
                                SMonitor.Log($"Location {locName} does not exist");
                                return true;
                            }
                            SMonitor.Log($"Warping to Cart {locName}");
                            Game1.warpFarmer(new LocationRequest(locName, false, location), cartDict[which].entryTile.X, cartDict[which].entryTile.Y, 0);
                            __result = true;
                            return false;
                        }
                    }
                }
                foreach (var farmer in Game1.currentLocation.farmers)
                {
                    if (!farmer.isRidingHorse())
                        continue;
                    if (!farmer.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                    {
                        which = "_default";
                    }
                    if (IsMouseInBoundingBox(farmer, cartDict[which]))
                    {
                        if (who.mount is not null)
                            who.mount.dismount(false);

                        if (!farmer.modData.TryGetValue(locKey, out string locName))
                        {
                            SMonitor.Log($"horse's owner has no location");
                            return true;
                        }
                        var location = Game1.getLocationFromName(locName);
                        if (location is null)
                        {
                            SMonitor.Log($"Location {locName} does not exist");
                            return true;
                        }
                        SMonitor.Log($"Warping to Cart {locName}");
                        Game1.warpFarmer(new LocationRequest(locName, false, location), cartDict[which].entryTile.X, cartDict[which].entryTile.Y, 0);
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.drawWeather))]
        public class Game1_drawWeather_Patch
        {
            public static void Prefix(GameTime time, RenderTarget2D target_screen)
            {
                if (!skip)
                {
                    deltaTime = time;
                    screen = target_screen;
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawBackground))]
        public class GameLocation_drawBackground_Patch
        {
            public static Tile lastTile;
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !__instance.Name.StartsWith(locPrefix))
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
                Horse horse = null;
                foreach (var l in Game1.locations)
                {
                    foreach (var c in l.characters)
                    {
                        if (c is Horse && (c as Horse).getOwner()?.modData.TryGetValue(locKey, out string locName) == true && locName == __instance.Name)
                        {
                            horse = c as Horse;
                            goto gothorse;
                        }
                    }
                    foreach (var c in l.farmers)
                    {
                        if (c.isRidingHorse() && c.modData.TryGetValue(locKey, out string locName) == true && locName == __instance.Name)
                        {
                            horse = c.mount;
                            goto gothorse;
                        }
                    }
                }
            gothorse:
                if (horse is null)
                {
                    return;
                }

                GameLocation loc = horse.currentLocation;
                int horseX = horse.getTileX() * 64 - __instance.Map.GetLayer("Back").LayerWidth * 32;
                int horseY = horse.getTileY() * 64 - __instance.Map.GetLayer("Back").LayerHeight * 32;
                Location offset = new Location(-horseX, -horseY);
                Vector2 offsetTile = new Vector2(-horseX, -horseY) / 64f;
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
                var currentLoc = Game1.currentLocation;
                Game1.currentLocation = loc;
                skip = true;
                Game1.game1.drawWeather(deltaTime, Game1.game1.screen);
                Game1.updateWeather(deltaTime);
                skip = false;
                Game1.currentLocation = currentLoc;
            }
        }
    }
}
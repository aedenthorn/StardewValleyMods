using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
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

                cartDict[defaultKey] = new PersonalCartData() { spriteSheet = SHelper.ModContent.Load<Texture2D>("assets/cart.png"), mapPath = SHelper.ModContent.GetInternalAssetName("assets/Cart.tmx").BaseName };

                string mapAssetKey;
                if (Game1.player.modData.TryGetValue(cartKey, out string which) && cartDict.TryGetValue(which, out PersonalCartData data) && data.mapPath is not null)
                {
                    mapAssetKey = data.mapPath;
                }
                else
                {
                    mapAssetKey = SHelper.ModContent.GetInternalAssetName("assets/Cart.tmx").BaseName;
                }

                SMonitor.Log($"adding location {thisPlayerCartLocation}");
                DecoratableLocation location = new DecoratableLocation(mapAssetKey, thisPlayerCartLocation) { IsOutdoors = false, IsFarm = false, IsGreenhouse = false };
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
                if (!Config.ModEnabled || __instance.Name.StartsWith("tractor/") || __instance.currentLocation != Game1.currentLocation)
                    return;
                var owner = __instance.getOwner();
                if (owner is null || owner.modData.ContainsKey(parkedKey))
                    return;

                if (!owner.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                {
                    which = defaultKey;
                    owner.modData[cartKey] = which;
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
                if (__instance.mount.Name.StartsWith("tractor/") || __instance.modData.ContainsKey(parkedKey))
                    return true;

                if (!__instance.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                {
                    which = defaultKey;
                    __instance.modData[cartKey] = which;
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
                if (clickableCart is not null)
                {
                    Game1.mouseCursor = 2;
                    __result = true;
                    return false;
                }
                foreach (var c in Game1.currentLocation.characters)
                {
                    if(c is Horse && !c.Name.StartsWith("tractor/"))
                    {
                        Farmer owner = (c as Horse).getOwner();
                        if (owner is null || owner.modData.ContainsKey(parkedKey))
                            continue;
                        if (!owner.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                        {
                            which = defaultKey;
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
                    if (!farmer.isRidingHorse() || farmer.modData.ContainsKey(parkedKey))
                        continue;
                    if (!farmer.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                    {
                        which = defaultKey;
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
                if (clickableCart is not null)
                {
                    SMonitor.Log("Clicked on clickable parked cart");
                    var location = Game1.getLocationFromName(clickableCart.location);
                    if (location is null)
                    {
                        SMonitor.Log($"Location {clickableCart.location} does not exist");
                        return true;
                    }
                    SMonitor.Log($"Warping to Cart {clickableCart.location}");
                    if (who.mount is not null)
                        who.mount.dismount(false);
                    who.Position = clickableCart.position;
                    var data = GetCartData(clickableCart.whichCart);
                    Game1.warpFarmer(new LocationRequest(clickableCart.location, false, location), data.entryTile.X, data.entryTile.Y, 0);
                    __result = true;
                    return false;
                }

                foreach (var c in Game1.currentLocation.characters)
                {
                    if (c is Horse && !c.Name.StartsWith("tractor/"))
                    {
                        var owner = (c as Horse).getOwner();
                        if (owner is null || owner.modData.ContainsKey(parkedKey))
                            continue;
                        if (owner?.modData.TryGetValue(cartKey, out string which) != true || !cartDict.ContainsKey(which))
                        {
                            which = defaultKey;
                        }
                        if (IsMouseInBoundingBox(c, cartDict[which]))
                        {
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
                            if (who.mount is not null)
                                who.mount.dismount(false);
                            Game1.warpFarmer(new LocationRequest(locName, false, location), cartDict[which].entryTile.X, cartDict[which].entryTile.Y, 0);
                            __result = true;
                            return false;
                        }
                    }
                }
                foreach (var farmer in Game1.currentLocation.farmers)
                {
                    if (!farmer.isRidingHorse() || farmer.modData.ContainsKey(parkedKey))
                        continue;
                    if (!farmer.modData.TryGetValue(cartKey, out string which) || !cartDict.ContainsKey(which))
                    {
                        which = defaultKey;
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
                        var locations = Game1.getLocationFromName(locName);
                        SMonitor.Log($"Warping to Cart {locName}");
                        Game1.warpFarmer(new LocationRequest(locName, false, location), cartDict[which].entryTile.X, cartDict[which].entryTile.Y, 0);
                        __result = true;
                        return false;
                    }
                }
                return true;
            }

        }
        [HarmonyPatch(typeof(Stable), nameof(Stable.grabHorse))]
        public class Stable_grabHorse_Patch
        {
            public static bool Prefix(Stable __instance)
            {
                if (!Config.ModEnabled)
                    return true;

                Horse horse = Utility.findHorse(__instance.HorseId);
                return horse is null;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.CanPlaceThisFurnitureHere))]
        public class GameLocation_CanPlaceThisFurnitureHere_Patch
        {
            public static bool Prefix(GameLocation __instance, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.StartsWith(locPrefix))
                    return true;
                __result = true;
                return false;
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
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw))]
        public class GameLocation_draw_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(parkedKey, out string parkedString))
                    return;
                clickableCart = null;
                List<ParkedCart> carts = JsonConvert.DeserializeObject<List<ParkedCart>>(parkedString);
                foreach (var cart in carts)
                {
                    var data = GetCartData(cart.whichCart);
                    var ddata = data.GetDirectionData(cart.facing);
                    b.Draw(data.spriteSheet, Game1.GlobalToLocal(cart.position + ddata.cartOffset), ddata.backRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (cart.position.Y + ddata.cartOffset.Y + (ddata.hitchRect.Y + ddata.hitchRect.Height / 2 + 16) * 4 - 1) / 10000f);
                    b.Draw(data.spriteSheet, Game1.GlobalToLocal(cart.position + ddata.cartOffset), ddata.frontRect, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, (cart.position.Y + ddata.cartOffset.Y + (ddata.hitchRect.Y + ddata.hitchRect.Height / 2 + 16) * 4 + 1) / 10000f);
                    if(!drawingExterior && new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(cart.position) + ddata.cartOffset) + new Point(ddata.clickRect.Location.X * 4, ddata.clickRect.Location.Y * 4), new Point(ddata.clickRect.Size.X * 4,ddata.clickRect.Size.Y * 4)).Contains(Game1.getMousePosition()))
                    {
                        clickableCart = cart;
                    }
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

                int positionX = 0;
                int positionY = 0;
                GameLocation loc = null;
                Point outerTile = Point.Zero;
                bool shouldWarp = false;
                var playerTile = __instance.Map.GetLayer("Back").PickTile(new Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y), Game1.viewport.Size);
                if (playerTile is not null && playerTile != lastTile && playerTile.Properties.ContainsKey("Leave"))
                {
                    shouldWarp = true;
                }
                lastTile = playerTile;

                if (!Config.DrawCartExterior && !shouldWarp)
                    return;

                if (!Game1.player.modData.ContainsKey(parkedKey))
                {
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

                    loc = horse.currentLocation;
                    outerTile = horse.getTileLocationPoint();
                    positionX = outerTile.X * 64 - __instance.Map.GetLayer("Back").LayerWidth * 32;
                    positionY = outerTile.Y * 64 - __instance.Map.GetLayer("Back").LayerHeight * 32;
                }
                else
                {
                    foreach (var l in Game1.locations)
                    {
                        if (!l.modData.TryGetValue(parkedKey, out string parkedString))
                            continue;
                        List<ParkedCart> carts = JsonConvert.DeserializeObject<List<ParkedCart>>(parkedString);
                        foreach (var cart in carts)
                        {
                            if (cart.location == __instance.Name)
                            {
                                var data = GetCartData(cart.whichCart);
                                var ddata = data.GetDirectionData(cart.facing);
                                var position = cart.position + ddata.cartOffset + ddata.backRect.Size.ToVector2() * 2;
                                positionX = (int)position.X - __instance.Map.GetLayer("Back").LayerWidth * 32;
                                positionY = (int)position.Y - __instance.Map.GetLayer("Back").LayerHeight * 32;
                                outerTile = new Point((int)(cart.position.X / 64), (int)(cart.position.Y / 64));
                                loc = l;
                                break;
                            }
                        }
                    }
                }

                if (shouldWarp)
                {
                    if (loc is null)
                    {
                        SMonitor.Log($"Warping to farm");
                        Game1.warpFarmer("Farm", Game1.getFarm().GetMainFarmHouseEntry().X, Game1.getFarm().GetMainFarmHouseEntry().Y, false);
                    }
                    else
                    {
                        SMonitor.Log($"Warping to last location");
                        Game1.warpFarmer(new LocationRequest(loc.Name, false, loc), outerTile.X, outerTile.Y, 2);
                    }
                    return;
                }

                if (loc is null)
                    return;

                Location offset = new Location(-positionX, -positionY);
                Vector2 offsetTile = new Vector2(-positionX, -positionY) / 64f;

                /*
                Game1.spriteBatch.End();
                Game1.SetRenderTarget(Game1.lightmap);
                Game1.game1.GraphicsDevice.Clear(Color.White * 0f);
                Matrix lighting_matrix = Matrix.Identity;
                if (Game1.game1.useUnscaledLighting)
                {
                    lighting_matrix = Matrix.CreateScale(Game1.options.zoomLevel);
                }
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, new Matrix?(lighting_matrix));
                Color lighting;
                if (!Game1.ambientLight.Equals(Color.White) && (!Game1.IsRainingHere(loc) || !loc.IsOutdoors))
                {
                    lighting = Game1.ambientLight;
                }
                else
                {
                    lighting = Game1.outdoorLight;
                }
                float light_multiplier = 1f;
                if (Game1.player.hasBuff(26))
                {
                    if (lighting == Color.White)
                    {
                        lighting = new Color(0.75f, 0.75f, 0.75f);
                    }
                    else
                    {
                        lighting.R = (byte)Utility.Lerp((float)lighting.R, 255f, 0.5f);
                        lighting.G = (byte)Utility.Lerp((float)lighting.G, 255f, 0.5f);
                        lighting.B = (byte)Utility.Lerp((float)lighting.B, 255f, 0.5f);
                    }
                    light_multiplier = 0.33f;
                }
                Game1.spriteBatch.Draw(Game1.staminaRect, Game1.lightmap.Bounds, lighting);
                foreach (LightSource lightSource in Game1.currentLightSources)
                {
                    if ((!Game1.IsRainingHere(loc) && !Game1.isDarkOut()) || lightSource.lightContext.Value != LightSource.LightContext.WindowLight)
                    {
                        if (lightSource.PlayerID != 0L && lightSource.PlayerID != Game1.player.UniqueMultiplayerID)
                        {
                            Farmer farmer = Game1.getFarmerMaybeOffline(lightSource.PlayerID);
                            if (farmer == null || (farmer.currentLocation != null && farmer.currentLocation.Name != Game1.currentLocation.Name) || farmer.hidden.Value)
                            {
                                continue;
                            }
                        }
                        if (Utility.isOnScreen(lightSource.position.Value, (int)(lightSource.radius.Value * 64f * 4f)))
                        {
                            Game1.spriteBatch.Draw(lightSource.lightTexture, Game1.GlobalToLocal(Game1.viewport, lightSource.position.Value) / (float)(Game1.options.lightingQuality / 2), new Microsoft.Xna.Framework.Rectangle?(lightSource.lightTexture.Bounds), lightSource.color.Value * light_multiplier, 0f, new Vector2((float)(lightSource.lightTexture.Bounds.Width / 2), (float)(lightSource.lightTexture.Bounds.Height / 2)), lightSource.radius.Value / (float)(Game1.options.lightingQuality / 2), SpriteEffects.None, 0.9f);
                        }
                    }
                }
                Game1.spriteBatch.End();
                Game1.SetRenderTarget(Game1.game1.screen);
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                */

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
                drawingExterior = true;
                loc.draw(Game1.spriteBatch);
                drawingExterior = false;
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
                
				if (loc.LightLevel > 0f && Game1.timeOfDay < 2000)
				{
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * Game1.currentLocation.LightLevel);
				}
				if (Game1.screenGlow)
				{
					Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
				}
                if (Config.DrawCartExteriorWeather)
                {
                    var currentLoc = Game1.currentLocation;
                    Game1.currentLocation = loc;
                    skip = true;
                    if (deltaTime is not null)
                    {
                        Game1.game1.drawWeather(deltaTime, Game1.game1.screen);
                        Game1.updateWeather(deltaTime);
                    }

                    Game1.spriteBatch.End();
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, AccessTools.FieldRefAccess<Game1, BlendState>(Game1.game1, "lightingBlend"), SamplerState.LinearClamp, null, null, null, null);
                    Viewport vp = Game1.game1.GraphicsDevice.Viewport;
                    vp.Bounds = ((Game1.game1.screen != null) ? Game1.game1.screen.Bounds : Game1.game1.GraphicsDevice.PresentationParameters.Bounds);
                    Game1.game1.GraphicsDevice.Viewport = vp;
                    float render_zoom = (float)(Game1.options.lightingQuality / 2);
                    if (Game1.game1.useUnscaledLighting)
                    {
                        render_zoom /= Game1.options.zoomLevel;
                    }
                    Game1.spriteBatch.Draw(Game1.lightmap, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(Game1.lightmap.Bounds), Color.White, 0f, Vector2.Zero, render_zoom, SpriteEffects.None, 1f);
                    if (Game1.IsRainingHere(null) && Game1.currentLocation.IsOutdoors && !(Game1.currentLocation is Desert))
                    {
                        Game1.spriteBatch.Draw(Game1.staminaRect, vp.Bounds, Color.OrangeRed * 0.45f);
                    }
                    Game1.spriteBatch.End();
                    Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);


                    skip = false;
                    Game1.currentLocation = currentLoc;
                }
                
            }
        }
    }
}
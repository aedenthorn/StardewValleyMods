using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PersonalTravellingCart
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Horse), nameof(Horse.draw))]
        public class Horse_draw_Patch
        {
            public static void Prefix(Horse __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
                Vector2 bottomPosition = __instance.Position + new Vector2(56, -196);
                Rectangle bottomSource = new Rectangle(0, 0, 109, 71);
                Rectangle topSource = new Rectangle(0,142,109,71);
                if (__instance.FacingDirection == 1)
                {
                    bottomPosition = __instance.Position + new Vector2(-382, -196);
                    bottomSource = new Rectangle(0, 71, 109, 71);
                    topSource = new Rectangle(0, 213, 109, 71);
                }
                b.Draw(cartTexture, Game1.GlobalToLocal(bottomPosition), bottomSource, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, Game1.player.getDrawLayer() - 0.01f);
                b.Draw(cartTexture, Game1.GlobalToLocal(bottomPosition), topSource, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, Game1.player.getDrawLayer()+0.01f);
            }
            public static void Postfix(Horse __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static void Prefix(Farmer __instance, SpriteBatch b, ref float[] __state)
            {
                if (!Config.ModEnabled || !__instance.isRidingHorse())
                    return;
                __state = new float[] {
                    __instance.xOffset,
                    __instance.yOffset
                };
                if(__instance.FacingDirection == 1)
                {
                    __instance.xOffset += 75;
                    __instance.yOffset -= 22;
                }
                else if(__instance.FacingDirection == 3)
                {
                    __instance.xOffset -= 75;
                    __instance.yOffset -= 22;
                }
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
                if (!Config.ModEnabled || !Game1.IsMasterGame)
                    return;
                mapAssetKey = SHelper.ModContent.GetInternalAssetName("assets/Cart.tmx").BaseName;
                GameLocation location = new GameLocation(mapAssetKey, "PersonalCart") { IsOutdoors = false, IsFarm = false, IsGreenhouse = false };
                Game1.locations.Add(location);
                SHelper.GameContent.InvalidateCache("Data/Locations");
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.canGrabSomethingFromHere))]
        public class Utility_canGrabSomethingFromHere_Patch
        {
            public static bool Prefix(int x, int y, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !Game1.player.isRidingHorse() || !IsMouseInBoundingBox())
                    return true;
                Game1.mouseCursor = 6;
                __result = true;
                return false;
            }

        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;

                if(who.currentLocation.Name == "PersonalCart" && tileLocation.X == 6 && tileLocation.Y == 3)
                {
                    SMonitor.Log($"Warping to last location");
                    Game1.warpFarmer("Farm", 66, 44, false);
                    __result = true;
                    return false;
                }
                else if(who.isRidingHorse() && IsMouseInBoundingBox())
                {
                    who.mount.dismount(false);
                    SMonitor.Log($"Warping to Cart");
                    Game1.warpFarmer("PersonalCart", 6, 6, false);
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}
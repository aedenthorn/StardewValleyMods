using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace Dreamscape
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
            {
                if (!Config.EnableMod || !Game1.currentLocation.Name.Equals("Dreamscape") || tileLocation.X != 21 || tileLocation.Y != 9)
                    return true;
                TemporaryAnimatedSprite t = __instance.getTemporarySpriteByID(5858585);
                if (t != null && t is EmilysParrot)
                {
                    (t as EmilysParrot).doAction();
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(EmilysParrot), nameof(EmilysParrot.doAction))]
        public class EmilysParrot_doAction_Patch
        {
            private static string currentMusic;
            public static void Prefix(EmilysParrot __instance, int ___shakeTimer)
            {
                if (!Config.EnableMod || ___shakeTimer <= 200)
                    return;
                if (Game1.currentLocation.Name.Equals("Dreamscape"))
                {
                    var home = Utility.getHomeOfFarmer(Game1.player);
                    var layer = home.map.GetLayer("Buildings");
                    SMonitor.Log($"Warping to FarmHouse");
                    for (int x = 0; x < layer.LayerWidth; x++)
                    {
                        for (int y = 0; y < layer.LayerHeight; y++)
                        {
                            if(layer.Tiles[new Location(x, y)]?.TileIndex == 2173)
                            {
                                Game1.warpFarmer("FarmHouse", x, y + 1, false);
                                Game1.changeMusicTrack(currentMusic);
                                return;
                            }
                        }
                    }
                    Game1.warpFarmer("FarmHouse", home.getEntryLocation().X, home.getEntryLocation().Y, false);
                    Game1.changeMusicTrack(currentMusic);
                }
                else
                {
                    SMonitor.Log($"Warping to Dreamscape");
                    currentMusic = Game1.getMusicTrackName(Game1.MusicContext.Default);
                    Game1.warpFarmer("Dreamscape", 21, 15, false);
                }
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.draw))]
        public class HoeDirt_draw_Patch
        {
            public static void Prefix(HoeDirt __instance, ref Texture2D ___texture)
            {
                if (!Config.EnableMod || !Game1.currentLocation.Name.Equals("Dreamscape"))
                    return;
                ___texture = HoeDirt.snowTexture;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.resetForPlayerEntry))]
        public class GameLocation_resetForPlayerEntry_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.EnableMod || !__instance.Name.Equals("Dreamscape"))
                    return;
                __instance.temporarySprites.Add(new EmilysParrot(new Vector2(21 * 64, 9 * 64)));
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
        public class Game1_loadForNewGame_Patch
        {
            public static void Postfix()
            {
                if (!Config.EnableMod)
                    return;
                mapAssetKey = SHelper.Content.GetActualAssetKey("assets/Dreamscape.tmx");
                GameLocation location = new GameLocation(mapAssetKey, "Dreamscape") { IsOutdoors = false, IsFarm = true, IsGreenhouse = true };
                Game1.locations.Add(location);
                SHelper.Content.InvalidateCache("Data/Locations");
            }
        }
    }
}
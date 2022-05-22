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
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace LoadMenuTweaks
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Tree), "loadTexture")]
        public class Tree_loadTexture_Patch
        {
            public static bool Prefix(Tree __instance, ref Texture2D __result)
            {
                if (!Config.EnableMod || !__instance.currentLocation.Name.Equals("LoadMenuTweaks") || __instance.treeType.Value != 6)
                    return true;
                __result = palmTreeTexture;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.doesTileSinkDebris))]
        public class GameLocation_doesTileSinkDebris_Patch
        {
            public static bool Prefix(GameLocation __instance, int xTile, int yTile, Debris.DebrisType type, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.Name.Equals("LoadMenuTweaks"))
                    return true;
                var tile = __instance.map.GetLayer("Back").PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size);
                if ((tile != null && tile.TileIndex != 26 && tile.TileIndex != 15) || __instance.map.GetLayer("Back3").PickTile(new Location(xTile * 64, yTile * 64), Game1.viewport.Size) is not null)
                {
                    __result = true;
                }
                return false;
            }
        }
        private static int fairyFrame;
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw), new Type[] { typeof(SpriteBatch) })]
        public class GameLocation_draw_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.EnableMod || !__instance.Name.Equals("LoadMenuTweaks"))
                    return;
                fairyFrame++;
                fairyFrame %= 32;

                b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3, 20) * 64 + new Vector2(32, 0)), new Rectangle?(new Rectangle(16 + fairyFrame / 8 * 16, 592, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);

            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.sinkDebris))]
        public class GameLocation_sinkDebris_Patch
        {
            public static bool Prefix(GameLocation __instance, Debris debris, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.Name.Equals("LoadMenuTweaks") || debris.isEssentialItem() || (debris.debrisType.Value == Debris.DebrisType.OBJECT && debris.chunkType.Value == 74))
                    return true;
                __instance.localSound("dwop");
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
            {
                if (!Config.EnableMod || !Game1.currentLocation.Name.Equals("LoadMenuTweaks"))
                    return true;

                if(tileLocation.X == 21 && tileLocation.Y == 9)
                {
                    TemporaryAnimatedSprite t = __instance.getTemporarySpriteByID(5858585);
                    if (t != null && t is EmilysParrot)
                    {
                        (t as EmilysParrot).doAction();
                        return false;
                    }
                }
                else if(new Rectangle(3, 20, 2, 2).Contains(new Point(tileLocation.X, tileLocation.Y)))
                {
                    __instance.localSound("yoba");
                    who.health = who.maxHealth;
                    who.stamina = who.MaxStamina;
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
                if (Game1.currentLocation.Name.Equals("LoadMenuTweaks"))
                {
                    if (!Game1.player.friendshipData.TryGetValue("Emily", out Friendship f) || !f.IsMarried())
                    {
                        Game1.warpFarmer("HaleyHouse", 14, 5, false);
                    }
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
                    SMonitor.Log($"Warping to LoadMenuTweaks");
                    currentMusic = Game1.getMusicTrackName(Game1.MusicContext.Default);
                    Game1.warpFarmer("LoadMenuTweaks", 21, 15, false);
                }
            }
        }
        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.draw))]
        public class HoeDirt_draw_Patch
        {
            public static void Prefix(ref Texture2D ___texture)
            {
                if (!Config.EnableMod || !Game1.currentLocation.Name.Equals("LoadMenuTweaks"))
                    return;
                ___texture = HoeDirt.snowTexture;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.resetForPlayerEntry))]
        public class GameLocation_resetForPlayerEntry_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.EnableMod || !__instance.Name.Equals("LoadMenuTweaks"))
                    return;
                SMonitor.Log($"Building Emily's parrot");
                __instance.temporarySprites.Clear();
                __instance.temporarySprites.Add(new EmilysParrot(new Vector2(21 * 64, 9 * 64)));
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.loadForNewGame))]
        public class Game1_loadForNewGame_Patch
        {
            public static void Postfix()
            {
                if (!Config.EnableMod || !Game1.IsMasterGame)
                    return;
                mapAssetKey = SHelper.Content.GetActualAssetKey("assets/LoadMenuTweaks.tmx");
                GameLocation location = new GameLocation(mapAssetKey, "LoadMenuTweaks") { IsOutdoors = false, IsFarm = true, IsGreenhouse = true };
                Game1.locations.Add(location);
                SHelper.Content.InvalidateCache("Data/Locations");
            }
        }
    }
}
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FlowingMineRocks
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static ModConfig Config;
        public static Dictionary<string, object> outdoorAreas = new Dictionary<string, object>();
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        private static Texture2D tilesheet;
        private static int sheetWidth = 8;
        private static Dictionary<Vector2, bool> flippedObjects = new Dictionary<Vector2, bool>();
        public static List<int> rocks = new List<int>() { 32, 34, 36, 38, 40, 42, 44, 48, 50, 52, 54, 56, 58, 668, 670, 760, 762 };
        public static List<int> treasures = new List<int>() { 751, 290, 764, 765, 2, 4, 6, 8, 10, 12, 14, 25, 46, 75, 76, 77, 95, 843, 844, 849, 850 };


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            SMonitor = Monitor;
            SHelper = Helper;

            tilesheet = Helper.Content.Load<Texture2D>("assets/tilesheet.png");

            helper.Events.Player.Warped += Player_Warped;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] {typeof(SpriteBatch),typeof(int),typeof(int),typeof(float) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_draw_Prefix))
            );

        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            flippedObjects.Clear();
        }

        private static bool Object_draw_Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!(Game1.currentLocation is MineShaft))
                return true;


            if (rocks.Contains(__instance.parentSheetIndex) || treasures.Contains(__instance.parentSheetIndex))
            {

                if (!flippedObjects.ContainsKey(new Vector2(x, y)))
                {
                    flippedObjects[new Vector2(x, y)] = Game1.random.NextDouble() < 0.5;
                }

                bool flip = flippedObjects[new Vector2(x, y)];

                Vector2 origin = new Vector2(8f, 8f);
                GetTileInfo(Game1.currentLocation.objects, x, y, __instance.parentSheetIndex, flip, out int tileIndex);
                if (tileIndex == -1)
                    return true;

                Rectangle sourceRect = new Rectangle(tileIndex * 16 % (sheetWidth* 16), tileIndex * 16 / (sheetWidth * 16) * 16, 16, 16);
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
                spriteBatch.Draw(tilesheet, position, sourceRect, Color.White, 0, origin, (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.isPassable() ? __instance.getBoundingBox(new Vector2(x, y)).Top : __instance.getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
                if (treasures.Contains(__instance.parentSheetIndex))
                {
                    Rectangle treasureRect = new Rectangle((treasures.IndexOf(__instance.parentSheetIndex) % sheetWidth) * 16, (10 + treasures.IndexOf(__instance.parentSheetIndex) / sheetWidth) * 16, 16, 16);
                    spriteBatch.Draw(tilesheet, position, treasureRect, Color.White, 0, origin, (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.isPassable() ? __instance.getBoundingBox(new Vector2(x, y)).Top : __instance.getBoundingBox(new Vector2(x, y)).Bottom) / 10000f + 0.001f);
                }

                return false;
            }
            return true;
        }
        private static void GetTileInfo(OverlaidDictionary objects, int x, int y, NetInt parentSheetIndex, bool flip, out int tileIndex)
        {
            int setOffset = GetGroupSetOffset(objects, x, y);
            if (IsRock(objects, x - 1, y)) // rock to left
            {
                if (IsRock(objects, x + 1, y)) // rock to right 
                {
                    if (IsRock(objects, x, y - 1)) // rock above
                    {
                        if (IsRock(objects, x, y + 1)) // rock below
                        {
                            // full
                            
                            tileIndex = 7;
                        }
                        else
                        {
                            // no bottom

                            tileIndex = 10;
                        }
                    }
                    else
                    {
                        if (IsRock(objects, x, y + 1))
                        {
                            // no top
                            tileIndex = 2;
                        }
                        else
                        {
                            // no top or bottom
                            tileIndex = 6;
                        }
                    }
                }
                else // no right
                {
                    if (IsRock(objects, x, y - 1)) // rock above
                    {
                        if (IsRock(objects, x, y + 1)) // rock below
                        {
                            // no right
                            tileIndex = flip ? 3 : 11;

                        }
                        else
                        {
                            // no right or bottom
                            tileIndex = flip ? 8 : 9;
                        }
                    }
                    else // no right or above
                    {
                        if (IsRock(objects, x, y + 1)) // rock below
                        {
                            // no right or above
                            tileIndex = flip ? 0 : 1;

                        }
                        else
                        {
                            // only left
                            tileIndex = flip ? 5 : 13;

                        }
                    }
                }
            }
            else // no left
            {
                if (IsRock(objects, x + 1, y)) // rock to right 
                {
                    if (IsRock(objects, x, y - 1)) // rock top
                    {
                        if (IsRock(objects, x, y + 1)) // rock bottom
                        {
                            // no left

                            tileIndex = flip ? 11 : 3;
                        }
                        else
                        {
                            // no left or bottom

                            tileIndex = flip ? 9 : 8;
                        }
                    }
                    else
                    {
                        if (IsRock(objects, x, y + 1))
                        {
                            // no left or top
                            tileIndex = flip ? 1 : 0;

                        }
                        else
                        {
                            // only right

                            tileIndex = flip ? 13 : 5;

                        }
                    }
                }
                else // no right
                {
                    if (IsRock(objects, x, y - 1)) // rock top
                    {
                        if (IsRock(objects, x, y + 1)) // rock bottom
                        {
                            // no right or left

                            tileIndex = 14;

                        }
                        else
                        {
                            // only top
                            tileIndex = 12;
                        }
                    }
                    else // no left, right or top
                    {
                        if (IsRock(objects, x, y + 1)) // rock bottom
                        {
                            // only bottom
                            tileIndex = 4;

                        }
                        else
                        {
                            // none

                            tileIndex = -1;
                            return;
                        }
                    }
                }
            }
            tileIndex += sheetWidth * setOffset * 2;
        }
        private static List<Vector2> check = new List<Vector2>();
        private static int GetGroupSetOffset(OverlaidDictionary objects, int x, int y)
        {
            Dictionary<int, List<Vector2>> offsets = new Dictionary<int, List<Vector2>>();
            GetSurroundingOffsets(objects, new List<Vector2>(), offsets, new Vector2(x, y));
            int offset = 0;
            int count = 0;

            int dCount = 0;
            string debug = "";
            if(!check.Contains(new Vector2(x, y)) && offsets.Count > 1)
            {
                check.Add(new Vector2(x, y));
                debug += $"multi: {x},{y}'s group has {offsets.Count} different sets";
            }
            
            foreach (var k in offsets.Keys)
            {
                
                dCount += offsets[k].Count;
                if(debug != "")
                {
                    debug += $"\noffset {k}: {string.Join(" | ", offsets[k])}";
                }
                
                if (offsets[k].Count > count || (offsets[k].Count == count && k > offset))
                {
                    count = offsets[k].Count;
                    offset = k;
                }
            }
            if(debug != "")
            {
                debug += $"\nfinal offset: {offset}, pieces: {dCount}";
                SMonitor.Log(debug);
            }

            return offset;
        }

        private static void GetSurroundingOffsets(OverlaidDictionary objects, List<Vector2> tiles, Dictionary<int, List<Vector2>> offsets, Vector2 tile)
        {
            if (!tiles.Contains(tile))
                tiles.Add(tile);
            int offset = GetSetOffset(objects[tile].parentSheetIndex);
            if (!offsets.ContainsKey(offset))
                offsets.Add(offset, new List<Vector2>() { tile });
            else
                offsets[offset].Add(tile);

            if(!tiles.Contains(tile + new Vector2(-1, 0)) && IsRock(objects, (int)tile.X - 1, (int)tile.Y))
                GetSurroundingOffsets(objects, tiles, offsets, tile + new Vector2(-1, 0));
            if(!tiles.Contains(tile + new Vector2(1, 0)) && IsRock(objects, (int)tile.X + 1, (int)tile.Y))
                GetSurroundingOffsets(objects, tiles, offsets, tile + new Vector2(1, 0));
            if(!tiles.Contains(tile + new Vector2(0, -1)) && IsRock(objects, (int)tile.X, (int)tile.Y - 1))
                GetSurroundingOffsets(objects, tiles, offsets, tile + new Vector2(0, -1));
            if(!tiles.Contains(tile + new Vector2(0, 1)) && IsRock(objects, (int)tile.X, (int)tile.Y + 1))
                GetSurroundingOffsets(objects, tiles, offsets, tile + new Vector2(0, 1));
        }

        private static int GetSetOffset(NetInt parentSheetIndex)
        {
            if (new int[] { 48, 50, 52, 54, 76, 290, 850 }.Contains(parentSheetIndex))
                return 1; // blue
            if (new int[] { 56, 58}.Contains(parentSheetIndex))
                return 2; // red
            if (new int[] { 10, 44, 46, 765, 843, 844, 849 }.Contains(parentSheetIndex))
                return 3; // purple
            if (new int[] { 2, 34, 36, 75, 668, 670,760, 762, 764 }.Contains(parentSheetIndex))
                return 4; // grey
            return 0;
        }

        private static bool IsRock(OverlaidDictionary objects, int x, int y)
        {
            return objects.ContainsKey(new Vector2(x, y)) && (rocks.Contains(objects[new Vector2(x, y)].parentSheetIndex) || treasures.Contains(objects[new Vector2(x, y)].parentSheetIndex));
        }
    }
}
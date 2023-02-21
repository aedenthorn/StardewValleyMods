using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using xTile.Dimensions;
using xTile.Tiles;
using xTile;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace DynamicFlooring
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.draw))]
        public class GameLocation_draw_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !drawingTiles.Value || startTile.Value is null || Game1.player.ActiveObject is not Wallpaper)
                    return;
                var start = startTile.Value.Value;
                Vector2 rectStart = new Vector2(MathHelper.Min(start.X, Game1.currentCursorTile.X), MathHelper.Min(start.Y, Game1.currentCursorTile.Y));
                Vector2 rectEnd = new Vector2(MathHelper.Max(start.X, Game1.currentCursorTile.X) + 1, MathHelper.Max(start.Y, Game1.currentCursorTile.Y) + 1);
                Rectangle tileRect = new Rectangle(Utility.Vector2ToPoint(rectStart), Utility.Vector2ToPoint(rectEnd - rectStart));
                Rectangle drawRect = new Rectangle(Utility.Vector2ToPoint(rectStart * 64), Utility.Vector2ToPoint((rectEnd - rectStart) * 64));
                if (!SHelper.Input.IsSuppressed(Config.PlaceButton))
                {
                    drawingTiles.Value = false;
                    Wallpaper w = (Game1.player.ActiveObject as Wallpaper);
                    string id;
                    if (w.GetModData() != null)
                    {
                        id = w.GetModData().ID + ":" + w.ParentSheetIndex.ToString();
                    }
                    else
                    {
                        id = w.ParentSheetIndex.ToString();
                    }
                    var list = new List<FlooringData>();
                    if (Game1.player.currentLocation.modData.TryGetValue(flooringKey, out string listString))
                    {
                        list = JsonConvert.DeserializeObject<List<FlooringData>>(listString);
                    }
                    if (list.Any())
                    {
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            if (list[i].area == tileRect)
                            {
                                list.RemoveAt(i);
                                UpdateFloor(Game1.currentLocation, list);
                            }
                        }
                    }
                    list.Add(new FlooringData() { area = tileRect, id = id, ignore = SHelper.Input.IsDown(Config.IgnoreButton) });
                    Game1.currentLocation.modData[flooringKey] = JsonConvert.SerializeObject(list);
                    UpdateFloor(Game1.currentLocation, list);
                    if(Config.Consume)
                        Game1.player.reduceActiveItemByOne();
                    startTile.Value = null;
                }
                else
                {
                    b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, drawRect), Color.Green * 0.5f);
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.resetForPlayerEntry))]
        public class GameLocation_resetForPlayerEntry_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(flooringKey, out string listString))
                    return;
                var list = JsonConvert.DeserializeObject<List<FlooringData>>(listString);
                UpdateFloor(__instance, list);
            }
        }
        [HarmonyPatch(typeof(DecoratableLocation), nameof(DecoratableLocation.UpdateFloor))]
        public class DecoratableLocation_UpdateFloor_Patch
        {
            public static void Postfix(DecoratableLocation __instance)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(flooringKey, out string listString))
                    return;
                var list = JsonConvert.DeserializeObject<List<FlooringData>>(listString);
                UpdateFloor(__instance, list);
            }
        }
        [HarmonyPatch(typeof(InteriorDoor), "closeDoorTiles")]
        public class closeDoorTiles_Patch
        {
            public static bool Prefix(InteriorDoor __instance)
            {
                return false;
                var tile = __instance.Tile;
                foreach(var s in __instance.Tile.TileSheet.Map.TileSheets)
                {
                    SMonitor.Log(s.Id);
                }
                Location doorLocation = new Location(__instance.Position.X, __instance.Position.Y);
                Map map = __instance.Location.Map;
                if (map == null)
                {
                    return false;
                }
                if (__instance.Tile == null)
                {
                    return false;
                }
                map.GetLayer("Buildings").Tiles[doorLocation] = __instance.Tile;
                __instance.Location.removeTileProperty(__instance.Position.X, __instance.Position.Y, "Back", "TemporaryBarrier");
                doorLocation.Y--;
                map.GetLayer("Front").Tiles[doorLocation] = new StaticTile(map.GetLayer("Front"), __instance.Tile.TileSheet, BlendMode.Alpha, __instance.Tile.TileIndex - __instance.Tile.TileSheet.SheetWidth);
                doorLocation.Y--;
                map.GetLayer("Front").Tiles[doorLocation] = new StaticTile(map.GetLayer("Front"), __instance.Tile.TileSheet, BlendMode.Alpha, __instance.Tile.TileIndex - __instance.Tile.TileSheet.SheetWidth * 2);
                return false;
            }   
        }
    }
}
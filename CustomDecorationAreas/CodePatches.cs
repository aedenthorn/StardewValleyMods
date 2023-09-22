using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomDecorationAreas
{
    internal class CodePatches
    {

        public static void getFloors_Postfix(DecoratableLocation __instance, ref List<Rectangle> __result)
        {
            if (!ModEntry.config.EnableMod || !ModEntry.floorsWallsDataDict.ContainsKey(__instance.Name))
                return;
            FloorWallData data = ModEntry.floorsWallsDataDict[__instance.Name];
            if (data.getFloorsFromFile?.Length > 0)
                data.floors = ModEntry.PHelper.GameContent.Load<List<Rectangle>>(data.getFloorsFromFile);
            if (data.replaceFloors)
                __result = data.floors;
            else
                __result.AddRange(data.floors);
        }

        public static void getWalls_Postfix(DecoratableLocation __instance, ref List<Rectangle> __result)
        {
            if (!ModEntry.config.EnableMod || !ModEntry.floorsWallsDataDict.ContainsKey(__instance.Name))
                return;
            FloorWallData data = ModEntry.floorsWallsDataDict[__instance.Name];
            if (data.getWallsFromFile?.Length > 0)
                data.walls = ModEntry.PHelper.GameContent.Load<List<Rectangle>>(data.getWallsFromFile);
            if (data.replaceWalls)
                __result = data.walls;
            else
                __result.AddRange(data.walls);
        }
        public static void loadForNewGame_Postfix()
        {
            if (!ModEntry.config.EnableMod)
                return;
            for(int i = Game1.locations.Count - 1; i>= 0; i--)
            {
                if (Game1.locations[i].GetType() == typeof(GameLocation) && ModEntry.floorsWallsDataDict.ContainsKey(Game1.locations[i].Name))
                {
                    GameLocation gl = Game1.locations[i];
                    ModEntry.PMonitor.Log($"Converting {gl.Name} to decoratable");
                    DecoratableLocation dl = new DecoratableLocation(gl.mapPath.Value, gl.Name);
                    if (dl.map.GetTileSheet("walls_and_floors") == null)
                    {
                        Texture2D tex = ModEntry.PHelper.Content.Load<Texture2D>($"Maps/walls_and_floors", ContentSource.GameContent);
                        dl.map.AddTileSheet(new TileSheet("walls_and_floors", dl.map, ModEntry.PHelper.Content.GetActualAssetKey($"Maps/walls_and_floors", ContentSource.GameContent), new Size(tex.Width / 16, tex.Height / 16), new Size(16, 16)));
                    }
                    Game1._locationLookup.Remove(gl.name);
                    Game1.locations.RemoveAt(i);
                    Game1.locations.Add(dl);
                    if (gl.characters.Count > 0)
                    {
                        for (int j = gl.characters.Count - 1; j >= 0; j--)
                        {
                            NPC npc = gl.characters[j];
                            NPC newNPC = new NPC(new AnimatedSprite(npc.sprite.Value.textureName.Value, npc.sprite.Value.currentFrame, npc.sprite.Value.SpriteWidth, npc.sprite.Value.SpriteHeight), ModEntry.PHelper.Reflection.GetField<NetVector2>(npc, "defaultPosition").GetValue().Value, Game1.locations[i].Name, ModEntry.PHelper.Reflection.GetField<int>(npc, "defaultFacingDirection").GetValue(), npc.Name, npc.datable.Value, null, Game1.content.Load<Texture2D>($"Portraits\\{npc.Name}"));
                            ModEntry.PMonitor.Log($"Adding {newNPC.Name}, sprite {newNPC.Sprite.textureName.Value}");
                            Game1.locations[Game1.locations.Count - 1].addCharacter(newNPC);
                        }
                    }
                    foreach (Vector2 key in gl.objects.Keys)
                        dl.objects.Add(key, gl.objects[key]);

                    ModEntry.convertedLocations.Add(dl.Name);
                }
            }
        }

        public static bool doSetVisibleFloor_Prefix(DecoratableLocation __instance, int whichRoom, int which)
        {
            if (!ModEntry.config.EnableMod || !ModEntry.floorsWallsDataDict.ContainsKey(__instance.Name))
                return true;

            int idx = -1;

            for (int i = 0; i < __instance.map.TileSheets.Count; i++)
            {
                if (__instance.map.TileSheets[i].Id == "walls_and_floors")
                {
                    idx = i;
                    break;
                }
            }
            if (idx == -1)
            {
                return true;
            }

            List<Rectangle> rooms = __instance.getFloors();
            int tileSheetIndex = 336 + which % 8 * 2 + which / 8 * 32;
            if (whichRoom == -1)
            {
                using (List<Rectangle>.Enumerator enumerator = rooms.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Rectangle r = enumerator.Current;
                        for (int x = r.X; x < r.Right; x += 2)
                        {
                            for (int y = r.Y; y < r.Bottom; y += 2)
                            {
                                if (r.Contains(x, y) && IsFloorableTile(x, y, "Back", __instance))
                                {
                                    SetFlooringTile(tileSheetIndex, x, y, r, __instance, idx);
                                }
                                if (r.Contains(x + 1, y) && IsFloorableTile(x + 1, y, "Back", __instance))
                                {
                                    SetFlooringTile(tileSheetIndex, x + 1, y, r, __instance, idx);
                                }
                                if (r.Contains(x, y + 1) && IsFloorableTile(x, y + 1, "Back", __instance))
                                {
                                    SetFlooringTile(tileSheetIndex, x, y + 1, r, __instance, idx);
                                }
                                if (r.Contains(x + 1, y + 1) && IsFloorableTile(x + 1, y + 1, "Back", __instance))
                                {
                                    SetFlooringTile(tileSheetIndex, x + 1, y + 1, r, __instance, idx);
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            if (rooms.Count > whichRoom)
            {
                Rectangle r2 = rooms[whichRoom];
                for (int x2 = r2.X; x2 < r2.Right; x2 += 2)
                {
                    for (int y2 = r2.Y; y2 < r2.Bottom; y2 += 2)
                    {
                        if (r2.Contains(x2, y2) && IsFloorableTile(x2, y2, "Back", __instance))
                        {
                            SetFlooringTile(tileSheetIndex, x2, y2, r2, __instance, idx);
                        }
                        if (r2.Contains(x2 + 1, y2) && IsFloorableTile(x2 + 1, y2, "Back", __instance))
                        {
                            SetFlooringTile(tileSheetIndex, x2 + 1, y2, r2, __instance, idx);
                        }
                        if (r2.Contains(x2, y2 + 1) && IsFloorableTile(x2, y2 + 1, "Back", __instance))
                        {
                            SetFlooringTile(tileSheetIndex, x2, y2 + 1, r2, __instance, idx);
                        }
                        if (r2.Contains(x2 + 1, y2 + 1) && IsFloorableTile(x2 + 1, y2 + 1, "Back", __instance))
                        {
                            SetFlooringTile(tileSheetIndex, x2 + 1, y2 + 1, r2, __instance, idx);
                        }
                    }
                }
            }
            return false;
        }
        public static bool doSetVisibleWallpaper_Prefix(DecoratableLocation __instance, int whichRoom, int which)
        {
            if (!ModEntry.config.EnableMod || !ModEntry.floorsWallsDataDict.ContainsKey(__instance.Name))
                return true;

            int idx = -1;

            for (int i = 0; i < __instance.map.TileSheets.Count; i++)
            {
                if (__instance.map.TileSheets[i].Id == "walls_and_floors")
                {
                    idx = i;
                    break;
                }
            }
            if (idx == -1)
            {
                return true;
            }

            __instance.updateMap();
            List<Rectangle> rooms = __instance.getWalls();
            int tileSheetIndex = which % 16 + which / 16 * 48;
            if (whichRoom == -1)
            {
                using (List<Rectangle>.Enumerator enumerator = rooms.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Rectangle r = enumerator.Current;
                        for (int x = r.X; x < r.Right; x++)
                        {
                            if (IsFloorableOrWallpaperableTile(x, r.Y, "Back", __instance))
                            {
                                SetWallpaperTile(x, r.Y, tileSheetIndex, "Back", __instance, idx);
                            }
                            if (IsFloorableOrWallpaperableTile(x, r.Y + 1, "Back", __instance))
                            {
                                SetWallpaperTile(x, r.Y + 1, tileSheetIndex + 16, "Back", __instance, idx);
                            }
                            if (r.Height >= 3)
                            {
                                if (IsFloorableOrWallpaperableTile(x, r.Y + 2, "Buildings", __instance))
                                {
                                    SetWallpaperTile(x, r.Y + 2, tileSheetIndex + 32, "Buildings", __instance, idx);
                                }
                                if (IsFloorableOrWallpaperableTile(x, r.Y + 2, "Back", __instance))
                                {
                                    SetWallpaperTile(x, r.Y + 2, tileSheetIndex + 32, "Back", __instance, idx);
                                }
                            }
                        }
                    }
                    return false;
                }
            }
            if (rooms.Count > whichRoom)
            {
                Rectangle r2 = rooms[whichRoom];
                for (int x2 = r2.X; x2 < r2.Right; x2++)
                {
                    if (IsFloorableOrWallpaperableTile(x2, r2.Y, "Back", __instance))
                    {
                        SetWallpaperTile(x2, r2.Y, tileSheetIndex, "Back", __instance, idx);
                    }
                    if (IsFloorableOrWallpaperableTile(x2, r2.Y + 1, "Back", __instance))
                    {
                        SetWallpaperTile(x2, r2.Y + 1, tileSheetIndex + 16, "Back", __instance, idx);
                    }
                    if (r2.Height >= 3)
                    {
                        if (IsFloorableOrWallpaperableTile(x2, r2.Y + 2, "Buildings", __instance))
                        {
                            SetWallpaperTile(x2, r2.Y + 2, tileSheetIndex + 32, "Buildings", __instance, idx);
                        }
                        else if (IsFloorableOrWallpaperableTile(x2, r2.Y + 2, "Back", __instance))
                        {
                            SetWallpaperTile(x2, r2.Y + 2, tileSheetIndex + 32, "Back", __instance, idx);
                        }
                    }
                }
            }
            return false;
        }

        public static void SetWallpaperTile(int tile_x, int tile_y, int tileIndex, string layerName,  DecoratableLocation location, int sheetIndex)
        {

            foreach(Rectangle omit in ModEntry.floorsWallsDataDict[location.name].floorsOmit)
            {
                if (omit.Contains(tile_x, tile_y))
                    return;
            }

            Layer layer = location.map.GetLayer(layerName);

            if(!ModEntry.floorsWallsDataDict[location.name].replaceNonDecorationTiles && layer.Tiles[tile_x, tile_y].TileSheet.Id != "walls_and_floors")
                return;
            Dictionary<string,xTile.ObjectModel.PropertyValue> props = new Dictionary<string, xTile.ObjectModel.PropertyValue>(location.map.GetLayer(layerName).Tiles[tile_x, tile_y].Properties);
            location.setMapTile(tile_x, tile_y, tileIndex, layerName, null, sheetIndex);
            foreach(var kvp in props)
            {
                location.map.GetLayer(layerName).Tiles[tile_x, tile_y].Properties.Add(kvp.Key, kvp.Value);
            }
        }
        public static void SetFlooringTile(int base_tile_sheet, int tile_x, int tile_y, Rectangle r, DecoratableLocation location, int sheetIndex)
        {

            foreach(Rectangle omit in ModEntry.floorsWallsDataDict[location.name].floorsOmit)
            {
                if (omit.Contains(tile_x, tile_y))
                    return;
            }

            Layer layer = location.map.GetLayer("Back");

            int replaced_tile_index;
            if(layer.Tiles[tile_x, tile_y].TileSheet.Id == "walls_and_floors")
            {
                replaced_tile_index = location.getTileIndexAt(tile_x, tile_y, "Back");
            }
            else
            {
                if (!ModEntry.floorsWallsDataDict[location.name].replaceNonDecorationTiles)
                    return;
                replaced_tile_index = base_tile_sheet + (tile_x - r.X) % 2 + (tile_y - r.Y) % 2 * 16;
            }
            if (replaced_tile_index < 336)
            {
                return;
            }
            replaced_tile_index -= 336;

            int x_offset = replaced_tile_index % 2;
            int y_offset = replaced_tile_index % 32 / 16;

            Dictionary<string, xTile.ObjectModel.PropertyValue> props = new Dictionary<string, xTile.ObjectModel.PropertyValue>(location.map.GetLayer("Back").Tiles[tile_x, tile_y].Properties);
            location.setMapTile(tile_x, tile_y, base_tile_sheet + x_offset + 16 * y_offset, "Back", null, sheetIndex);
            foreach (var kvp in props)
            {
                location.map.GetLayer("Back").Tiles[tile_x, tile_y].Properties.Add(kvp.Key, kvp.Value);
            }

        }

        public static bool IsFloorableTile(int x, int y, string layer, DecoratableLocation location)
        {
            int tile_index = location.getTileIndexAt(x, y, "Buildings");
            return (tile_index < 197 || tile_index > 199 || !(location.getTileSheetIDAt(x, y, "Buildings") == "untitled tile sheet")) && IsFloorableOrWallpaperableTile(x, y, layer, location);
        }

        public static bool IsFloorableOrWallpaperableTile_Prefix(DecoratableLocation __instance, ref bool __result, int x, int y, string layer_name)
        {
            if (!ModEntry.config.EnableMod || !ModEntry.floorsWallsDataDict.ContainsKey(__instance.Name))
                return true;
            __result = IsFloorableOrWallpaperableTile(x, y, layer_name, __instance);
            return false;

        }
        public static bool IsFloorableOrWallpaperableTile(int x, int y, string layer_name, DecoratableLocation location)
        {
            Layer layer = location.map.GetLayer(layer_name);

            if (layer == null || x >= layer.LayerWidth || y >= layer.LayerHeight || layer.Tiles[x, y] == null || layer.Tiles[x, y].TileSheet == null)
                return false;

            return true;
        }
    }
}
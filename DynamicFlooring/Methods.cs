using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace DynamicFlooring
{
    public partial class ModEntry
    {
        public static int firstFlooringTile = 336;
        private static void UpdateFloor(GameLocation location, List<FlooringData> list)
        {
            if (!Config.ModEnabled || location.map is null)
                return;
            foreach (var data in list)
            {
                for(int y = data.area.Y; y < data.area.Y + data.area.Height; y++)
                {
                    for(int x = data.area.X; x < data.area.X + data.area.Width; x++)
                    {
                        if (!location.isTileOnMap(new Vector2(x, y)))
                            continue;
                        try
                        {
                            KeyValuePair<int, int> source = GetFloorSource(location, data.id);
                            if (source.Value >= 0)
                            {
                                int tilesheet_index = source.Key;
                                int floor_pattern_id = source.Value;
                                int tiles_wide = location.map.TileSheets[tilesheet_index].SheetWidth;
                                string id = location.map.TileSheets[tilesheet_index].Id;
                                string layer = "Back";
                                floor_pattern_id = floor_pattern_id * 2 + floor_pattern_id / (tiles_wide / 2) * tiles_wide;
                                if (id == "walls_and_floors")
                                {
                                    floor_pattern_id += firstFlooringTile;
                                }
                                var floor = IsFloorableTile(location, x, y, layer) && IsFloorableOrWallpaperableTile(location, x, y, layer);
                                var wall = !floor && IsFloorableOrWallpaperableTile(location, x, y, layer);
                                var wallOrFloor = floor || wall;
                                if ((data.ignore && !wall) || (!data.ignore && floor))
                                {
                                    Tile old_tile = location.map.GetLayer(layer).Tiles[x, y];
                                    location.setMapTile(x, y, GetFlooringIndex(location, floor_pattern_id, x, y, data, tilesheet_index), layer, null, tilesheet_index);
                                    Tile new_tile = location.map.GetLayer(layer).Tiles[x, y];
                                    if (old_tile != null)
                                    {
                                        foreach (KeyValuePair<string, PropertyValue> property in old_tile.Properties)
                                        {
                                            new_tile.Properties[property.Key] = property.Value;
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        public static KeyValuePair<int, int> GetFloorSource(GameLocation location, string pattern_id)
        {
            int pattern_index = -1;
            if (pattern_id.Contains(":"))
            {
                string[] pattern_split = pattern_id.Split(':', StringSplitOptions.None);
                TileSheet tilesheet = GetWallAndFloorTilesheet(location, pattern_split[0]);
                if (int.TryParse(pattern_split[1], out pattern_index) && tilesheet != null)
                {
                    return new KeyValuePair<int, int>(location.map.TileSheets.IndexOf(tilesheet), pattern_index);
                }
            }
            else if (int.TryParse(pattern_id, out pattern_index))
            {
                TileSheet tilesheet2 = GetWallAndFloorTilesheet(location, "walls_and_floors");
                return new KeyValuePair<int, int>(location.map.TileSheets.IndexOf(tilesheet2), pattern_index);
            }
            return new KeyValuePair<int, int>(-1, -1);
        }
        public static TileSheet GetWallAndFloorTilesheet(GameLocation location, string id)
        {
            TileSheet result = null;
            if (id == "walls_and_floors")
            {
                result = location.map.GetTileSheet(id);
                if(result is null)
                {
                    var texture = SHelper.GameContent.Load<Texture2D>("Maps/walls_and_floors");
                    TileSheet tilesheet = new TileSheet("walls_and_floors", location.map, "Maps/walls_and_floors", new Size(texture.Width / 16, texture.Height / 16), new Size(16, 16));
                    location.map.AddTileSheet(tilesheet);
                    location.map.LoadTileSheets(Game1.mapDisplayDevice);
                    location.interiorDoors.ResetLocalState();
                    result = tilesheet;
                }
                return result;
            }
            result = location.map.GetTileSheet("x_WallsAndFloors_" + id);
            if (result != null)
                return result;
            try
            {
                List<ModWallpaperOrFlooring> list = Game1.content.Load<List<ModWallpaperOrFlooring>>("Data\\AdditionalWallpaperFlooring");
                ModWallpaperOrFlooring found_mod_data = null;
                foreach (ModWallpaperOrFlooring mod_data_entry in list)
                {
                    if (mod_data_entry.ID == id)
                    {
                        found_mod_data = mod_data_entry;
                        break;
                    }
                }
                if (found_mod_data != null)
                {
                    Texture2D texture = Game1.content.Load<Texture2D>(found_mod_data.Texture);
                    if (texture.Width / 16 != 16)
                    {
                        Console.WriteLine("WARNING: Wallpaper/floor tilesheets must be 16 tiles wide.");
                    }
                    TileSheet tilesheet = new TileSheet("x_WallsAndFloors_" + id, location.map, found_mod_data.Texture, new Size(texture.Width / 16, texture.Height / 16), new Size(16, 16));
                    location.map.AddTileSheet(tilesheet);
                    location.map.LoadTileSheets(Game1.mapDisplayDevice);
                    location.interiorDoors.ResetLocalState();
                    result = tilesheet;
                }
                else
                {
                    Console.WriteLine("Error trying to load wallpaper/floor tilesheet: " + id);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error trying to load wallpaper/floor tilesheet: " + id);
            }
            return result;
        }
        public static bool IsFloorableTile(GameLocation location, int x, int y, string layer_name)
        {
            int tile_index = location.getTileIndexAt(x, y, "Buildings");
            return (tile_index < 197 || tile_index > 199 || !(location.getTileSheetIDAt(x, y, "Buildings") == "untitled tile sheet"));
        }
        public static bool IsFloorableOrWallpaperableTile(GameLocation location, int x, int y, string layer_name)
        {
            Layer layer = location.map.GetLayer(layer_name);
            return layer != null && x < layer.LayerWidth && y < layer.LayerHeight && layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet != null && IsWallAndFloorTilesheet(layer.Tiles[x, y].TileSheet.Id);
        }
        public static bool IsWallAndFloorTilesheet(string tilesheet_id)
        {
            return tilesheet_id.StartsWith("x_WallsAndFloors_") || tilesheet_id == "walls_and_floors";
        }
        public static int GetFlooringIndex(GameLocation location, int base_tile_sheet, int tile_x, int tile_y, FlooringData data, int tileSheetIndex)
        {
            string tilesheet_name = location.getTileSheetIDAt(tile_x, tile_y, "Back");
            TileSheet tilesheet = data.ignore ? location.map.TileSheets[tileSheetIndex] : location.map.GetTileSheet(tilesheet_name);
            int tiles_wide = 16;
            if (tilesheet != null)
            {
                tiles_wide = tilesheet.SheetWidth;
            }
            if (data.ignore)
            {
                int x_offset = (tile_x - data.area.X) % 2;
                int y_offset = (tile_y - data.area.Y) % 2;
                return base_tile_sheet + x_offset + tiles_wide * y_offset;
            }
            else
            {
                int replaced_tile_index = location.getTileIndexAt(tile_x, tile_y, "Back");
                if (replaced_tile_index < 0)
                {
                    return 0;
                }
                if (tilesheet_name == "walls_and_floors")
                {
                    replaced_tile_index -= firstFlooringTile;
                }
                int x_offset = replaced_tile_index % 2;
                int y_offset = replaced_tile_index % (tiles_wide * 2) / tiles_wide;
                return base_tile_sheet + x_offset + tiles_wide * y_offset;
            }
        }
    }
}
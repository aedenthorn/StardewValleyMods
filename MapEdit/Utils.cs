using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Xml.Linq;
using xTile.Layers;
using xTile.Tiles;
using xTile;
using System.Linq;

namespace MapEdit
{
    public partial class ModEntry
    {
        public static bool MouseInMenu()
        {
            if (!modActive.Value)
                return false;
            if(tileMenu.Value is null)
            {
                tileMenu.Value = new();
            }
            if (Config.ShowMenu)
            {
                return Game1.getMouseX(true) < tileMenu.Value.width - IClickableMenu.spaceToClearSideBorder || TileSelectMenu.button.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true));
            }
            else
            {
                return TileSelectMenu.button.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true));
            }
        }

        private static Map EditMap(string mapPath, Map map, MapData data)
        {
            SMonitor.Log("Editing map " + mapPath);
            foreach (var kvp in data.customSheets)
            {
                if (map.TileSheets.FirstOrDefault(s => s.ImageSource == kvp.Value.path) != null)
                    continue;
                string name = kvp.Key;
                int which = 0;
                while (map.Layers.FirstOrDefault(l => l.Id == name) != null)
                {
                    which++;
                }
                if (which > 0)
                {
                    name += "_" + which;
                }
                map.AddTileSheet(new TileSheet(name, map, kvp.Value.path, new xTile.Dimensions.Size(kvp.Value.width, kvp.Value.height), new xTile.Dimensions.Size(16, 16)));
            }
            int count = 0;
            foreach (var kvp in data.tileDataDict)
            {
                foreach (Layer layer in map.Layers)
                {
                    if (layer.Id == "Paths")
                        continue;
                    try
                    {
                        layer.Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = null;
                    }
                    catch
                    {

                    }
                }
                foreach (var kvp2 in kvp.Value.tileDict)
                {
                    try
                    {
                        List<StaticTile> tiles = new List<StaticTile>();
                        for (int i = 0; i < kvp2.Value.tiles.Count; i++)
                        {
                            TileInfo tile = kvp2.Value.tiles[i];
                            tiles.Add(new StaticTile(map.GetLayer(kvp2.Key), map.GetTileSheet(tile.tileSheet), tile.blendMode, tile.tileIndex));
                            foreach (var prop in kvp2.Value.tiles[i].properties)
                            {
                                tiles[i].Properties[prop.Key] = prop.Value;
                            }
                        }

                        if (kvp2.Value.tiles.Count == 1)
                        {
                            map.GetLayer(kvp2.Key).Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = tiles[0];
                        }
                        else
                        {
                            map.GetLayer(kvp2.Key).Tiles[(int)kvp.Key.X, (int)kvp.Key.Y] = new AnimatedTile(map.GetLayer(kvp2.Key), tiles.ToArray(), kvp2.Value.frameInterval);
                        }
                        count++;
                    }
                    catch
                    {

                    }
                }
            }
            SMonitor.Log($"Added {count} custom tiles to map {mapPath}");
            cleanMaps.Add(mapPath);
            return map;
        }
    }
}
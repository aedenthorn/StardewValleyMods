using DMT.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace DMT
{
    public static partial class Utils
    {
        private static bool Enabled => context.Config.Enabled;

        public static bool CheckTrigger(DynamicTileProperty property, IEnumerable<string> triggers)
        {
            if (property.Trigger == null)
                return false;
            foreach (var trigger in triggers)
            {
                if (!trigger.Equals(property.Trigger, StringComparison.OrdinalIgnoreCase) && !trigger.StartsWith(property.Trigger + "(", StringComparison.OrdinalIgnoreCase))
                    continue;
                return true;
            }
            return false;
        }

        public static Point GetNextTile(int dir)
        {
            return dir switch
            {
                0 => new(0, -1),
                1 => new(1, 0),
                2 => new(0, 1),
                3 => new(-1, 0),
                _ => new(0) //This will never be hit, but I don't like my IDE complaining with colorfull squiqly lines
            };
        }

        public static bool PushTiles(GameLocation l, List<(Point p, Tile t)> tiles, int dir, Farmer f)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Point destination = tiles[i].p + GetNextTile(dir);
                if (!l.isTileOnMap(tiles[i].p) || !l.isTileOnMap(destination) || 
                    (
                        tiles[i].t.Layer.Tiles[destination.X, destination.Y] is Tile t &&
                        (t.HasProperty(Actions.PushKey, out _) || t.HasProperty(Actions.PushableKey, out _)) &&
                        !tiles.Any(x => x.p == destination && t.Layer.Id == tiles[i].t.Layer.Id)
                    ))
                    return false;
            }
            if (!context.PushTileDict.TryGetValue(l, out var pushed))
                context.PushTileDict[l] = pushed = [];
            for (int i = 0; i < tiles.Count; i++)
            {
                Point destination = tiles[i].p + GetNextTile(dir);
                var (p, t) = tiles[i];
                if (t.Layer.Id == "Buildings")
                    TriggerActions([t.Layer], f, l, p, ["Push"]);
                pushed.Add(new() { Tile = t, Position = new(p.X * 64, p.Y * 64), Origin = p, Direction = dir, Farmer = f, Destination = destination });
                tiles[i].t.Layer.Tiles[p.X, p.Y] = null;
            }
            return true;
        }

        public static void PushTilesWithOthers(Farmer f, Tile t, Point start)
        {
            if (f == null)
                return;

            List<(Point, Tile)> tiles = [(start, t)];
            if (!t.HasProperty(Actions.PushAlsoKey, out var others))
            {
                _ = PushTiles(f.currentLocation, tiles, f.FacingDirection, f);
                return;
            }
            var split = others.Split(',');
            foreach (var tile in split)
            {
                try
                {
                    var other = tile.Split(' ');
                    if (split.Length != 3 || !int.TryParse(other[1], out int x) || !int.TryParse(other[2], out int y))
                        continue;
                    x += start.X;
                    y += start.Y;
                    var layer = f.currentLocation.Map.GetLayer(other[0]);
                    if (layer is null || !f.currentLocation.isTileOnMap(x, y) || layer.Tiles[x, y] is not Tile otherTile)
                        continue;
                    tiles.Add((new(x, y), otherTile));
                }
                catch { }
            }
            PushTiles(f.currentLocation, tiles, f.FacingDirection, f);
        }

        internal static Layer AddLayer(Map map, string id)
        {
            var layer = new Layer(id, map, map.Layers[0].LayerSize, Layer.m_tileSize);
            map.AddLayer(layer);
            return layer;
        }

        internal static TileSheet AddTileSheet(Map map, string id, string texturePath)
        {
            var texture = context.Helper.GameContent.Load<Texture2D>(texturePath);
            if (texture == null)
                return null;
            var tilesheet = new TileSheet(id, map, texturePath, new xSize(texture.Width / 16, texture.Height / 16), new xSize(16, 16));
            map.AddTileSheet(tilesheet);
            return tilesheet;
        }

        internal static Item? ParseItemString(string value)
        {
            Item item = null;
            string[] split = [];

            try
            {
                item = ItemRegistry.Create(value, 1, 0, true) ?? throw new();
            }
            catch
            {
                int amount = 1;
                int quality = 0;
                if (value.Contains(','))
                {
                    split = value.Split(',');
                    value = split[0];
                    amount = int.Parse(split[1]);
                    if (split.Length == 3)
                        quality = int.Parse(split[2]);
                }
                item = ItemRegistry.Create(value, amount, quality);
            }

            return item;
        }

        internal static DynamicTileProperty? ParseProperty(KeyValuePair<string, string> kvp, IOrderedEnumerable<string> keys)
        {
            if (!kvp.Key.StartsWith(ModPrefix))
                return null;
            DynamicTileProperty prop = new() { Value = kvp.Value };
            string[] preKey = kvp.Key.Split('(', 2);
            string[] key;
            if (preKey.Length == 2)
            {
                key = preKey[0].Split('_');
                key[key.Length - 1] += "(" + preKey[1];
            }
            else
            {
                key = kvp.Key.Split('_');
            }
            if (key.Length < 2)
                return null;
            foreach (var item in keys)
            {
                if (!key[0].StartsWith(item, StringComparison.OrdinalIgnoreCase))
                    continue; 
                prop.Key = item;
                break;
            }
            if(key.Length > 2)
            {
                for (int i = 1; i < key.Length - 1; i++) // first is the action, last is the trigger
                {
                    if (key[i].Equals("Once", StringComparison.OrdinalIgnoreCase))
                    {
                        prop.Once = true;
                    }
                    else if (key[i].Equals("Swap", StringComparison.OrdinalIgnoreCase))
                    {
                        prop.Swap = true;
                    }
                    else if (key[i].StartsWith("Invalidate", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach(var e in Enum.GetValues(typeof(Invalidate)))
                        {
                            if (key[i].StartsWith((string)e))
                            {
                                prop.Invalidate = (string)e;
                                break;
                            }
                        }
                    }
                }
            }
            var trigger = key[^1];
            if (string.IsNullOrWhiteSpace(trigger))
                return null;
            foreach (var item in Triggers.Regexes)
            {
                if (Regex.IsMatch(trigger, item))
                {
                    prop.Trigger = trigger;
                    break;
                }
            }
            return prop;
        }

        internal static List<DynamicTileProperty> FilterProperties(List<DynamicTileProperty> properties)
        {
            List<string> keys = [];
            List<DynamicTileProperty> filtered = [];
            foreach (var prop in properties)
            {
                if (keys.Contains(prop.Key))
                {
                    context.Monitor.Log($"Found duplicate key {prop.Key}{(string.IsNullOrWhiteSpace(prop.LogName) ? "" : $" For named property {prop.LogName}")}, This property was automatically filtered out. Please avoid adding duplicate actions for tiles", LogLevel.Warn);
                    continue;
                }
                keys.Add(prop.Key);
                filtered.Add(prop);
            }

            return filtered;
        }

        internal static string BuildFormattedTrigger(params object?[]? parts)
        {
            if (!(parts?.Any() ?? false) || (parts?.FirstOrDefault() is null && parts.Length == 1))
                return "";

            StringBuilder sb = new();
            sb.Append('(');
            foreach (var part in parts)
                sb.Append(part);
            sb.Append(')');
            return sb.ToString();
        }

        internal static bool IsGlobalTrigger(string trigger)
        {
            foreach (var item in Triggers.GlobalTriggers)
                if (Regex.IsMatch(trigger, item))
                    return true;

            return false;
        }

        public static bool TriggerActions(List<Layer> layers, Farmer? who, GameLocation location, Point tilePosition, string[] triggers)
        {
            if (!Enabled || !location.isTileOnMap(tilePosition) || (!context.Config.TriggerDuringEvents && Game1.eventUp))
                return false;

            List<(DynamicTileProperty prop, Tile tile)> properties = GetPropertiesForTriggers(layers, tilePosition, triggers);
            if (properties.Count == 0)
            {
                return false;
            }
            return DoTriggerActions(who, location, tilePosition, properties);
        }

        internal static List<(DynamicTileProperty prop, Tile tile)> GetPropertiesForTriggers(List<Layer> layers, Point tilePosition, IEnumerable<string> triggers)
        {
            List<string> localTriggers = [];
            List<string> globalTriggers = [];
            List<(DynamicTileProperty prop, Tile tile)> properties = [];
            IOrderedEnumerable<string> allKeys = (IOrderedEnumerable<string>)Actions.AllActions.Union(Actions.ModKeys).OrderByDescending(x => x.Length);
            foreach (var trigger in (string[])[.. triggers])
            {
                if (IsGlobalTrigger(trigger))
                    globalTriggers.Add(trigger);
                else
                    localTriggers.Add(trigger);
            }

            if (globalTriggers.Count > 0)
                properties.AddRange(GetPropertiesForGlobalTriggers(layers, globalTriggers, allKeys));
            if (localTriggers.Count > 0)
                properties.AddRange(GetPropertiesForLocalTriggers(layers, tilePosition, localTriggers, allKeys));

            return properties;
        }

        internal static List<(DynamicTileProperty prop, Tile tile)> GetPropertiesForLocalTriggers(List<Layer> layers, Point tilePosition, IEnumerable<string> triggers, IOrderedEnumerable<string> allKeys)
        {
            List<(DynamicTileProperty prop, Tile tile)> properties = [];
            foreach (var layer in layers)
            {
                var tile = layer.Tiles[tilePosition.X, tilePosition.Y];

                if (tile is null)
                    continue;
                string[] props = [.. tile.Properties.Keys];

                foreach (var key in props)
                {
                    DynamicTileProperty? prop = ParseProperty(new(key, tile.Properties[key]), allKeys);

                    if (prop is null || !CheckTrigger(prop, triggers))
                        continue;

                    if (prop.Once)
                    {
                        tile.Properties.Remove(key);
                    }
                    else if (prop.Swap && tile.Properties.TryGetValue("Swap" + key, out var swap))
                    {
                        var old = tile.Properties[key];
                        tile.Properties[key] = swap;
                        tile.Properties["Swap" + key] = old;
                    }

                    properties.Add(new(prop, tile));
                }
            }
            return properties;
        }

        internal static List<(DynamicTileProperty prop, Tile tile)> GetPropertiesForGlobalTriggers(List<Layer> layers, IEnumerable<string> triggers, IOrderedEnumerable<string> allKeys)
        {
            List<(DynamicTileProperty prop, Tile tile)> properties = [];
            int width = layers[0].Tiles.Array.GetLength(0);
            int height = layers[0].Tiles.Array.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    foreach (var layer in layers)
                    {
                        var tile = layer.Tiles[x, y];

                        if (tile is null)
                            continue;
                        foreach (var key in tile.Properties.Keys)
                        {
                            DynamicTileProperty? prop = ParseProperty(new(key, tile.Properties[key]), allKeys);

                            if (prop is null || !CheckTrigger(prop, triggers))
                                continue;

                            if (prop.Once)
                                tile.Properties.Remove(key);

                            properties.Add(new(prop, tile));
                        }
                    }
                }
            }
            return properties;
        }

        /// <summary>
        /// Load the tile properties to a location from the dictionary asset
        /// </summary>
        /// <param name="l">The location to load properties to</param>
        internal static void LoadLocation(GameLocation l)
        {
            if (!context.Config.Enabled)
                return;
            Stopwatch s = new();
            s.Start();
            var dict = context.Helper.GameContent.Load<Dictionary<string, DynamicTile>>(TileDataDictPath);
            var keys = dict.Keys.ToList();
            for(int i = keys.Count - 1; i >= 0 ; i--)
            {
                if (!IsValidLocation(dict[keys[i]], l))
                {
                    keys.RemoveAt(i);
                }
            }
            IOrderedEnumerable<string> allKeys = (IOrderedEnumerable<string>)Actions.AllActions.Union(Actions.ModKeys).OrderByDescending(x => x.Length);

            foreach (var layer in l.Map.Layers)
            {

                int width = layer.Tiles.Array.GetLength(0);
                int height = layer.Tiles.Array.GetLength(1);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var tile = layer.Tiles[x, y];
                        if (tile is null)
                            continue;
                        foreach(var key in tile.Properties.Keys)
                        {
                            var prop = ParseProperty(new(key, tile.Properties[key]), allKeys);
                            if (prop?.Trigger == Triggers.Load)
                            {
                                DoTriggerActions(Game1.player, l, new(x, y), [(prop, tile)]);
                            }
                        }
                        foreach (var key in keys)
                        {
                            if (dict[key].Layers?.Contains(layer.Id) == false)
                                continue;
                            int count = 0;
                            int count2 = 0;
                            var value = dict[key];

                            Point tilePoint = new(x, y);
                            if (!IsValidTile(value, tile, tilePoint))
                                continue;
                            count++;
                            foreach (var prop in value.Properties)
                            {
                                layer.Tiles[x, y].Properties[prop.Key] = prop.Value;
                            }
                            foreach (var action in value.Actions)
                            {
                                if (action.trigger == Triggers.Load)
                                {
                                    DoTriggerActions(Game1.player, l, new(x, y), [(action, tile)]);
                                }
                                else
                                {
                                    ++count2;
                                    layer.Tiles[x, y].Properties[CreatePropertyKey(action)] = action.Value;
                                }
                            }
                        }
                    }
                }
            }
            context.Monitor.Log($"Load location {l.DisplayName} took {s.ElapsedMilliseconds}ms");
        }

        internal static bool HasProperty(this Tile t, string key, [NotNullWhen(true)] out string? propertyValue)
        {
            propertyValue = null;
            if (t is null || t.Properties is null || t.Properties.Count == 0)
                return false;
            if (t.Properties.TryGetValue(key, out var prop))
            {
                propertyValue = prop;
                return true;
            }

            return false;
        }
        internal static string CreatePropertyKey(DynamicTileProperty action)
        {
            return $"{action.Key}{(action.Once ? "_Once" : "")}{(action.Trigger != null ? "_" + action.Trigger : "")}";
        }

        internal static bool IsValidLocation(DynamicTile tileData, GameLocation l)
        {
            var tileSheets = l.Map.TileSheets.ToList();
            if (!(tileData.Properties?.Any() ?? false) && !(tileData.Actions?.Any() ?? false))
                return false;

            if (tileData.Locations is not null && 
                tileData.Locations.Count > 0 && 
                !tileData.Locations.Contains(l.Name))
                return false;

            if (tileData.TileSheets is not null && 
                tileData.TileSheets.Count > 0 && 
                !tileData.TileSheets.Exists(x => tileSheets.Exists(y => y.Id == x)))
                return false;

            if (tileData.TileSheetsPaths is not null && 
                tileData.TileSheetsPaths.Count > 0 && 
                !tileData.TileSheetsPaths.Exists(x => tileSheets.Exists(y => y.ImageSource.Contains(x))))
                return false;

            return true;
        }

        internal static bool IsValidTile(DynamicTile tileData, Tile tile, Point tilePoint)
        {
            if (tileData.TileSheets is not null && tileData.TileSheets.Count > 0 && !tileData.TileSheets.Contains(tile.TileSheet.Id))
                return false;

            if (tileData.TileSheetsPaths is not null && tileData.TileSheetsPaths.Count > 0 && !tileData.TileSheetsPaths.Exists(tile.TileSheet.ImageSource.Contains))
                return false;

            if (tileData.Indexes is not null && tileData.Indexes.Count > 0 && !tileData.Indexes.Contains(tile.TileIndex))
                return false;

            if (tileData.Rectangles is not null && tileData.Rectangles.Count > 0 && !tileData.Rectangles.Exists(x => x.Contains(tilePoint)))
                return false;

            if (tileData.Tiles is not null && tileData.Tiles.Count > 0 && !tileData.Tiles.Contains(new(tilePoint.X, tilePoint.Y)))
                return false;

            return true;
        }


        internal static bool TryGetParameter(string value, ParameterInfo pi, out object p)
        {

            if (int.TryParse(value, out var i))
            {
                if (pi.ParameterType == typeof(int))
                {
                    p = i;
                    return true;
                }
            }
            else if (bool.TryParse(value, out var b))
            {
                if (pi.ParameterType == typeof(bool))
                {
                    p = b;
                    return true;
                }
            }
            else if (value.StartsWith('#') && value.Length == 7)
            {
                if (pi.ParameterType == typeof(Color))
                {
                    Color color = new(
                        int.Parse(value.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                        int.Parse(value.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                        int.Parse(value.Substring(5, 2), System.Globalization.NumberStyles.HexNumber));
                    p = color;
                    return true;
                }
            }
            else if (pi.ParameterType == typeof(string))
            {
                p = value;
                return true;
            }
            p = "";
            return false;
        }
    }
}

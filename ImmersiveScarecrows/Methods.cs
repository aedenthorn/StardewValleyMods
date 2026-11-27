
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace ImmersiveScarecrows
{
    public partial class ModEntry
    {
        public static Object GetScarecrow(TerrainFeature tf, int which)
        {
            if (tf is not HoeDirt)
                return null;
            Object obj;
            if (tf.modData.TryGetValue(scarecrowKey + which, out string scarecrowString))
            {
                obj = (Object)ItemRegistry.Create(scarecrowString, 1, 0, true);

            }
            else
            {
                return null;
            }
                
            if(obj == null && scarecrowString != null)
            {
                foreach (var kvp in Game1.bigCraftableData)
                {
                    if (kvp.Key.Equals(scarecrowString))
                    {
                        obj = new Object(Vector2.Zero, kvp.Key);
                        break;
                    }
                }
            }
            if (obj == null && scarecrowString != null)
            {
                scarecrowString = scarecrowString.Split('/')[0];
                foreach (var kvp in Game1.bigCraftableData)
                {
                    if (kvp.Value.Name == scarecrowString)
                    {
                        obj = new Object(Vector2.Zero, kvp.Key);
                        break;
                    }
                }
            }
            if (obj is null)
                return null;

            if (atApi is not null)
            {
                foreach (var kvp2 in tf.modData.Pairs)
                {
                    if (kvp2.Key.EndsWith(which + "") && kvp2.Key.StartsWith(altTexturePrefix))
                    {
                        var key = kvp2.Key.Substring(prefixKey.Length, kvp2.Key.Length - prefixKey.Length - 1);
                        obj.modData[key] = kvp2.Value;
                    }
                }
            }
            if (!tf.modData.TryGetValue(guidKey + which, out var guid))
            {
                guid = Guid.NewGuid().ToString();
                tf.modData[guidKey + which] = guid;
            }
            scarecrowDict[guid] = obj;

            return obj;
        }
        public static string GetScarecrowString(Object instance)
        {
            return instance.QualifiedItemId;
        }
        public static Vector2 GetScarecrowCorner(int i)
        {
            switch (i)
            {
                case 0:
                    return new Vector2(-1, -1);
                case 1:
                    return new Vector2(1, -1);
                case 2:
                    return new Vector2(-1, 1);
                default:
                    return new Vector2(1, 1);
            }
        }

        public static int GetMouseCorner()
        {
            var x = Game1.getMouseX() + Game1.viewport.X;
            var y = Game1.getMouseY() + Game1.viewport.Y;
            if (x % 64 < 32)
            {
                if (y % 64 < 32)
                {
                    return 0;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (y % 64 < 32)
                {
                    return 1;
                }
                else
                {
                    return 3;
                }
            }
        }

        public static Object GetScarecrowAtMouse()
        {
            if (Game1.currentLocation == null)
                return null;

            var tile = Game1.currentCursorTile;
            int corner = GetMouseCorner();

            if (!GetScarecrowTileBool(Game1.currentLocation, ref tile, ref corner))
            return null;

            Game1.currentLocation.terrainFeatures.TryGetValue(tile, out var tf);
            return GetScarecrow(tf, corner);
        }

        public static bool GetScarecrowTileBool(GameLocation location, ref Vector2 tile, ref int which)
        {
            if (TileHasScarecrow(location, tile, which))
            { 
                return true; 
            }
            else
            {
                Dictionary<int, Vector2> dict = new();
                switch (which)
                {
                    case 0:
                        dict.Add(3, new Vector2(-1, -1));
                        dict.Add(2, new Vector2(0, -1));
                        dict.Add(1, new Vector2(-1, 0));
                        break;
                    case 1:
                        dict.Add(3, new Vector2(0, -1));
                        dict.Add(2, new Vector2(1, 1));
                        dict.Add(0, new Vector2(1, 0));
                        break;
                    case 2:
                        dict.Add(3, new Vector2(-1, 0));
                        dict.Add(1, new Vector2(-1, 1));
                        dict.Add(0, new Vector2(0, 1));
                        break;
                    case 3:
                        dict.Add(2, new Vector2(1, 0));
                        dict.Add(1, new Vector2(0, 1));
                        dict.Add(0, new Vector2(1, 1));
                        break;
                }
                foreach (var kvp in dict)
                {
                    var newTile = tile + kvp.Value;
                    if (TileHasScarecrow(location, newTile, kvp.Key))
                    {
                        tile = newTile;
                        which = kvp.Key;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TileHasScarecrow(GameLocation location, Vector2 tile, int which)
        {
            return location.terrainFeatures.TryGetValue(tile, out var tf) && tf.modData.ContainsKey(scarecrowKey + which);
        }

        public static bool ReturnScarecrow(Farmer who, GameLocation location, Vector2 placementTile, int which)
        {
            if (location.terrainFeatures.TryGetValue(placementTile, out var tf) && tf is HoeDirt && TryReturnScarecrow(who, location, tf, which))
            { 
                return true; 
            }
            else
            {
                Dictionary<int, Vector2> dict = new();
                switch (which)
                {
                    case 0:
                        dict.Add(3, new Vector2(-1, -1));
                        dict.Add(2, new Vector2(0, -1));
                        dict.Add(1, new Vector2(-1, 0));
                        break;
                    case 1:
                        dict.Add(3, new Vector2(0, -1));
                        dict.Add(2, new Vector2(1, 1));
                        dict.Add(0, new Vector2(1, 0));
                        break;
                    case 2:
                        dict.Add(3, new Vector2(-1, 0));
                        dict.Add(1, new Vector2(-1, 1));
                        dict.Add(0, new Vector2(0, 1));
                        break;
                    case 3:
                        dict.Add(2, new Vector2(1, 0));
                        dict.Add(1, new Vector2(0, 1));
                        dict.Add(0, new Vector2(1, 1));
                        break;
                }
                foreach (var kvp in dict)
                {
                    if (!location.terrainFeatures.TryGetValue(placementTile + kvp.Value, out var otf))
                        continue;
                    if (TryReturnScarecrow(who, location, otf, kvp.Key))
                        return true;
                }
            }
            return false;
        }

        public static bool TryReturnScarecrow(Farmer who, GameLocation location, TerrainFeature tf, int which)
        {
            Object scarecrow = null;
            if (tf.modData.TryGetValue(scarecrowKey + which, out var scarecrowString))
            {
                scarecrow = GetScarecrow(tf, which);
                tf.modData.Remove(scarecrowKey + which);
                tf.modData.Remove(scaredKey + which);
                tf.modData.Remove(guidKey + which);
                if (scarecrow is not null && !who.addItemToInventoryBool(scarecrow))
                {
                    who.currentLocation.debris.Add(new Debris(scarecrow, who.Position));
                }
                SMonitor.Log($"Returning {scarecrow.Name}");
                return true;
            }
            return false;
        }

        public static List<Vector2> GetScarecrowTiles(Vector2 tileLocation, int which, int radius)
        {
            Vector2 start = tileLocation + new Vector2(-1, -1) * (radius - 2);
            Vector2 position = tileLocation + GetScarecrowCorner(which) * 0.5f;
            List<Vector2> list = new();
            switch (which)
            {
                case 0:
                    start += new Vector2(-1, -1);
                    break;
                case 1:
                    start += new Vector2(0, -1);
                    break;
                case 2:
                    start += new Vector2(-1, 0);
                    break;
            }
            var diameter = (radius - 1) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Vector2 tile = start + new Vector2(x, y);
                    if ((int)Math.Ceiling(Vector2.Distance(position, tile)) <= radius)
                        list.Add(tile);
                }
            }
            return list;

        }
        public static bool IsScarecrowInRange(bool scarecrow, Farm f, Vector2 v)
        {
            if (!Config.EnableMod || scarecrow)
                return scarecrow;
            foreach (var kvp in f.terrainFeatures.Pairs)
            {
                if (kvp.Value is HoeDirt tf)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var obj = GetScarecrow(kvp.Value, i);
                        if(obj is null && sprinklerApi is not null && tf.modData.ContainsKey(sprinklerKey + i))
                        {
                            Vector2 tile = tf.Tile;
                            int corner = i;
                            var sprinkler = sprinklerApi.GetObjectAtTileCorner(tf.Location, ref tile, ref corner);
                            if (sprinkler?.IsScarecrow() == true)
                            {
                                obj = sprinkler;
                            }
                        }
                        if (obj is not null)
                        {
                            var tiles = GetScarecrowTiles(kvp.Key, i, obj.GetRadiusForScarecrow());
                            if (tiles.Contains(v))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


        public static void SetAltTextureForObject(Object obj)
        {
            if (atApi is null)
                return;
            try
            {
                AccessTools.Method(atApi.GetType(), "SetTextureForObject").Invoke(atApi, new object[] { obj });
            }
            catch
            {
            }
        }

        public static Texture2D GetAltTextureForObject(Object obj, out Rectangle sourceRect)
        {
            sourceRect = new Rectangle();
            var inputParams = new object[] { obj, sourceRect };
            Texture2D result = (Texture2D)AccessTools.Method(atApi.GetType(), "GetTextureForObject").Invoke(atApi, inputParams);
            sourceRect = (Rectangle)inputParams[1];
            return result;
        }

        public static bool RemoveWhere(KeyValuePair<Vector2, TerrainFeature> pair)
        {
            HoeDirt dirt = pair.Value as HoeDirt;
            var match = dirt != null && dirt.crop == null && Game1.random.NextDouble() < 0.8;
            
            if (!Config.EnableMod || !match)
                return match;

            for (int i = 0; i < 4; i++)
            {
                if (dirt.modData.TryGetValue(scarecrowKey + i, out var scarecrowString))
                {
                    SMonitor.Log($"Preventing hoedirt removal");
                    return false;
                }
            }
            return match;
        }
    }
}

using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace ImmersiveScarecrows
{
    public partial class ModEntry
    {

        private static Object GetScarecrow(string scarecrowString)
        {

            foreach (var kvp in Game1.bigCraftablesInformation)
            {
                if (kvp.Value.Equals(scarecrowString))
                {
                    var obj = new Object(Vector2.Zero, kvp.Key);
                    scarecrowDict[scarecrowString] = obj;
                    return obj;
                }
            }
            scarecrowString = scarecrowString.Split('/')[0];
            foreach (var kvp in Game1.bigCraftablesInformation)
            {
                if (kvp.Value.StartsWith(scarecrowString + "/"))
                {
                    var obj = new Object(Vector2.Zero, kvp.Key);
                    scarecrowDict[scarecrowString] = obj;
                    return obj;
                }
            }
            return null;
        }
        private static string GetScarecrowString(Object instance)
        {
            return Game1.bigCraftablesInformation.TryGetValue(instance.ParentSheetIndex, out var str) ? str : instance.Name;
        }
        private static Vector2 GetScarecrowCorner(int i)
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

        private static int GetMouseCorner()
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

        private static bool GetScarecrowTileBool(GameLocation location, ref Vector2 tile, ref int which, out string scarecrowString)
        {
            if ((scarecrowString = TileScarecrowString(location, tile, which)) is not null)
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
                    if ((scarecrowString = TileScarecrowString(location, newTile, kvp.Key)) is not null)
                    {
                        tile = newTile;
                        which = kvp.Key;
                        return true;
                    }
                }
            }
            return false;
        }

        private static string TileScarecrowString(GameLocation location, Vector2 tile, int which)
        {
            return (location.terrainFeatures.TryGetValue(tile, out var tf) && tf.modData.TryGetValue(scarecrowKey + which, out var scarecrowString)) ? scarecrowString : null;
        }

        private static bool ReturnScarecrow(Farmer who, GameLocation location, TerrainFeature tf, Vector2 placementTile, int which)
        {
            if (TryReturnScarecrow(who, location, tf, placementTile, which))
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
                    if (TryReturnScarecrow(who, location, otf, placementTile + kvp.Value, kvp.Key))
                        return true;
                }
            }
            return false;
        }

        private static bool TryReturnScarecrow(Farmer who, GameLocation location, TerrainFeature tf, Vector2 placementTile, int which)
        {
            Object scarecrow = null;
            if (tf.modData.TryGetValue(scarecrowKey + which, out var scarecrowString))
            {
                tf.modData.Remove(scarecrowKey + which);
                scarecrow = GetScarecrow(scarecrowString);
                if (scarecrow is not null && !who.addItemToInventoryBool(scarecrow))
                {
                    who.currentLocation.debris.Add(new Debris(scarecrow, who.Position));
                }
                return true;
            }
            return false;
        }

        private static List<Vector2> GetScarecrowTiles(Vector2 tileLocation, int which, int radius)
        {
            Vector2 start = tileLocation + new Vector2(-1, -1) * radius;
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
            var diameter = (radius + 1) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    list.Add(start + new Vector2(x, y));
                }
            }
            return list;

        }
        private static bool CheckForScarecrowInRange(Farm f, Vector2 v)
        {
            foreach (var kvp in f.terrainFeatures.Pairs)
            {
                if (kvp.Value is HoeDirt)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (kvp.Value.modData.TryGetValue(scarecrowKey + i, out var scarecrowString))
                        {
                            var obj = GetScarecrow(scarecrowString);
                            if (obj is not null)
                            {
                                int radius = obj.GetRadiusForScarecrow();
                                var distance = Vector2.Distance(kvp.Key + GetScarecrowCorner(i) * 32, v);
                                if (distance < radius)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

    }
}

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Object = StardewValley.Object;

namespace ImmersiveSprinklersAndScarecrows
{
    public partial class ModEntry
    {
        public static List<Vector2> GetScarecrowTiles(Vector2 tileLocation, int radius)
        {
            if(Config.ScarecrowRadius > 0)
            {
                radius = Config.ScarecrowRadius;
            }
            Vector2 start = tileLocation + new Vector2(-1, -1) * (radius - 2);
            Vector2 position = tileLocation + new Vector2(0.5f, 0.5f);
            List<Vector2> list = new();
            var diameter = (radius - 1) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Vector2 tile = start + new Vector2(x, y);
                    if(Config.SquareScarecrowRange || Math.Ceiling(Vector2.Distance(position, tile)) <= radius)
                        list.Add(tile);
                }
            }
            return list;

        }
        public static bool IsScarecrowInRange(bool scarecrow, Farm f, Vector2 v)
        {
            if (!Config.EnableMod)
                return false;
            foreach (var kvp in f.Objects.Pairs.Where(kvp => kvp.Value?.IsScarecrow() == true))
            {
                var obj = kvp.Value;
                var tiles = GetScarecrowTiles(obj.TileLocation, obj.GetRadiusForScarecrow());
                if (tiles.Contains(v))
                {
                    obj.SpecialVariable++;
                    SMonitor.Log($"Scarecrow at {obj.TileLocation} has scared {obj.SpecialVariable} crows");
                    return true;
                }
            }
            return false;
        }
    }
}
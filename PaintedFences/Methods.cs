
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace PaintedFences
{
    public partial class ModEntry
    {

        public static int GetWhichColor(Fence fence)
        {
            int which = 21;
            if(fence.modData.TryGetValue(colorKey, out var value))
            {
                int.TryParse(value, out which);
            }
            which = Math.Clamp(which, 0, 21);
            return which;
        }

        public static Color GetColor(Fence fence)
        {
            int which = GetWhichColor(fence);
            if (which == 21)
            {
                return Color.Transparent;
            }
            return DiscreteColorPicker.getColorFromSelection(which);
        }
        public static void IncrementColor(bool inc, Fence fence)
        {
            int which = 21;
            if (fence.modData.TryGetValue(colorKey, out var value))
            {
                int.TryParse(value, out which);
            }
            if (inc)
                which = (which + 1) % 22;
            else
            {
                which = (which == 0 ? 21 : which - 1);
            }
            fence.modData[colorKey] = which.ToString();
        }
        public static void GetBulkFences(Fence fence)
        {
            bulkFences.Value.Add(fence.TileLocation);
            int color = GetWhichColor(fence);
            foreach (var tile in new Vector2[] { new Vector2(0, -1), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(1, 0), })
            {
                if (!bulkFences.Value.Contains(tile + fence.TileLocation) 
                    && Game1.currentLocation.Objects.TryGetValue(tile + fence.TileLocation, out var otherObj) 
                    && otherObj is Fence otherFence && otherFence.ItemId == fence.ItemId 
                    && GetWhichColor(otherFence) == color)
                {
                    GetBulkFences(otherFence);
                }
            }
        }
        public static void ColorFences(bool inc)
        {
            foreach (var tile in bulkFences.Value)
            {
                if (Game1.currentLocation.Objects.TryGetValue(tile, out var obj) && obj is Fence fence)
                {
                    IncrementColor(inc, fence);
                }
            }
        }
        public static void UncolorFences(Fence fence, int color)
        {
            if (color == 21)
                return;
            fence.modData.Remove(colorKey);
            foreach (var tile in new Vector2[] { new Vector2(0, -1),new Vector2(0, 1),new Vector2(-1, 0),new Vector2(1, 0), })
            {
                if (Game1.currentLocation.Objects.TryGetValue(tile + fence.TileLocation, out var otherObj) && otherObj is Fence otherFence && GetWhichColor(otherFence) == color)
                {
                    UncolorFences(otherFence, color);
                }
            }
        }
    }
}
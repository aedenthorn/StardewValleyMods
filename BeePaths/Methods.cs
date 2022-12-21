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

namespace BeePaths
{
    public partial class ModEntry
    {
        private BeeData GetBee(Vector2 startTile, Vector2 endTile, bool random = true)
        {
            var start = startTile * 64 + new Vector2(Game1.random.Next(64), Game1.random.Next(64) - 32);
            var end = endTile * 64 + new Vector2(Game1.random.Next(64), Game1.random.Next(64) - 32);
            var pos = random ? Vector2.Lerp(start, end, (float)Game1.random.NextDouble()) : start;
            return new BeeData()
            {
                startPos = start,
                endPos = end,
                pos = pos,
                startTile = startTile,
                endTile = endTile
            };
        }
    }
}
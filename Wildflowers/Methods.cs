using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wildflowers
{
    public partial class ModEntry
    {
        private static int GetRandomFlowerSeed(int[] flowers)
        {
            var crops = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
            var idxs = new List<int>();
            foreach(var kvp in crops)
            {
                var split = kvp.Value.Split('/');
                if (!flowers.Contains(int.Parse(split[3])))
                    continue;
                if (!split[1].Split(' ').Contains(Game1.currentSeason))
                    continue;
                idxs.Add(kvp.Key);
            }
            return idxs[Game1.random.Next(idxs.Count)];
        }
    }
}
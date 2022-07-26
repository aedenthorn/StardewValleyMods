using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace FruitTreeTweaks
{
    public partial class ModEntry
    {
        private static Dictionary<GameLocation, Dictionary<Vector2, List<Vector2>>> fruitOffsets = new Dictionary<GameLocation, Dictionary<Vector2, List<Vector2>>>();
        private static Dictionary<GameLocation, Dictionary<Vector2, List<Color>>> fruitColors = new Dictionary<GameLocation, Dictionary<Vector2, List<Color>>>();
        private static Dictionary<GameLocation, Dictionary<Vector2, List<float>>> fruitSizes = new Dictionary<GameLocation, Dictionary<Vector2, List<float>>>();

        private static float GetTreeBottomOffset(FruitTree tree)
        {
            if (!Config.EnableMod)
                return 1E-07f;
            return 1E-07f + tree.currentTileLocation.X / 100000f;
        }
        private static bool TreesBlock()
        {
            return Config.TreesBlock;
        }
        private static bool FruitTreesBlock()
        {
            return Config.TreesBlock;
        }
        private static bool CanPlantAnywhere()
        {
            return Config.PlantAnywhere;
        }
        private static int GetMaxFruit()
        {
            return !Config.EnableMod ? 3 : Config.MaxFruitPerTree;
        }
        private static int GetFruitPerDay()
        {
            return !Config.EnableMod ? 1 : Game1.random.Next(Config.MinFruitPerDay, Math.Max(Config.MinFruitPerDay, Config.MaxFruitPerDay + 1));
        }
        private static Color GetFruitColor(FruitTree tree, int index)
        {
            if (!Config.EnableMod)
                return Color.White;
            if (!fruitColors.TryGetValue(tree.currentLocation, out Dictionary<Vector2, List<Color>> dict) || !dict.TryGetValue(tree.currentTileLocation, out List<Color> colors) || colors.Count < tree.fruitsOnTree.Value)
                ReloadFruit(tree.currentLocation, tree.currentTileLocation, tree.fruitsOnTree.Value);
            return fruitColors[tree.currentLocation][tree.currentTileLocation][index];
        }
        private static float GetFruitScale(FruitTree tree, int index)
        {
            if (!Config.EnableMod)
                return 4;
            if (!fruitSizes.TryGetValue(tree.currentLocation, out Dictionary<Vector2, List<float>> dict) || !dict.TryGetValue(tree.currentTileLocation, out List<float> sizes) || sizes.Count < tree.fruitsOnTree.Value)
                ReloadFruit(tree.currentLocation, tree.currentTileLocation, tree.fruitsOnTree.Value);
            return fruitSizes[tree.currentLocation][tree.currentTileLocation][index];
        }
        private static Vector2 GetFruitOffsetForShake(FruitTree tree, int index)
        {
            if (!Config.EnableMod || index < 2)
                return Vector2.Zero;
            return GetFruitOffset(tree, index);
        }
        private static int GetFruitQualityDays(int days)
        {
            if (!Config.EnableMod)
                return days;
            switch (days)
            {
                case -112:
                    return -Config.DaysUntilSilverFruit;
                case -224:
                    return -Config.DaysUntilGoldFruit;
                case -336:
                    return -Config.DaysUntilIridiumFruit;
            }
            return days;
        }
        private static Vector2 GetFruitOffset(FruitTree tree, int index)
        {
            if (!fruitOffsets.TryGetValue(tree.currentLocation, out Dictionary<Vector2, List<Vector2>> dict) || !dict.TryGetValue(tree.currentTileLocation, out List<Vector2> offsets) || offsets.Count < tree.fruitsOnTree.Value)
                ReloadFruit(tree.currentLocation, tree.currentTileLocation, tree.fruitsOnTree.Value);
            return fruitOffsets[tree.currentLocation][tree.currentTileLocation][index];
        }
        private static int ChangeDaysToMatureCheck(int oldValue)
        {
            if (!Config.EnableMod)
                return oldValue;
            switch (oldValue)
            {
                case 0:
                    return 0;
                case 7:
                    return Config.DaysUntilMature / 4;
                case 14:
                    return Config.DaysUntilMature / 2;
                case 21:
                    return Config.DaysUntilMature * 3 / 4;
            }
            return oldValue;
        }

        private static void ReloadFruit(GameLocation location, Vector2 tileLocation, int max)
        {
            if (!fruitOffsets.ContainsKey(location))
                fruitOffsets.Add(location, new Dictionary<Vector2, List<Vector2>>());
            if (!fruitOffsets[location].TryGetValue(tileLocation, out List<Vector2> offsets))
            {
                offsets = new List<Vector2>();
                fruitOffsets[location][tileLocation] = offsets;
            }
            if (!fruitColors.ContainsKey(location))
                fruitColors.Add(location, new Dictionary<Vector2, List<Color>>());
            if (!fruitColors[location].TryGetValue(tileLocation, out List<Color> colors))
            {
                colors = new List<Color>();
                fruitColors[location][tileLocation] = colors;
            }
            if (!fruitSizes.ContainsKey(location))
                fruitSizes.Add(location, new Dictionary<Vector2, List<float>>());
            if (!fruitSizes[location].TryGetValue(tileLocation, out List<float> sizes))
            {
                sizes = new List<float>();
                fruitSizes[location][tileLocation] = sizes;
            }
            if (offsets.Count != max)
            {
                offsets.Clear();
                colors.Clear();
                sizes.Clear();
                SMonitor.Log($"Resetting fruit offsets for {tileLocation} in {location.Name}");
                for (int i = 0; i < max; i++)
                {
                    var color = Color.White;
                    color.R -= (byte)(Game1.random.NextDouble() * Config.ColorVariation);
                    color.G -= (byte)(Game1.random.NextDouble() * Config.ColorVariation);
                    color.B -= (byte)(Game1.random.NextDouble() * Config.ColorVariation);
                    colors.Add(color);

                    sizes.Add(4 * (float)(1 + ((Game1.random.NextDouble() * 2 - 1) * Config.SizeVariation / 100)));

                    if (i < 3)
                    {
                        offsets.Add(Vector2.Zero);
                        continue;
                    }
                    bool gotSpot = false;
                    Vector2 offset;
                    while (!gotSpot)
                    {
                        double distance = 24;
                        for (int j = 0; j < 100; j++)
                        {
                            gotSpot = true;
                            offset = new Vector2(Config.FruitSpawnBufferX + Game1.random.Next(34 * 4 - Config.FruitSpawnBufferX), Config.FruitSpawnBufferY + Game1.random.Next(58 * 4 - Config.FruitSpawnBufferY));
                            for (int k = 0; k < offsets.Count; k++)
                            {
                                if (Vector2.Distance(offsets[k], offset) < distance)
                                {
                                    distance--;
                                    gotSpot = false;
                                    break;
                                }
                            }
                            if (gotSpot)
                            {
                                offsets.Add(offset);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
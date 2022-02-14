using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace FieldHarvest
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        public static bool harvestingField = false;
        private static void Crop_harvest_Prefix(Crop __instance, JunimoHarvester junimoHarvester)
        {
            if (!harvestingField || Config.AutoCollect)
                return;
            __instance.harvestMethod.Value = 1;
        }
        private static bool HoeDirt_performUseAction_Prefix(HoeDirt __instance, Vector2 tileLocation, GameLocation location, ref bool __result)
        {
            if (!Config.EnableMod || !SHelper.Input.IsDown(Config.ModButton) || (Config.OnlySameSeed && __instance.crop == null))
                return true;
            SMonitor.Log($"Harvesting all");

            Crop sameCrop = __instance.crop;

            List<Vector2> hoedirts = new List<Vector2>();
            hoedirts.Add(tileLocation);
            GetHoeDirt(tileLocation, hoedirts, location);
            SMonitor.Log($"Got {hoedirts.Count} hoe dirt tiles");
            //hoedirts.Sort(delegate (Vector2 p1, Vector2 p2) { return Vector2.Distance(tileLocation, p1).CompareTo(Vector2.Distance(tileLocation, p2)); });


            int count = 0;
            foreach (var p in hoedirts)
            {
                HoeDirt dirt = location.terrainFeatures[p] as HoeDirt;
                if (dirt.crop == null || (sameCrop != null && Config.OnlySameSeed && dirt.crop.netSeedIndex.Value != sameCrop.netSeedIndex.Value))
                    continue;
                harvestingField = true;
                if (dirt.crop.harvest((int)p.X, (int)p.Y, dirt))
                {
                    if (location != null && location is IslandLocation && Game1.random.NextDouble() < 0.05)
                    {
                        Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tileLocation.X * 64, (int)tileLocation.Y * 64, 5, 1);
                    }
                    (location.terrainFeatures[p] as HoeDirt).crop = null;
                    (location.terrainFeatures[p] as HoeDirt).nearWaterForPaddy.Value = -1;

                    count++;
                }
                harvestingField = false;
            }
            if (count > 0)
            {
                if(!Config.AutoCollect)
                    Game1.player.currentLocation.playSound("harvest");
                __result = true;
                SMonitor.Log($"Harvested {count} crops");
            }
            return false;
        }


        private static void GetHoeDirt(Vector2 tile, List<Vector2> hoedirts, GameLocation location)
        {
            List<Vector2> tiles = new List<Vector2>();
            int x = (int)tile.X;
            int y = (int)tile.Y;
            if (Config.AllowDiagonal)
            {
                for (int i = x - 1; i < x + 2; i++)
                {
                    for (int j = y - 1; j < y + 2; j++)
                    {
                        if (i == x && j == y)
                            continue;
                        Vector2 p = new Vector2(i, j);
                        if (!hoedirts.Contains(p))
                            tiles.Add(p);
                    }
                }
            }
            else
            {
                if (!hoedirts.Contains(new Vector2(x - 1, y)))
                    tiles.Add(new Vector2(x - 1, y));
                if (!hoedirts.Contains(new Vector2(x + 1, y)))
                    tiles.Add(new Vector2(x + 1, y));
                if (!hoedirts.Contains(new Vector2(x, y - 1)))
                    tiles.Add(new Vector2(x, y - 1));
                if (!hoedirts.Contains(new Vector2(x, y + 1)))
                    tiles.Add(new Vector2(x, y + 1));
            }
            for (int i = tiles.Count - 1; i >= 0; i--)
            {
                if (!location.terrainFeatures.TryGetValue(tiles[i], out TerrainFeature feature) || feature is not HoeDirt)
                {
                    tiles.RemoveAt(i);
                }
                else
                {
                    hoedirts.Add(tiles[i]);
                }
            }
            foreach (var v in tiles)
            {
                GetHoeDirt(v, hoedirts, location);
            }
        }

    }
}
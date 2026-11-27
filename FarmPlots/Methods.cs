using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Locations;
using StardewValley.Internal;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace FarmPlots
{
    public partial class ModEntry
    {
        public static bool TryGetAutoPlot(out AutoPlot plot, Vector2 tile)
        {
            plot = null;
            if (!Config.EnableMod || Game1.currentLocation == null)
                return false;
            if (!TryGetAutoPlots(Game1.currentLocation, out var list))
            {
                return false;
            }
            foreach (var p in list)
            {
                if (p.tiles.Contains(tile))
                {
                    plot = p;
                    return true;
                }
            }
            return false;
        }
        public static bool TryGetAutoPlots(GameLocation l, out List<AutoPlot> list)
        {
            list = new List<AutoPlot>();
            if (!Config.EnableMod || l == null)
                return false;
            if (!locationDict.TryGetValue(l, out list))
            {
                if (!l.modData.TryGetValue(plotsKey, out var plotString))
                {
                    return false;
                }
                list = JsonConvert.DeserializeObject<List<AutoPlot>>(plotString); ;
                locationDict[l] = list;
            }
            return true;
        }
        public static void AddAutoPlot(Rectangle rect, Vector2 startTile)
        {
            HashSet<Vector2> tiles = new HashSet<Vector2>();
            for (int x = rect.X;  x < rect.X + rect.Width; x++)
            {
                for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    var tile = new Vector2(x, y);
                    if (!TryGetAutoPlot(out AutoPlot aPlot, tile))
                    {
                        tiles.Add(tile);
                    }
                }
            }
            if (!tiles.Any())
                return;
            bool exists = locationDict.TryGetValue(Game1.currentLocation, out var list);
            if (exists && TryGetAutoPlot(out AutoPlot plot, startTile))
            {
                plot.tiles.AddRange(tiles);
            }
            else
            {
                plot = new AutoPlot()
                {
                    tiles = tiles,
                    creator = Game1.player.UniqueMultiplayerID
                };
                if (!exists)
                {
                    if (Game1.currentLocation.modData.TryGetValue(plotsKey, out var plotString))
                    {
                        list = JsonConvert.DeserializeObject<List<AutoPlot>>(plotString);
                    }
                    else
                    {
                        list = new List<AutoPlot>();
                    }
                    locationDict[Game1.currentLocation] = list;
                }
                list.Add(plot);
            }
            currentPlot.Value = plot;
            Game1.currentLocation.modData[plotsKey] = JsonConvert.SerializeObject(list);
        }
        public static void RemoveAutoPlot(Rectangle rect, Vector2 startTile)
        {
            if (locationDict.TryGetValue(Game1.currentLocation, out var list) && TryGetAutoPlot(out AutoPlot plot, startTile))
            {
                bool changed = false;
                for (int x = rect.X; x < rect.X + rect.Width; x++)
                {
                    for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                    {
                        var tile = new Vector2(x, y);
                        if (plot.tiles.Contains(tile))
                        {
                            plot.tiles.Remove(tile);
                            changed = true;
                        }
                    }
                }
                if (changed)
                {
                    if (!plot.tiles.Any())
                    {
                        list.Remove(plot);
                    }
                    else
                    {
                        currentPlot.Value = plot;
                    }
                    Game1.currentLocation.modData[plotsKey] = JsonConvert.SerializeObject(list);
                }
            }
        }

        public static Rectangle GetCurrentDragRect()
        {
            Point start = new();
            Point size = new();
            if (startTile.Value.X < Game1.currentCursorTile.X)
            {
                start.X = (int)startTile.Value.X;
                size.X = (int)(Game1.currentCursorTile.X - startTile.Value.X + 1);
            }
            else
            {
                start.X = (int)Game1.currentCursorTile.X;
                size.X = (int)(startTile.Value.X - Game1.currentCursorTile.X + 1);
            } 
            if (startTile.Value.Y < Game1.currentCursorTile.Y)
            {
                start.Y = (int)startTile.Value.Y;
                size.Y = (int)(Game1.currentCursorTile.Y - startTile.Value.Y + 1);
            }
            else
            {
                start.Y = (int)Game1.currentCursorTile.Y;
                size.Y = (int)(startTile.Value.Y - Game1.currentCursorTile.Y + 1);
            } 
            return new Rectangle(start, size);
        }
        public static void ActivatePlot(GameLocation location, AutoPlot plot)
        {
            if (!Config.EnableMod)
                return;
            List<Object> chests = location.Objects.Values.Where(o => o is Chest).ToList();
            Random shopRandom = Utility.CreateDaySaveRandom(0.0, 0.0, 0.0);
            var who = Game1.GetPlayer(plot.creator);
            bool late = false;

            var fert = plot.fertilizers[(int)Game1.season];
            var seed = plot.seeds[(int)Game1.season];
            if (seed != null)
            {
                seed = Crop.ResolveSeedId(seed, location);
                if (Crop.TryGetData(seed, out var data))
                {
                    int days = 0;
                    foreach (var d in data.DaysInPhase)
                    {
                        days += d;
                    }
                    if (Game1.dayOfMonth + days > 28)
                    {
                        int nextSeason = (((int)Game1.season) + 1) % 4;
                        if (!data.Seasons.Contains((Season)nextSeason))
                        {
                            late = true;
                        }
                    }
                }
            }

            foreach (var tile in plot.tiles)
            {
                foreach(var b in location.buildings)
                {
                    if (b.occupiesTile(tile))
                        goto next;
                }
                if(location.getLargeTerrainFeatureAt((int)tile.X, (int)tile.Y) != null)
                    goto next;
                location.terrainFeatures.TryGetValue(tile, out var tf);
                location.objects.TryGetValue(tile, out var obj);
                if (plot.till[(int)Game1.season])
                {
                    if(obj != null)
                    {
                        if (obj.isDebrisOrForage())
                        {
                            location.objects.Remove(tile);
                        }
                        else if (obj is IndoorPot pot)
                        {
                            tf = pot.hoeDirt.Value;
                        }
                        else
                        {
                            goto next;
                        }
                    }
                    if(tf is Grass)
                    {
                        location.terrainFeatures.Remove(tile);
                        tf = null;
                    }
                    if(tf is null && location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back", false) != null)
                    {
                        location.makeHoeDirt(tile, true);
                        location.terrainFeatures.TryGetValue(tile, out tf);
                    }
                }
                if (tf is not HoeDirt dirt)
                    goto next;

                if (plot.harvest[(int)Game1.season] && dirt.readyForHarvest())
                {
                    var harvest = Harvest(tile, dirt.crop);
                    chests.Sort(delegate (Object a, Object b)
                    {
                        return Vector2.Distance(a.TileLocation, tile).CompareTo(Vector2.Distance(b.TileLocation, tile));
                    });
                    for(int i = 0; i < harvest.Count; i++)
                    {
                        foreach(Chest c in chests)
                        {
                            harvest[i] = c.addItem(harvest[i]);
                            if (harvest[i] == null)
                                break;
                        }
                        if (harvest[i] != null)
                        {
                            Game1.createItemDebris(harvest[i], new Vector2(tile.X * 64f + 32f, tile.Y * 64f + 32f), -1, null, -1, false);
                        }
                    }
                }
                if (late)
                    goto next;
                if (fert != null && dirt.CanApplyFertilizer(fert))
                {
                    bool found = false;
                    foreach(Chest chest in chests)
                    {
                        if (chest.Items.ContainsId(fert))
                        {
                            Fertilize(location, dirt, fert, who);
                            chest.Items.ReduceId(fert, 1);
                            found = true;
                            break;
                        }
                    }
                    if(!found && plot.buy[(int)Game1.season])
                    {
                        var price = GetPrice(fert, shopRandom);
                        if(price > 0 && who?.Money >= price)
                        {
                            Fertilize(location, dirt, fert, who);
                            who.Money -= price;
                        }
                    }
                }
                if (seed != null && dirt.canPlantThisSeedHere(seed))
                {
                    bool found = false;
                    foreach(Chest chest in location.Objects.Values.Where(o => o is Chest))
                    {
                        if (chest.Items.ContainsId(seed))
                        {
                            if(Plant(location, dirt, seed, who))
                            {
                                chest.Items.ReduceId(seed, 1);
                                found = true;
                            }
                            break;
                        }
                    }
                    if(!found && plot.buy[(int)Game1.season])
                    {
                        var price = GetPrice(seed, shopRandom);
                        if (price > 0 && who?.Money >= price)
                        {
                            if (Plant(location, dirt, seed, who))
                            {
                                who.Money -= price;
                            }
                        }
                    }
                }
            next:
                continue;
            }
        }

        private static void Fertilize(GameLocation location, HoeDirt dirt, string itemId, Farmer who)
        {
            dirt.fertilizer.Value = ItemRegistry.QualifyItemId(itemId) ?? itemId;
            if (who != null)
                dirt.applySpeedIncreases(who);
        }
        private static bool Plant(GameLocation location, HoeDirt dirt, string itemId, Farmer who)
        {
            Season season = location.GetSeason();
            Point tilePos = Utility.Vector2ToPoint(dirt.Tile);
            
            CropData cropData;
            if (!Crop.TryGetData(itemId, out cropData) || cropData.Seasons.Count == 0)
            {
                return false;
            }
            Object obj;
            bool isGardenPot = location.objects.TryGetValue(dirt.Tile, out obj) && obj is IndoorPot;
            bool isIndoorPot = isGardenPot && !location.IsOutdoors;
            string text = itemId;
            bool flag = isGardenPot;
            bool flag2;
            if (!isIndoorPot)
            {
                LocationData data = location.GetData();
                flag2 = ((data != null) ? data.CanPlantHere : null) ?? location.IsFarm;
            }
            else
            {
                flag2 = true;
            }
            string deniedMessage;
            if (!location.CheckItemPlantRules(text, flag, flag2, out deniedMessage))
            {
                return false;
            }
            if (!isIndoorPot && !location.CanPlantSeedsHere(itemId, tilePos.X, tilePos.Y, isGardenPot, out deniedMessage))
            {
                return false;
            }
            if (!isIndoorPot && !location.SeedsIgnoreSeasonsHere())
            {
                List<Season> seasons = cropData.Seasons;
                bool? flag4 = ((seasons != null) ? new bool?(seasons.Contains(season)) : null);
                if (flag4 == null || !flag4.GetValueOrDefault())
                {
                    return false;
                }
            }
            dirt.crop = new Crop(itemId, tilePos.X, tilePos.Y, dirt.Location);
            Stats stats = Game1.stats;
            uint seedsSown = stats.SeedsSown;
            stats.SeedsSown = seedsSown + 1U;
            if(who != null)
                dirt.applySpeedIncreases(who);
            dirt.nearWaterForPaddy.Value = -1;
            if (dirt.hasPaddyCrop() && dirt.paddyWaterCheck(false))
            {
                dirt.state.Value = 1;
                dirt.updateNeighbors();
            }
            return true;
        }

        public static int GetPrice(string id, Random shopRandom)
        {
            float price = -1;
            DataLoader.Shops(Game1.content).TryGetValue("SeedShop", out var shop);
            var itemData = shop?.Items.FirstOrDefault(i => i.ItemId == "(O)"+id);
            if (itemData != null)
            {
                var item = ItemRegistry.Create(itemData.ItemId);
                price = (float)ShopBuilder.GetBasePrice(new ItemQueryResult(item), shop, itemData, item, false, itemData.UseObjectDataPrice);
                price = Utility.ApplyQuantityModifiers(price, itemData.PriceModifiers, itemData.PriceModifierMode, null, null, item as Item, null, shopRandom);
            }
            return (int)price;
        }

        public static List<Item> Harvest(Vector2 tile, Crop crop)
        {
            if (crop.dead.Value)
            {
                crop.Dirt.destroyCrop(false);
            }
            List<Item> harvest = new List<Item>();
            if (crop.forageCrop.Value)
            {
                Object o = null;
                int experience = 3;
                Random r = Utility.CreateDaySaveRandom((double)(tile.X * 1000), (double)(tile.Y * 2000), 0.0);
                string text = crop.whichForageCrop.Value;
                if (!(text == "1"))
                {
                    if (text == "2")
                    {
                        return null;
                    }
                }
                else
                {
                    o = ItemRegistry.Create<Object>("(O)399", 1, 0, false);
                }
                if (Game1.player.professions.Contains(16))
                {
                    o.Quality = 4;
                }
                else if (r.NextDouble() < (double)((float)Game1.player.ForagingLevel / 30f))
                {
                    o.Quality = 2;
                }
                else if (r.NextDouble() < (double)((float)Game1.player.ForagingLevel / 15f))
                {
                    o.Quality = 1;
                }
                Game1.stats.ItemsForaged += (uint)o.Stack;
                Game1.player.gainExperience(2, experience);
                harvest.Add(o);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(crop.indexOfHarvest.Value))
                {
                    return null;
                }
                CropData data = crop.GetData();
                Random r2 = Utility.CreateRandom((double)tile.X * 7.0, (double)tile.Y * 11.0, Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, 0.0);
                int fertilizerQualityLevel = crop.Dirt.GetFertilizerQualityBoostLevel();
                double chanceForGoldQuality = 0.2 * ((double)Game1.player.FarmingLevel / 10.0) + 0.2 * (double)fertilizerQualityLevel * (((double)Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
                double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);
                int cropQuality = 0;
                if (fertilizerQualityLevel >= 3 && r2.NextDouble() < chanceForGoldQuality / 2.0)
                {
                    cropQuality = 4;
                }
                else if (r2.NextDouble() < chanceForGoldQuality)
                {
                    cropQuality = 2;
                }
                else if (r2.NextDouble() < chanceForSilverQuality || fertilizerQualityLevel >= 3)
                {
                    cropQuality = 1;
                }
                cropQuality = MathHelper.Clamp(cropQuality, (data != null) ? data.HarvestMinQuality : 0, ((data != null) ? data.HarvestMaxQuality : null).GetValueOrDefault(cropQuality));
                int numToHarvest = 1;
                if (data != null)
                {
                    int minStack = data.HarvestMinStack;
                    int maxStack = Math.Max(minStack, data.HarvestMaxStack);
                    if (data.HarvestMaxIncreasePerFarmingLevel > 0f)
                    {
                        maxStack += (int)((float)Game1.player.FarmingLevel * data.HarvestMaxIncreasePerFarmingLevel);
                    }
                    if (minStack > 1 || maxStack > 1)
                    {
                        numToHarvest = r2.Next(minStack, maxStack + 1);
                    }
                }
                if (data != null && data.ExtraHarvestChance > 0.0)
                {
                    while (r2.NextDouble() < Math.Min(0.9, data.ExtraHarvestChance))
                    {
                        numToHarvest++;
                    }
                }
                Item item;
                if (!crop.programColored.Value)
                {
                    item = ItemRegistry.Create(crop.indexOfHarvest.Value, 1, cropQuality, false);
                }
                else
                {
                    (item = new ColoredObject(crop.indexOfHarvest.Value, 1, crop.tintColor.Value)).Quality = cropQuality;
                }
                Item baseItem = item;
                harvest.Add(baseItem.getOne());
                if (crop.indexOfHarvest.Value == "421")
                {
                    crop.indexOfHarvest.Value = "431";
                    numToHarvest = r2.Next(1, 4);
                }
                var extraItem = (crop.programColored.Value ? new ColoredObject(crop.indexOfHarvest.Value, 1, crop.tintColor.Value) : ItemRegistry.Create(crop.indexOfHarvest.Value, 1, 0, false));
                int price = 0;
                Object obj = extraItem as Object;
                if (obj != null)
                {
                    price = obj.Price;
                }
                float experience2 = (float)(16.0 * Math.Log(0.018 * (double)price + 1.0, 2.718281828459045));
                Game1.player.gainExperience(0, (int)Math.Round((double)experience2));
                obj.Stack = numToHarvest - 1;
                if(obj.Stack > 0)
                {
                    harvest.Add(obj);
                }
                string text = crop.indexOfHarvest.Value;
                if (!(text == "262"))
                {
                    if (text == "771")
                    {
                        if (r2.NextDouble() < 0.1)
                        {
                            Item mixedSeeds = ItemRegistry.Create("(O)770", 1, 0, false);
                            harvest.Add((Object)mixedSeeds.getOne());
                        }
                    }
                }
                else if (r2.NextDouble() < 0.4)
                {
                    Item hay_item = ItemRegistry.Create("(O)178", 1, 0, false);
                    harvest.Add((Object)hay_item.getOne());

                }
                int regrowDays = ((data != null) ? data.RegrowDays : (-1));
                if (regrowDays > 0)
                {
                    crop.fullyGrown.Value = true;
                    if (crop.dayOfCurrentPhase.Value == regrowDays)
                    {
                        crop.updateDrawMath(crop.tilePosition);
                    }
                    crop.dayOfCurrentPhase.Value = regrowDays;
                    return harvest;
                }
            }
            crop.Dirt.destroyCrop(false);
            return harvest;
        }
    }
}
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FruitTreeShaker
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        /// <summary>Per-tree save data: the day number each fruit currently on the tree appeared, oldest first, comma-separated.</summary>
        public const string fruitDaysKey = "Aedenthorn.FruitTreeShaker/fruitDays";

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsMainPlayer)
                return;

            int today = SDate.Now().DaysSinceStart;
            IEnumerable<GameLocation> locations = Config.FarmOnly ? new[] { Game1.getFarm() } : Game1.locations;
            foreach (GameLocation location in locations)
            {
                bool storm = Config.DropAllInStorm && location.IsOutdoors && location.IsLightningHere();
                foreach (var pair in location.terrainFeatures.Pairs.ToList())
                {
                    if (pair.Value is FruitTree fruitTree)
                    {
                        UpdateFruitTree(fruitTree, pair.Key, today, storm);
                    }
                    else if (pair.Value is Tree tree && tree.hasSeed.Value)
                    {
                        bool palm = tree.treeType.Value is Tree.palmTree or Tree.palmTree2;
                        if ((Config.ShakePalmTrees && palm) || (Config.ShakeNormalTrees && !palm))
                            tree.shake(pair.Key, false);
                    }
                }
            }
        }

        private static void UpdateFruitTree(FruitTree tree, Vector2 tile, int today, bool storm)
        {
            int count = tree.fruit.Count;
            List<int> days = ReadFruitDays(tree);

            // reconcile with what the game did overnight: new fruit appeared today; fewer fruit means the tree was shaken
            if (count == 0)
                days.Clear();
            else if (count > days.Count)
                days.AddRange(Enumerable.Repeat(today, count - days.Count));
            else if (count < days.Count)
                days.RemoveRange(0, days.Count - count);

            if (count > 0 && storm)
            {
                tree.shake(tile, true);
                days.Clear();
            }
            else
            {
                while (days.Count > 0 && today - days[0] >= Config.DaysUntilFruitFalls && DropOldestFruit(tree, tile))
                    days.RemoveAt(0);
            }

            if (days.Count > 0)
                tree.modData[fruitDaysKey] = string.Join(",", days);
            else
                tree.modData.Remove(fruitDaysKey);
        }

        private static List<int> ReadFruitDays(FruitTree tree)
        {
            var days = new List<int>();
            if (tree.modData.TryGetValue(fruitDaysKey, out string raw))
            {
                foreach (string part in raw.Split(','))
                {
                    if (int.TryParse(part, out int day))
                        days.Add(day);
                }
            }
            return days;
        }

        /// <summary>Drop the oldest fruit on the ground the same way <see cref="FruitTree.shake"/> does, one fruit instead of all.</summary>
        private static bool DropOldestFruit(FruitTree tree, Vector2 tile)
        {
            if (tree.fruit.Count == 0 || tree.growthStage.Value < 4 || tree.stump.Value || tree.Location is null)
                return false;

            int quality = tree.GetQuality();
            var origin = new Vector2(tile.X * 64f + 32f, (tile.Y - 3f) * 64f + 32f) + new Vector2(-64f, 0f);
            var target = new Vector2(tile.X * 64f + 32f, tile.Y * 64f + 64f);
            Debris debris;
            if (tree.struckByLightningCountdown.Value <= 0)
            {
                Item item = tree.fruit[0];
                tree.fruit.RemoveAt(0);
                debris = new Debris(item, origin, target) { itemQuality = quality };
            }
            else
            {
                tree.fruit.RemoveAt(0);
                debris = new Debris("382", origin, target) { itemQuality = quality };
            }
            debris.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
            debris.chunkFinalYLevel = (int)(tile.Y * 64f + 64f);
            tree.Location.debris.Add(debris);
            return true;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("ModEnabled"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            foreach (var p in typeof(ModConfig).GetProperties())
            {
                if (p.Name == nameof(Config.EnableMod))
                    continue;
                if (p.PropertyType == typeof(bool))
                {
                    configMenu.AddBoolOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get(p.Name),
                        tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => (bool)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value)
                    );
                }
                else if (p.PropertyType == typeof(int))
                {
                    configMenu.AddNumberOption(
                        mod: ModManifest,
                        name: () => SHelper.Translation.Get(p.Name),
                        tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                        getValue: () => (int)p.GetValue(Config),
                        setValue: value => p.SetValue(Config, value),
                        min: 0
                    );
                }
            }
        }
    }
}

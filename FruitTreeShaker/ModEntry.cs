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

        /// <summary>Per-tree save data, one entry per fruit currently on the tree in the same order as <see cref="FruitTree.fruit"/>:
        /// <c>appearDay:lifetime</c>, where lifetime is the random number of days this fruit stays on the tree (used only when RandomDrops is on).</summary>
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
            List<FruitRecord> fruits = ReadFruitRecords(tree);

            // reconcile with what the game did overnight: new fruit appeared today; fewer fruit means the tree was shaken
            if (count == 0)
                fruits.Clear();
            else if (count > fruits.Count)
                fruits.AddRange(Enumerable.Range(0, count - fruits.Count).Select(_ => new FruitRecord(today, RollLifetime())));
            else if (count < fruits.Count)
                fruits.RemoveRange(0, fruits.Count - count);

            if (count > 0 && storm)
            {
                tree.shake(tile, true);
                fruits.Clear();
            }
            else
            {
                for (int i = fruits.Count - 1; i >= 0; i--)
                {
                    int lifetime = Config.RandomDrops ? fruits[i].Lifetime : Config.DaysRipeBeforeFalling;
                    if (today - fruits[i].AppearDay >= lifetime && DropFruit(tree, tile, i))
                        fruits.RemoveAt(i);
                }
            }

            if (fruits.Count > 0)
                tree.modData[fruitDaysKey] = string.Join(",", fruits.Select(f => $"{f.AppearDay}:{f.Lifetime}"));
            else
                tree.modData.Remove(fruitDaysKey);
        }

        private record struct FruitRecord(int AppearDay, int Lifetime);

        /// <summary>Pick how many days a new fruit stays on the tree when RandomDrops is on.</summary>
        private static int RollLifetime()
        {
            int min = Math.Max(0, Config.DaysRipeBeforeFalling);
            int max = Math.Max(min, Config.MaxDaysRipeBeforeFalling);
            return Game1.random.Next(min, max + 1);
        }

        private static List<FruitRecord> ReadFruitRecords(FruitTree tree)
        {
            var fruits = new List<FruitRecord>();
            if (tree.modData.TryGetValue(fruitDaysKey, out string raw))
            {
                foreach (string part in raw.Split(','))
                {
                    string[] fields = part.Split(':');
                    if (fields.Length > 0 && int.TryParse(fields[0], out int day))
                        fruits.Add(new FruitRecord(day, fields.Length > 1 && int.TryParse(fields[1], out int life) ? life : RollLifetime()));
                }
            }
            return fruits;
        }

        /// <summary>Drop one fruit on the ground the same way <see cref="FruitTree.shake"/> does, one fruit instead of all.</summary>
        private static bool DropFruit(FruitTree tree, Vector2 tile, int index)
        {
            if (index < 0 || index >= tree.fruit.Count || tree.growthStage.Value < 4 || tree.stump.Value || tree.Location is null)
                return false;

            int quality = tree.GetQuality();
            Vector2 offset = index switch
            {
                0 => new Vector2(-64f, 0f),
                1 => new Vector2(64f, -32f),
                2 => new Vector2(0f, 32f),
                _ => Vector2.Zero,
            };
            var origin = new Vector2(tile.X * 64f + 32f, (tile.Y - 3f) * 64f + 32f) + offset;
            var target = new Vector2(tile.X * 64f + 32f, tile.Y * 64f + 64f);
            Debris debris;
            if (tree.struckByLightningCountdown.Value <= 0)
            {
                Item item = tree.fruit[index];
                tree.fruit.RemoveAt(index);
                debris = new Debris(item, origin, target) { itemQuality = quality };
            }
            else
            {
                tree.fruit.RemoveAt(index);
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

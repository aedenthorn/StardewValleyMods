using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace Dreamscape
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string mapAssetKey;
        public static Texture2D palmTreeTexture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                e.Edit(AddLocation);
            }
        }

        private void AddLocation(IAssetData obj)
        {
            obj.AsDictionary<string, string>().Data["Dreamscape"] = "486 .3 454 .3/486 .3 454 .3/486 .3 454 .3/486 .3 454 .3/-1/-1/-1/-1/386 .1 384 .3 380 .3 378 1";
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                palmTreeTexture = Game1.content.Load<Texture2D>("aedenthorn.Dreamscape/tree_palm");
            }
            catch
            {
                palmTreeTexture = Helper.ModContent.Load<Texture2D>("assets/tree_palm.png");
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            var location = Game1.getLocationFromName("Dreamscape");
            var layer = location.map.GetLayer("Back");
            var mine = new MineShaft();
            mine.mineLevel = 200;
            int trees = location.terrainFeatures.Values.Where(v => v is Tree).Count();
            for (int x = 0; x < layer.LayerWidth; x++)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    Vector2 tile = new Vector2(x, y);
                    if (!location.objects.ContainsKey(tile) && !location.terrainFeatures.ContainsKey(tile) && layer.Tiles[new Location(x, y)]?.TileIndex == 26)
                    {
                        if (trees < Config.MaxTrees && Game1.random.NextDouble() < Config.TreeChancePercent / 100f)
                        {
                            location.terrainFeatures.Add(tile, new Tree(6, Config.TreeGrowthStage));
                        }
                        else if (Game1.random.NextDouble() < Config.ObjectChancePercent / 100f)
                        {
                            double chance = Game1.random.NextDouble();
                            if (chance < 0.5)
                            {
                                location.objects.Add(tile, new Object(tile, 319 + Game1.random.Next(3), "Weeds", true, false, false, false)
                                {
                                    Fragility = 2,
                                    CanBeGrabbed = true,
                                });
                            }
                            else if (chance < 0.65)
                            {
                                location.objects[tile] = new Object(tile, 80, "Stone", true, true, false, true);
                            }
                            else if (chance < 0.74)
                            {
                                location.objects[tile] = new Object(tile, 86, "Stone", true, true, false, true);
                            }
                            else if (chance < 0.83)
                            {
                                location.objects[tile] = new Object(tile, 84, "Stone", true, true, false, true);
                            }
                            else if (chance < 0.90)
                            {
                                location.objects[tile] = new Object(tile, 82, "Stone", true, true, false, true);
                            }
                            else
                            {
                                location.objects.Add(tile, new Object(tile, (int)AccessTools.Method(typeof(MineShaft), nameof(MineShaft.getRandomGemRichStoneForThisLevel)).Invoke(mine, new object[] { 200 }), "Stone", true, false, false, false));
                            }
                        }
                    }
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Trees",
                tooltip: () => "Max # of existing trees after which to prevent spawning more trees at the start of day",
                getValue: () => Config.MaxTrees,
                setValue: value => Config.MaxTrees = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Tree Growth Stage",
                tooltip: () => "Growth stage for trees spawned at the start of day",
                getValue: () => Config.TreeGrowthStage,
                setValue: value => Config.TreeGrowthStage = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Tree % Chance",
                tooltip: () => "Chance per cloud tile per day to spawn a tree",
                getValue: () => Config.TreeChancePercent,
                setValue: value => Config.TreeChancePercent = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Object % Chance",
                tooltip: () => "Chance per empty cloud tile per day to spawn an object (crystal weed or gem resource)",
                getValue: () => Config.ObjectChancePercent,
                setValue: value => Config.ObjectChancePercent = value
            );
        }

    }
}
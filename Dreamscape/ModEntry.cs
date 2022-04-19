using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Globalization;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace Dreamscape
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetEditor
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string mapAssetKey;
        public static Texture2D dirtTexture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            var location = Game1.getLocationFromName("Dreamscape");
            var layer = location.map.GetLayer("Back");
            var mine = new MineShaft();
            mine.mineLevel = 200;
            for (int x = 0; x < layer.LayerWidth; x++)
            {
                for (int y = 0; y < layer.LayerHeight; y++)
                {
                    if (!location.objects.ContainsKey(new Vector2(x, y)) && layer.Tiles[new Location(x, y)]?.TileIndex == 26)
                    {
                        if(Game1.random.NextDouble() < 0.05)
                        {
                            if(Game1.random.NextDouble() < 0.5)
                            {
                                location.objects.Add(new Vector2(x, y), new Object(new Vector2(x, y), 319 + Game1.random.Next(3), "Weeds", true, false, false, false)
                                {
                                    Fragility = 2,
                                    CanBeGrabbed = true,
                                });
                            }
                            else
                            {
                                location.objects.Add(new Vector2(x, y), (Object)AccessTools.Method(typeof(MineShaft), "chooseStoneType").Invoke(mine, new object[] { 0.1, 0.05, 0.25, new Vector2(x, y) }));
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
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;
            if (asset.AssetNameEquals("Data/Locations"))
                return true;

            return false;
        }
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/Locations"))
            {
                var editor = asset.AsDictionary<string, string>();
                editor.Data["Dreamscape"] = "372 .9 718 .1 719 .3 723 .3/372 .9 394 .5 718 .1 719 .3 723 .3/372 .9 718 .1 719 .3 723 .3/372 .4 392 .8 718 .05 719 .2 723 .2/129 -1 131 -1 147 -1 148 -1 152 -1 708 -1 267 -1/128 -1 130 -1 146 -1 149 -1 150 -1 152 -1 155 -1 708 -1 701 -1 267 -1/129 -1 131 -1 148 -1 150 -1 152 -1 154 -1 155 -1 705 -1 701 -1/708 -1 130 -1 131 -1 146 -1 147 -1 150 -1 151 -1 152 -1 154 -1 705 -1/384 .08 589 .09 102 .15 390 .25 330 1";
            }
        }
    }
}
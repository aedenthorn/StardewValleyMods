using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Wildflowers
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string wildKey = "aedenthorn.Wildflowers/wild";
        public static Dictionary<string, Dictionary<Vector2, Crop>> cropDict = new Dictionary<string, Dictionary<Vector2, Crop>>();
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.Saving += GameLoop_Saving;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            if (!Config.ModEnabled || !Game1.IsMasterGame)
                return;
            foreach (var l in cropDict.Keys.ToArray())
            {
                GameLocation location = Game1.getLocationFromName(l);
                if (location is null)
                    continue;
                foreach (var key in cropDict[l].Keys.ToArray())
                {
                    if (!location.terrainFeatures.TryGetValue(key, out TerrainFeature f) || f is not Grass)
                        continue;
                    f.modData[wildKey] = JsonConvert.SerializeObject(new CropData(cropDict[l][key]));
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            cropDict.Clear();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            //MakeHatData();

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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Make Flower Honey",
                getValue: () => Config.WildFlowersMakeFlowerHoney,
                setValue: value => Config.WildFlowersMakeFlowerHoney = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Fix Flower Find",
                getValue: () => Config.FixFlowerFind,
                setValue: value => Config.FixFlowerFind = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bee Range",
                getValue: () => Config.BeeRange,
                setValue: value => Config.BeeRange = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Weapons Harvest Flowers",
                getValue: () => Config.WeaponsHarvestFlowers,
                setValue: value => Config.WeaponsHarvestFlowers = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Wildflower Grow % Chance",
                getValue: () => (Config.wildflowerGrowChance * 100) +"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float fv)) Config.wildflowerGrowChance = fv / 100f; }
            );
        }
    }
}
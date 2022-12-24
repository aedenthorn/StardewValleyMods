using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Spoilage
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static string dictPath = "aedenthorn.Spoilage/dictionary";
        public static string ageKey = "aedenthorn.Spoilage/age";
        public static string spoiledKey = "aedenthorn.Spoilage/spoiled";
        public static Dictionary<string, SpoilData> spoilageDict = new Dictionary<string, SpoilData>();
        public static ModEntry context;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        public override object GetApi()
        {
            if (!Config.ModEnabled)
                return null;
            return new SpoilageAPI();
        }
        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, SpoilData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }
        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            spoilageDict = Helper.GameContent.Load<Dictionary<string, SpoilData>>(dictPath);
            foreach(var kvp in Config.CustomSpoilage)
            {
                spoilageDict.TryAdd(kvp.Key, kvp.Value);
            }
            var locs = new List<GameLocation>();
            foreach(var l in Game1.locations)
            {
                locs.Add(l);
                if(l is BuildableGameLocation)
                {
                    foreach(var b in (l as BuildableGameLocation).buildings)
                    {
                        if(b.indoors.Value is not null)
                        {
                            locs.Add(b.indoors.Value);
                        }
                    }
                }
            }
            
            foreach(var l in locs)
            {
                foreach(var obj in l.objects.Values)
                {
                    if(obj is Chest)
                    {
                        SpoilItems((obj as Chest).items, (obj as Chest).fridge.Value ? Config.FridgeMult : 1);
                    }
                    else if(obj.heldObject.Value is Chest)
                    {
                        SpoilItems((obj.heldObject.Value as Chest).items, (obj.heldObject.Value as Chest).fridge.Value ? Config.FridgeMult : 1);
                    }
                }
            }
            SpoilItems(Game1.player.items, Config.PlayerMult);

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Allow Quality Reduction",
                getValue: () => Config.QualityReduction,
                setValue: value => Config.QualityReduction = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Allow Spoiling",
                getValue: () => Config.Spoiling,
                setValue: value => Config.Spoiling = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Remove Spoiled",
                getValue: () => Config.RemoveSpoiled,
                setValue: value => Config.RemoveSpoiled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Display Age",
                getValue: () => Config.DisplayDays,
                setValue: value => Config.DisplayDays = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Display Days Left",
                getValue: () => Config.DisplayDaysLeft,
                setValue: value => Config.DisplayDaysLeft = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Fruits Days",
                getValue: () => Config.FruitsDays,
                setValue: value => Config.FruitsDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Vegetables Days",
                getValue: () => Config.VegetablesDays,
                setValue: value => Config.VegetablesDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Greens Days",
                getValue: () => Config.GreensDays,
                setValue: value => Config.GreensDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Flowers Days",
                getValue: () => Config.FlowersDays,
                setValue: value => Config.FlowersDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Eggs Days",
                getValue: () => Config.EggDays,
                setValue: value => Config.EggDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Milk Days",
                getValue: () => Config.MilkDays,
                setValue: value => Config.MilkDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Cooked Days",
                getValue: () => Config.CookingDays,
                setValue: value => Config.CookingDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Meat Days",
                getValue: () => Config.MeatDays,
                setValue: value => Config.MeatDays = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Fish Days",
                getValue: () => Config.FishDays,
                setValue: value => Config.FishDays = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Fridge Spoilage Multiplier",
                getValue: () => Config.FridgeMult + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.FridgeMult = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Player Spoilage Multiplier",
                getValue: () => Config.PlayerMult + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.PlayerMult = f; } }
            );
        }
    }
}
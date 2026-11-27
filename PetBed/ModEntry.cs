using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace PetBed
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public const string dictPath = "aedenthorn.PetBed/dict";
        public const string sleepingKey = "aedenthorn.PedBed/sleeping";

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
            helper.Events.Content.AssetRequested += Content_AssetRequested;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if(!Config.ModEnabled)
                return;
            if(e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, PetBedData>(), AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Config.Debug)
            {
                Game1.player.whichPetType = "Cat";
            }
            foreach(Pet pet in Utility.getHomeOfFarmer(Game1.player).characters.ToList().Where(c => c.GetType().IsAssignableTo(typeof(Pet))))
            {
                Monitor.Log($"Checking pet bed for {pet.Name}");
                WarpPetToBed(pet, Utility.getHomeOfFarmer(Game1.player), false);
            }
            foreach(Pet pet in Game1.getFarm().characters.ToList().Where(c => c.GetType().IsAssignableTo(typeof(Pet))))
            {
                Monitor.Log($"Checking pet bed for {pet.Name}");
                WarpPetToBed(pet, Game1.getFarm(), true);
            }
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
                name: () => "Mod Enabled?",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Outdoor Bed?",
                getValue: () => Config.OutdoorIsBed,
                setValue: value => Config.OutdoorIsBed = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Indoor Bed?",
                getValue: () => Config.IndoorIsBed,
                setValue: value => Config.IndoorIsBed = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bed Chance",
                getValue: () => Config.BedChance,
                setValue: value => Config.BedChance = value,
                min: 0,
                max: 100
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Indoor Bed Name",
                tooltip: () => "Name of furniture to use as indoor bed. Or X,Y sleep coordinates.",
                getValue: () => Config.IndoorBedName,
                setValue: value => Config.IndoorBedName = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Outdoor Bed Name",
                tooltip: () => "Name of furniture to use as outdoor bed. Or X,Y sleep coordinates.",
                getValue: () => Config.OutdoorBedName,
                setValue: value => Config.OutdoorBedName = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Indoor Bed Offset X,Y",
                tooltip: () => "In pixels.",
                getValue: () => Config.IndoorBedOffset,
                setValue: value => Config.IndoorBedOffset = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Outdoor Bed Offset X,Y",
                tooltip: () => "In pixels.",
                getValue: () => Config.OutdoorBedOffset,
                setValue: value => Config.OutdoorBedOffset = value
            );
        }
    }

}
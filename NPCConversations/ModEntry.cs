using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile;

namespace NPCConversations
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static readonly string dictPath = "custom_object_production_dictionary";
        public static Dictionary<string, NPCConversationData> npcConversationDataDict = new Dictionary<string, NPCConversationData>();
        public static SpeakingAnimalData speakingAnimals;

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
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            helper.Events.Player.Warped += Player_Warped;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            speakingAnimals = null;
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if(Config.EnableMod && (Game1.currentLocation is Farm || Game1.currentLocation is AnimalHouse || Game1.currentLocation is Forest))
            {
                FarmAnimal[] animals;
                if (Game1.currentLocation is AnimalHouse)
                    animals = (Game1.currentLocation as AnimalHouse).animals.Values.ToArray();
                else if (Game1.currentLocation is Farm)
                    animals = (Game1.currentLocation as Farm).animals.Values.ToArray();
                else if (Game1.currentLocation is Forest)
                    animals = (Game1.currentLocation as Forest).marniesLivestock.ToArray();
                else return;
                foreach (var a in animals)
                {
                    foreach (var b in animals)
                    {
                        if (a.myID.Value == b.myID.Value)
                            continue;
                        if (Vector2.Distance(a.getTileLocation(), b.getTileLocation()) <= 10 && Game1.random.NextDouble() < 0.3)
                        {
                            speakingAnimals = new SpeakingAnimalData(a, b);
                            return;
                        }
                    }
                }
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(dictPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log("Loading dictionary");

            return (T)(object)new Dictionary<string, NPCConversationData>();
        }
    }
}
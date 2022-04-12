using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace AdvancedDialogueDisplay
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        private static string dictPath = "aedenthorn.AdvancedDialogueDisplay/dictionary";
        private static string defaultKey = "aedenthorn.AdvancedDialogueDisplay/default";
        private static Dictionary<string, DialogueDisplayData> dataDict = new Dictionary<string, DialogueDisplayData>();
        private static Dictionary<string, Texture2D> imageDict = new Dictionary<string, Texture2D>();
        private static List<string> loadedPacks = new List<string>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(Config.EnableMod && e.Button == Config.ReloadButton)
            {
                LoadData();
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            Monitor.Log("Loading Data");

            foreach(var pack in loadedPacks)
            {
                Helper.ConsoleCommands.Trigger("patch", new string[] { "reload", pack });
            }
            loadedPacks.Clear();
            dataDict = Helper.Content.Load<Dictionary<string, DialogueDisplayData>>(dictPath, ContentSource.GameContent);
            Monitor.Log($"Loaded data for {dataDict.Count} NPCs");
            if(!dataDict.ContainsKey(defaultKey))
                dataDict[defaultKey] = Helper.Data.ReadJsonFile<DialogueDisplayData>("assets/default.json");
            imageDict.Clear();
            foreach(var key in dataDict.Keys)
            {
                if(dataDict[key].packName != null && !loadedPacks.Contains(dataDict[key].packName))
                {
                    loadedPacks.Add(dataDict[key].packName);
                }
                foreach (var image in dataDict[key].images)
                {
                    if(!imageDict.ContainsKey(image.texturePath))
                        imageDict[image.texturePath] = Helper.Content.Load<Texture2D>(image.texturePath, ContentSource.GameContent);
                }
                if (dataDict[key].portrait.texturePath != null && !imageDict.ContainsKey(dataDict[key].portrait.texturePath))
                    imageDict[dataDict[key].portrait.texturePath] = Helper.Content.Load<Texture2D>(dataDict[key].portrait.texturePath, ContentSource.GameContent);
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

            return (T)(object)new Dictionary<string, DialogueDisplayData>();
        }
    }
}
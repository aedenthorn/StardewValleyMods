using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using Object = StardewValley.Object;

namespace PetHats
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string hatKey = "aedenthorn.PetHats/hat";
        public static string catPath = "aedenthorn.PetHats/cats";
        public static string dogPath = "aedenthorn.PetHats/dogs";
        public static string hatPath = "aedenthorn.PetHats/hats";
        public static Dictionary<int, HatOffsetData> hatOffsetDict = new();
        public static Dictionary<int, FrameOffsetData> catOffsetDict = new();
        public static Dictionary<int, FrameOffsetData> dogOffsetDict = new();
        public static Dictionary<string, Hat> hatDict = new();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(catPath))
            {
                e.LoadFromModFile<Dictionary<int, FrameOffsetData>>("assets/cats.json", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(dogPath))
            {
                e.LoadFromModFile<Dictionary<int, FrameOffsetData>>("assets/dogs.json", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(hatPath))
            {
                e.LoadFromModFile<Dictionary<int, HatOffsetData>>("assets/hats.json", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            if(Config.Debug && e.Button == SButton.G)
            {
                Helper.GameContent.InvalidateCache(catPath);
                Helper.GameContent.InvalidateCache(dogPath);
                Helper.GameContent.InvalidateCache(hatPath);

                ReloadData();
                //File.WriteAllText(Path.Combine(Helper.DirectoryPath, "assets", "cats.json"), JsonConvert.SerializeObject(catOffsetDict, Formatting.Indented));
                //File.WriteAllText(Path.Combine(Helper.DirectoryPath, "assets", "dogs.json"), JsonConvert.SerializeObject(dogOffsetDict, Formatting.Indented));
                //File.WriteAllText(Path.Combine(Helper.DirectoryPath, "assets", "hats.json"), JsonConvert.SerializeObject(hatOffsetDict, Formatting.Indented));
            }
                
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            ReloadData();
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Retrieve Mod Key",
                getValue: () => Config.RetrieveButton,
                setValue: value => Config.RetrieveButton = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Debug",
                getValue: () => Config.Debug,
                setValue: value => Config.Debug = value
            );
        }

    }
}

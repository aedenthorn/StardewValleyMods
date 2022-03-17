using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomSigns
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        private static List<string> loadedContentPacks = new List<string>();
        private static Dictionary<string, CustomSignData> customSignDataDict = new Dictionary<string, CustomSignData>();
        private static Dictionary<string, List<string>> customSignTypeDict = new Dictionary<string, List<string>>();
        private static Dictionary<string, SpriteFont> fontDict = new Dictionary<string, SpriteFont>();
        public static readonly string templateKey = "aedenthorn.CustomSigns/template";
        public static readonly string dictPath = "aedenthorn.CustomSigns/dictionary";

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
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ReloadSignData();
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
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Modifier Key",
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (placedSign != null && Game1.activeClickableMenu != null && Game1.player?.currentLocation?.lastQuestionKey?.Equals("CS_Choose_Template") == true)
            {

                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;

                DialogueBox db = menu as DialogueBox;
                int resp = db.selectedResponse;
                List<Response> resps = db.responses;

                if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null || resps[resp].responseKey == "cancel")
                    return;
                Monitor.Log($"Answered {Game1.player.currentLocation.lastQuestionKey} with {resps[resp].responseKey}");

                placedSign.modData[templateKey] = resps[resp].responseKey;
                placedSign = null;
            }
            else if (Helper.Input.IsDown(Config.ModKey) && e.Button == Config.ResetKey)
            {
                foreach(var pack in loadedContentPacks)
                {
                    Helper.ConsoleCommands.Trigger("patch", new string[] { "reload", pack });
                }
                ReloadSignData();
                Helper.Input.Suppress(Config.ResetKey);
            }
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

            return (T)(object)new Dictionary<string, CustomSignData>();
        }
    }
}
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
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static HashSet<string> loadedContentPacks = new HashSet<string>();
        public static Dictionary<string, CustomSignData> customSignDataDict = new Dictionary<string, CustomSignData>();
        public static Dictionary<string, List<string>> customSignTypeDict = new Dictionary<string, List<string>>();
        public static Dictionary<string, SpriteFont> fontDict = new Dictionary<string, SpriteFont>();
        public static NamingMenu textMenu;
        public static readonly string templateKey = "aedenthorn.CustomSigns/template";
        public static readonly string textKey = "aedenthorn.CustomSigns/text";
        public static readonly string dictPath = "aedenthorn.CustomSigns/dictionary";
        public static readonly char splitChar = '¶';

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

            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
        public int count = 0;

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {

            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, CustomSignData>(), AssetLoadPriority.Exclusive);
            }
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
                Response[] resps = db.responses;

                if (resp < 0 || resps == null || resp >= resps.Length || resps[resp] == null || resps[resp].responseKey == "cancel")
                    return;
                Monitor.Log($"Answered {Game1.player.currentLocation.lastQuestionKey} with {resps[resp].responseKey}");

                placedSign.modData[templateKey] = resps[resp].responseKey;
                placedSign = null;
            }
            else if (placedSign != null && Game1.activeClickableMenu != null && Game1.player?.currentLocation?.lastQuestionKey?.Equals("CS_Edit_Text") == true)
            {

                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;

                DialogueBox db = menu as DialogueBox;
                int resp = db.selectedResponse;
                Response[] resps = db.responses;

                if (resp < 0 || resps == null || resp >= resps.Length || resps[resp] == null || resps[resp].responseKey == "cancel")
                    return;
                if (!placedSign.modData.TryGetValue(templateKey, out string template) || !customSignDataDict.TryGetValue(template, out var data))
                {
                    SMonitor.Log($"Template {template} not found.", LogLevel.Warn);
                    return;
                }

                var respKey = resps[resp].responseKey;
                Monitor.Log($"Answered {Game1.player.currentLocation.lastQuestionKey} with {respKey}");

                var dataKey = textKey + respKey;
                string textString = placedSign.modData.TryGetValue(dataKey, out var str) ? str : "";
                db.closeDialogue();
                Game1.activeClickableMenu = new NamingMenu(delegate (string newText)
                {
                    placedSign.modData[dataKey] = newText;
                    placedSign = null;
                    Game1.exitActiveMenu();
                    Game1.playSound("newArtifact", null);
                }, SHelper.Translation.Get("enter-text"), textString);
            }
            else if (Helper.Input.IsDown(Config.ModKey) && e.Button == Config.ResetKey)
            {
                ReloadSignData();
                Helper.Input.Suppress(Config.ResetKey);
            }
        }

    }
}
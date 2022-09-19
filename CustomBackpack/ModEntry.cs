using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomBackpack
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static IClickableMenu lastMenu;

        public static string dictPath = "aedenthorn.CustomBackpack/dictionary";

        public static Dictionary<int, BackPackData> dataDict = new Dictionary<int, BackPackData>();

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

            helper.Events.Content.AssetRequested += Content_AssetRequested;
            
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            helper.ConsoleCommands.Add("custombackpack", "Manually set backpack slots. Usage: custombackpack <slotnumber>", SetSlots);


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
        public override object GetApi()
        {
            return new CustomBackpackApi();
        }
        private void SetSlots(string arg1, string[] arg2)
        {
            if(arg2.Length == 1 && int.TryParse(arg2[0], out int slots))
            {
                SetPlayerSlots(slots);
            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            LoadDict();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            LoadDict();
        }

        private void LoadDict()
        {
            dataDict = Game1.content.Load<Dictionary<int, BackPackData>>(dictPath);
            foreach (var key in dataDict.Keys.ToArray())
            {
                dataDict[key].texture = Helper.GameContent.Load<Texture2D>(dataDict[key].texturePath);
            }
            SHelper.GameContent.InvalidateCache("String/UI");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<int, BackPackData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("String/UI"))
            {
                e.Edit(EditStrings);
            }
        }

        private void EditStrings(IAssetData obj)
        {
            var editor = obj.AsDictionary<string, string>();
            foreach (var key in dataDict.Keys.ToArray())
            {
                editor.Data[$"Chat_CustomBackpack_{key}"] = string.Format(SHelper.Translation.Get("farmer-bought-x"));
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
                name: () => "Show Arrows?",
                getValue: () => Config.ShowArrows,
                setValue: value => Config.ShowArrows = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Row Numbers?",
                getValue: () => Config.ShowRowNumbers,
                setValue: value => Config.ShowRowNumbers = value
            );
        }
    }
}
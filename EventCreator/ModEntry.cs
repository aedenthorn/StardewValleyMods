using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCreator
{
    public partial class ModEntry : Mod
    {
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string dictPath = "aedenthorn.EventCreator/dict";
        public static Dictionary<string, Dictionary<string, string>> customEventsDict = new();
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;


            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            LoadEventCommands();
        }

        private async void LoadEventCommands()
        {
            await GetCommandsFromWiki();
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (Config.EnableMod && e.Button == Config.MenuKey && Game1.activeClickableMenu is GameMenu)
            {
                Game1.activeClickableMenu.exitThisMenu();
                OpenMenu();
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.StartsWith("Data/Events/") && customEventsDict.TryGetValue(e.NameWithoutLocale.ToString().Substring("Data/Events/".Length), out var customDict) && customDict.Any())
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, string>().Data;
                    foreach(var kvp in customDict)
                    {
                        dict[kvp.Key] = kvp.Value;
                    }
                });
            }
            //{
            //    e.LoadFrom(() => new Dictionary<string, PetCoatData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            //}
            //else if (e.NameWithoutLocale.StartsWith(texturesPrefix))
            //{
            //    e.LoadFrom(() => 
            //    {
            //        var coat = e.NameWithoutLocale.ToString().Substring(texturesPrefix.Length);
            //        var dict = GetDataDict();
            //        if (dict?.TryGetValue(coat, out var data) != true)
            //        {
            //            return null;
            //        }
            //        return data.Texture;
            //    }, StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            //}
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {

                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.EnableMod"),
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );

                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.MenuKey"),
                    getValue: () => Config.MenuKey,
                    setValue: value => Config.MenuKey = value
                );

            }
        }
    }
}

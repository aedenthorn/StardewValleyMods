using HarmonyLib;
using StardewModdingAPI;
using System.Collections.Generic;

namespace LuauSoup
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string dictPath = "aedenthorn.LuauSoup/dict";

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, SoupIngredientData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (Config.WarnMessage && e.NameWithoutLocale.IsEquivalentTo("Strings/StringsFromCSFiles"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, string>();
                    dict.Data["Event.cs.1719"] = SHelper.Translation.Get("select-ingredient");
                });
            }
            else if (Config.WarnMessage && e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/summer11"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, string>();
                    dict.Data["Lewis"] = SHelper.Translation.Get("Lewis");
                });
            }
        }
        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
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

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.EveryoneMustContribute"),
                    tooltip: () => SHelper.Translation.Get("Config.EveryoneMustContribute.Desc"),
                    getValue: () => Config.EveryoneMustContribute,
                    setValue: value => Config.EveryoneMustContribute = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.ShowItemReactions"),
                    getValue: () => Config.ShowItemInfo,
                    setValue: value => Config.ShowItemInfo = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.WarnMessage"),
                    getValue: () => Config.WarnMessage,
                    setValue: value => Config.WarnMessage = value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Config.FriendshipLoved",
                    getValue: () => Config.FriendshipLoved,
                    setValue: value => Config.FriendshipLoved = value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Config.FriendshipLiked",
                    getValue: () => Config.FriendshipLiked,
                    setValue: value => Config.FriendshipLiked = value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Config.FriendshipDisliked",
                    getValue: () => Config.FriendshipDisliked,
                    setValue: value => Config.FriendshipDisliked = value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Config.FriendshipHated",
                    getValue: () => Config.FriendshipHated,
                    setValue: value => Config.FriendshipHated = value
                );
            }

        }
    }
}

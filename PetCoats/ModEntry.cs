using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace PetCoats
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string dictPath = "aedenthorn.PetCoats/dict";
        public static string modKey = "aedenthorn.PetCoats/coat";
        public static string texturesPrefix = "aedenthorn.PetCoats/textures/";
        public static Dictionary<string, PetCoatData> dataDict;

        public static string masterCoat;
        public static string MasterCoat {  
            get
            {
                if(masterCoat == null)
                {
                    Game1.MasterPlayer.modData.TryGetValue(modKey + "_" + Game1.MasterPlayer.whichPetType + "_" + Game1.MasterPlayer.whichPetBreed, out masterCoat);
                }
                return masterCoat;
            }
            set
            {
                Game1.MasterPlayer.modData[modKey + "_" + Game1.MasterPlayer.whichPetType + "_" + Game1.MasterPlayer.whichPetBreed] = value;
                masterCoat = value;
            }
        }
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
                e.LoadFrom(() => new Dictionary<string, PetCoatData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.StartsWith(texturesPrefix))
            {
                e.LoadFrom(() => 
                {
                    var coat = e.NameWithoutLocale.ToString().Substring(texturesPrefix.Length);
                    var dict = GetDataDict();
                    if (dict?.TryGetValue(coat, out var data) != true)
                    {
                        return null;
                    }
                    return data.Texture;
                }, StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
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
                    name: () => SHelper.Translation.Get("Config.ModKey"),
                    getValue: () => Config.ModKey,
                    setValue: value => Config.ModKey = value
                );

                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.CoatKey"),
                    getValue: () => Config.CoatKey,
                    setValue: value => Config.CoatKey = value
                );

                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.BreedKey"),
                    getValue: () => Config.BreedKey,
                    setValue: value => Config.BreedKey = value
                );

                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.TypeKey"),
                    getValue: () => Config.TypeKey,
                    setValue: value => Config.TypeKey = value
                );
            }
        }
    }
}

using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace ChestFullnessTextures
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string dictPath = "aedenthorn.ChestFullnessTextures/dictionary";

        public static Dictionary<string, ChestTextureDataShell> dataDict = new();

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            return;
            var x = new TempJson();
            var y = new ChestTextureDataShell() { Entries = new() };
            for (int i = 24; i >= 1; i--)
            {
                y.Entries.Add(new ChestTextureData()
                {
                    texturePath = $"aedenthorn.ChestFullnessDisplay/Chest_{i}",
                    items = i,
                    tileWidth = 16,
                    tileHeight = 32
                });
            }
            x.Changes.Add(new Temp2Json()
            {
                Action = "EditData",
                Target = "aedenthorn.ChestFullnessTextures/dictionary",
                Entries = new()
                {
                    {"Chest", y }
                }
            });
            for (int i = 24; i >= 1; i--)
            {
                x.Changes.Add(new Temp2Json()
                {
                    Action = "Load",
                    Target = $"aedenthorn.ChestFullnessDisplay/Chest_{i}",
                    FromFile = $"assets/Chest_{i}.png"
                });
            }
            File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "content.json"), JsonConvert.SerializeObject(x, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            dataDict = SHelper.GameContent.Load<Dictionary<string, ChestTextureDataShell>>(dictPath);
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, ChestTextureDataShell>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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
                    name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
            }
        }
    }
}
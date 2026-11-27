using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;

namespace TappablePalmTrees
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        private static IDynamicGameAssetsApi apiDGA;
        private static IJsonAssetsApi apiJA;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Tree), nameof(Tree.UpdateTapperProduct)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Tree_UpdateTapperProduct_Prefix))
            );

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            apiDGA = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            apiJA = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");

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
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Days to fill",
                getValue: () => Config.DaysToFill,
                setValue: value => Config.DaysToFill = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Product Object",
                tooltip: () => "index or name (JA and DGA should work)",
                getValue: () => Config.Product,
                setValue: value => Config.Product = value
            );
        }
        private static Object GetObjectFromID(string id)
        {
            SMonitor.Log($"Trying to get object {id}, DGA {apiDGA != null}, JA {apiJA != null}");

            Object obj = null;
            try
            {

                if (int.TryParse(id, out int index))
                {
                    return new Object(index, 1, false, -1, 0);
                }
                else
                {
                    var dict = SHelper.Content.Load<Dictionary<int, string>>("Data/ObjectInformation", ContentSource.GameContent);
                    foreach(var kvp in dict)
                    {
                        if (kvp.Value.StartsWith(id+"/"))
                            return new Object(kvp.Key, 1, false, -1, 0);
                    }
                }
                if (apiDGA != null && id.Contains("/"))
                {
                    object o = apiDGA.SpawnDGAItem(id);
                    if (o is Object)
                    {
                        return obj;
                    }
                }
                if (apiJA != null)
                {
                    int idx = apiJA.GetObjectId(id);
                    if (idx != -1)
                    {
                        return new Object(idx, 1, false, -1, 0);

                    }
                }
            }
            catch
            {
            }
            SMonitor.Log($"Couldn't find item with id {id}");
            return obj;
        }
    }

}
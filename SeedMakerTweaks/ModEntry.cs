using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SeedMakerTweaks
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
        private Harmony harmony;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.performObjectDropInAction)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_performObjectDropInAction_Transpiler))
            );

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            apiDGA = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            apiJA = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            
            var obj = Helper.ModRegistry.GetApi("Pathoschild.Automate");
            if (obj != null)
            {
                SMonitor.Log($"patching automate");

                harmony.Patch(
                   original: AccessTools.Method(obj.GetType().Assembly.GetType("Pathoschild.Stardew.Automate.Framework.Machines.Objects.SeedMakerMachine"), "SetInput"),
                   transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SeedMakerMachine_SetInput_Transpiler))
                );
            }

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
                name: () => "Min Seeds",
                getValue: () => Config.MinSeeds,
                setValue: value => Config.MinSeeds = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Seeds",
                getValue: () => Config.MaxSeeds,
                setValue: value => Config.MaxSeeds = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Mixed Seeds",
                getValue: () => Config.MinMixedSeeds,
                setValue: value => Config.MinMixedSeeds = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Seeds",
                getValue: () => Config.MaxMixedSeeds,
                setValue: value => Config.MaxMixedSeeds = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Ancient Seed % Chance",
                getValue: () => "" + Config.AncientSeedChance,
                setValue: delegate (string value) { try { Config.AncientSeedChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Mixed Seed % Chance",
                getValue: () => "" + Config.MixedSeedChance,
                setValue: delegate (string value) { try { Config.MixedSeedChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
        }

        public static int GetMinMixedSeeds()
        {
            return Config.MinMixedSeeds;
        }
        public static int GetMaxMixedSeeds()
        {
            return Config.MaxMixedSeeds;
        }
        public static int GetMinSeeds()
        {
            return Config.MinSeeds;
        }
        public static int GetMaxSeeds()
        {
            return Config.MaxSeeds;
        }
        public static float GetAncientSeedChance()
        {
            return Config.AncientSeedChance / 100f;
        }
        public static float GetMixedSeedChance()
        {
            return Config.MixedSeedChance / 100f;
        }
    }
}
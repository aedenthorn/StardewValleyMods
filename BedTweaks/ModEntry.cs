using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace BedTweaks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);

            // Object patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Object_draw_Prefix))
            );

            // Furniture patches

            harmony.Patch(
               original: AccessTools.Method(typeof(BedFurniture), nameof(BedFurniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(BedFurniture_draw_Prefix))
            );
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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Redraw Middle Pillows",
                getValue: () => Config.RedrawMiddlePillows,
                setValue: value => Config.RedrawMiddlePillows = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bed Width",
                getValue: () => Config.BedWidth,
                setValue: value => Config.BedWidth = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Sheet Opacity %",
                getValue: () => (int)(Config.SheetTransparency * 100),
                setValue: value => Config.SheetTransparency = value / 100f,
                min: 1,
                max: 100
            );
        }

        public override object GetApi()
        {
            return new BedTweaksAPI();
        }
    }
}
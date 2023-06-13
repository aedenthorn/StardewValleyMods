using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace WateringCanTweaks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        private static Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();

        public static ModEntry context;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Tool), nameof(Tool.draw), new Type[] {typeof(SpriteBatch) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Tool_draw_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(WateringCan), nameof(WateringCan.DoFunction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.WateringCan_DoFunction_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.WateringCan_DoFunction_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.toolPowerIncrease)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_toolPowerIncrease_Prefix))
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Water Adjacent",
                tooltip: () => "When charging, fill adjacent tiles instead of creating rectangle area",
                getValue: () => Config.FillAdjacent,
                setValue: value => Config.FillAdjacent = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Volume Mult",
                tooltip: () => "Multiply watering can volume by this amount",
                getValue: () => "" + Config.VolumeMult,
                setValue: delegate (string value) { try { Config.VolumeMult = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Water Mult",
                tooltip: () => "Multiply number of tiles to water by this amount.",
                getValue: () => "" + Config.WaterMult,
                setValue: delegate (string value) { try { Config.WaterMult = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Charged Stamina Mult",
                tooltip: () => "Multiply stamina used for charged watering by this amount (only if Water Adjacent is enabled)",
                getValue: () => "" + Config.ChargedStaminaMult,
                setValue: delegate (string value) { try { Config.ChargedStaminaMult = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
        }
    }
}

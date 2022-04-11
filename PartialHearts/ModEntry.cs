using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;

namespace PartialHearts
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        private static Texture2D heartTexture;
        private static Harmony harmony;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            heartTexture = Helper.Content.Load<Texture2D>("assets/heart.png");

            harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.IsLoaded("CJBok.CheatsMenu"))
            {
                try
                {
                    Monitor.Log($"patching CJBok.CheatsMenu");
                    harmony.Patch(
                       original: AccessTools.Method(Type.GetType("CJBCheatsMenu.Framework.Components.CheatsOptionsNpcSlider, CJBCheatsMenu"), "draw"),
                       postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CheatsOptionsNpcSlider_draw_Postfix))
                    );
                }
                catch
                {
                    Monitor.Log($"Failed patching CJBok.CheatsMenu", LogLevel.Debug);
                }
            }

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
                    name: () => "Mod Enabled?",
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
                
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Granular?",
                    getValue: () => Config.Granular,
                    setValue: value => Config.Granular = value
                );

            }
        }
    }
}
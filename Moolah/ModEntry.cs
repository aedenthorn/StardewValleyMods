using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Numerics;
using Object = StardewValley.Object;

namespace Moolah
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            //helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            //helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            previousTargetValue = 0;
            currentValue = 0;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Config.EnableMod && e.Button == SButton.H && Context.IsPlayerFree)
            {
                if(Game1.player.Money < maxValue)
                    Game1.player.Money = 854775807;
                Game1.player.addUnearnedMoney((int)Math.Round(maxValue * 10 * Game1.random.NextDouble()));
            }
        }


        private static BigInteger GetTotalMoolah()
        {
            BigInteger moocha = Game1.player.Money;
            if (Game1.player.modData.TryGetValue("aedenthorn.Moolah/moocha", out string moochaString))
                moocha += BigInteger.Parse(moochaString);
            return moocha;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
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
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Separator",
                getValue: () => Config.Separator,
                setValue: value => Config.Separator = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Separator X",
                getValue: () => Config.SeparatorX,
                setValue: value => Config.SeparatorX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Separator Y",
                getValue: () => Config.SeparatorY,
                setValue: value => Config.SeparatorY = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Separator Interval",
                getValue: () => Config.SeparatorInterval,
                setValue: value => Config.SeparatorInterval = value
            );
        }
    }
}
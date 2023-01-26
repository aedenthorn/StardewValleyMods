using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Moolah
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static int maxValue = 1000000000;
        public static string moochaKey = "aedenthorn.Moolah/moocha";
        //public static string earnedKey = "aedenthorn.Moolah/earned";

        public static PerScreen<MoneyDialData> moneyDialData = new();
        public static PerScreen<List<MoneyDialData>> moneyDialDataList = new();

        public static PerScreen<Item[]> shippingBin = new();
        public static PerScreen<List<BigInteger>> categoryTotals = new();
        public static PerScreen<Dictionary<Item, BigInteger>> itemValues = new();
        public static PerScreen<Dictionary<Item, BigInteger>> singleItemValues = new();


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
            if (Config.Debug)
            {
                helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            }
            //helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            shippingBin.Value = new Item[0];
            moneyDialData.Value = new();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Config.EnableMod && e.Button == SButton.H && Context.IsPlayerFree)
            {

                var i = new BigInteger(maxValue * Game1.random.NextDouble() * 1000);
                BigInteger total = GetTotalMoolah(Game1.player) + i;
                Game1.player._money = AdjustMoney(Game1.player, total);
                //Game1.player.addUnearnedMoney(i);
            }
        }

        public override object GetApi()
        {
            return new MoolahAPI();
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            moneyDialData.Value = new();
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
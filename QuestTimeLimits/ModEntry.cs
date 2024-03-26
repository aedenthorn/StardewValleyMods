using HarmonyLib;
using StardewModdingAPI;
using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;
using StardewValley;
using System.Collections.Generic;
using System;
using System.Globalization;
using StardewModdingAPI.Events;
using StardewValley.Quests;

namespace QuestTimeLimits
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Daily Quest Mult",
                getValue: () => Config.DailyQuestMult+"",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)){Config.DailyQuestMult = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Special Order Mult",
                getValue: () => Config.SpecialOrderMult+ "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { Config.SpecialOrderMult = f; } }
            );
        }


        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            SMonitor.Log("Entering after fade function");
            if (!Config.ModEnabled || Config.SpecialOrderMult > 0)
                return;
            if (Game1.IsMasterGame)
            {
                Dictionary<string, SpecialOrderData> order_data = Game1.content.Load<Dictionary<string, SpecialOrderData>>("Data\\SpecialOrders");
                var orders = new List<SpecialOrder>(Game1.player.team.specialOrders);
                orders.AddRange(Game1.player.team.availableSpecialOrders);
                for (int m = 0; m < orders.Count; m++)
                {
                    SpecialOrder order = orders[m];
                    if (order_data.TryGetValue(order.questKey.Value, out SpecialOrderData data))
                    {
                        int days = getDaysFromQuestDuration(data.Duration);
                        order.dueDate.Value = Game1.Date.TotalDays + days;
                        SMonitor.Log($"Set special order {data.Name} quest days left to {order.dueDate.Value - Game1.Date.TotalDays}");
                    }
                }
            }
        }
        public static int getDaysFromQuestDuration(QuestDuration duration)
        {
            switch (duration)
            {
                case QuestDuration.OneDay:
                    return 1;
                case QuestDuration.TwoDays:
                    return 2;
                case QuestDuration.ThreeDays:
                    return 3;
                case QuestDuration.Week:
                    return 7;
                case QuestDuration.TwoWeeks:
                    return 14;
                case QuestDuration.Month:
                    return 28;
                default: // Should literally never be hit; the enum only has the above cases.
                    SMonitor.Log($"Unknown quest duration: {duration}! Assiming 7 days.", LogLevel.Error);
                    return 7; 
            }
        }

    }
}
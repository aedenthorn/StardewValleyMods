using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TreasureChestsExpanded
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        private static ModConfig Config;
        private static Random myRand;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        public static List<object> treasuresList = new List<object>();
        public static IAdvancedLootFrameworkApi advancedLootFrameworkApi = null;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;

            myRand = new Random();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "addLevelChests"),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(MineShaft_addLevelChests_postfix))
            );
        }
        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            advancedLootFrameworkApi = context.Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
            if (advancedLootFrameworkApi != null)
            {
                Monitor.Log($"loaded AdvancedLootFramework API", LogLevel.Debug);
            }
            treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.MinItemValue, Config.MaxItemValue);
            Monitor.Log($"Got {treasuresList.Count} possible treasures");
        }

        private static void MineShaft_addLevelChests_postfix(MineShaft __instance)
        {
            if (__instance.mineLevel < 121)
                return;

            Vector2 chestSpot = new Vector2(9f, 9f);

            NetBool treasureRoom = SHelper.Reflection.GetField<NetBool>(__instance, "netIsTreasureRoom").GetValue();

            if (treasureRoom.Value && __instance.overlayObjects.ContainsKey(chestSpot))
            {

                __instance.overlayObjects[chestSpot] = advancedLootFrameworkApi.MakeChest(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, __instance.mineLevel, Config.IncreaseRate, Config.ItemsBaseMaxValue, Config.CoinBaseMin, Config.CoinBaseMax, chestSpot);
            }
        }

    }
}

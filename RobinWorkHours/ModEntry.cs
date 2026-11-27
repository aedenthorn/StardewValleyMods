using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace RobinWorkHours
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static bool startedWalking;
        public static bool notBuildingToday;

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

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            Harmony harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingWithWarp)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_isCollidingWithWarp_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "updateConstructionAnimation"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_updateConstructionAnimation_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Farm), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farm_resetLocalState_Postfix))
            );
        }
        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            notBuildingToday = !Config.EnableMod || !Game1.IsMasterGame || Utility.isFestivalDay() || (!IsThereABuldingUnderConstruction(Game1.getFarm()) && Game1.player.daysUntilHouseUpgrade.Value <= 0 && (Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value <= 0);

            startedWalking = false;
            if (!Config.EnableMod || Utility.isFestivalDay())
                return;
            var robin = Game1.getCharacterFromName("Robin");
            if (robin is null)
            {
                Monitor.Log($"Couldn't find Robin", LogLevel.Warn);
                return;
            }
            robin.shouldPlayRobinHammerAnimation.Value = false;
            robin.ignoreScheduleToday = false;
            robin.resetCurrentDialogue();
            robin.reloadDefaultLocation();
            Game1.warpCharacter(robin, robin.DefaultMap, robin.DefaultPosition / 64f);
            Farm farm = Game1.getFarm();
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (notBuildingToday)
                return;
            if (!Config.EnableMod || !Game1.IsMasterGame || Utility.isFestivalDay() || (!IsThereABuldingUnderConstruction(Game1.getFarm()) && Game1.player.daysUntilHouseUpgrade.Value <= 0 && (Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value <= 0))
                return;
            var robin = Game1.getCharacterFromName("Robin");
            if (robin is null)
            {
                Monitor.Log($"Couldn't find Robin", LogLevel.Warn);
                return;
            }

            string dest;
            int destX, destY;
            int travelTime;
            if (IsThereABuldingUnderConstruction(Game1.getFarm()) || Game1.player.daysUntilHouseUpgrade.Value > 0)
            {
                dest = "BusStop";
                travelTime = Config.FarmTravelTime;
                destX = 9;
                destY = 23;
            }
            else if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
            {
                dest = "BusStop";
                travelTime = Config.BackwoodsTravelTime;
                destX = 11;
                destY = 10;
            }
            else
            {
                dest = "Town";
                travelTime = Config.TownTravelTime;
                destX = 72;
                destY = 69;
            }
            travelTime = Utility.ModifyTime(Config.StartTime, -travelTime);

            if (!startedWalking && e.NewTime >= travelTime && e.NewTime < Config.EndTime && !robin.shouldPlayRobinHammerAnimation.Value) // walk to destination
            {
                startedWalking = true;
                if (robin.currentLocation.Name == dest && robin.Tile.X == destX && robin.Tile.Y == destY)
                {
                    Monitor.Log($"Robin is starting work in {dest} at {e.NewTime}", LogLevel.Debug);
                    AccessTools.Method(typeof(NPC), "updateConstructionAnimation").Invoke(robin, new object[0]);
                    return;
                }
                Monitor.Log($"Robin is walking to work in {dest} at {e.NewTime}", LogLevel.Trace);
                robin.ignoreScheduleToday = false;
                robin.reloadSprite();
                robin.lastAttemptedSchedule = -1;
                robin.temporaryController = null;
                var sched = new Dictionary<int, SchedulePathDescription>() { { Game1.timeOfDay, robin.pathfindToNextScheduleLocation(robin.ScheduleKey, robin.currentLocation.Name, (int)robin.Tile.X, (int)robin.Tile.Y, dest, destX, destY, 3, null, null) } };
                robin.TryLoadSchedule(robin.ScheduleKey, sched);
                robin.checkSchedule(Game1.timeOfDay);
            }
            else if (e.NewTime >= Config.EndTime && IsRobinAtPlayerFarm())
            {
                Monitor.Log($"Robin is ending work at {e.NewTime}", LogLevel.Debug);

                if (Config.FinishNightBefore)
                {
                    using (List<Building>.Enumerator enumerator = Game1.getFarm().buildings.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.isUnderConstruction(false) && (enumerator.Current.daysOfConstructionLeft.Value == 1 || enumerator.Current.daysUntilUpgrade.Value == 1))
                            {
                                Monitor.Log($"Finishing construction of {enumerator.Current.buildingType.Value}", LogLevel.Debug);
                                enumerator.Current.FinishConstruction();
                            }
                        }
                    }
                    foreach(var f in Game1.getAllFarmers())
                    {
                        if (f.daysUntilHouseUpgrade.Value == 1)
                        {
                            Monitor.Log($"Finishing upgrade of farm house for {f.Name}", LogLevel.Debug);
                            FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(f);
                            homeOfFarmer.moveObjectsForHouseUpgrade(f.HouseUpgradeLevel + 1);
                            f.HouseUpgradeLevel++;
                            f.daysUntilHouseUpgrade.Value = -1;
                            homeOfFarmer.setMapForUpgradeLevel(f.HouseUpgradeLevel);
                            Game1.stats.checkForBuildingUpgradeAchievements();
                            f.autoGenerateActiveDialogueEvent("houseUpgrade_" + f.HouseUpgradeLevel.ToString(), 4);
                        }
                    }
                }

                robin.shouldPlayRobinHammerAnimation.Value = false;
                robin.ignoreScheduleToday = false;
                robin.resetCurrentDialogue();
                Game1.warpCharacter(robin, "BusStop", new Vector2(10, 23));
                Game1.getFarm().removeTemporarySpritesWithIDLocal(16846);

                robin.reloadSprite();
                robin.temporaryController = null;

                string scheduleString = GetTodayScheduleString(robin);
                if (scheduleString is null)
                {
                    scheduleString = "800 ScienceHouse 8 18 2/1700 Mountain 29 36 2/1930 ScienceHouse 16 5 0/2100 ScienceHouse 21 4 1 robin_sleep";
                }
                var schedule = new Dictionary<int, SchedulePathDescription>();
                var schedulesStrings = scheduleString.Split('/');
                int startIndex = 0;
                for (int i = schedulesStrings.Length - 1; i >= 0; i--)
                {
                    string[] parts = schedulesStrings[i].Split(' ');
                    if (!int.TryParse(parts[0], out int time) || !int.TryParse(parts[2], out int x) || !int.TryParse(parts[3], out int y))
                        continue;
                    int facing = 0;
                    int.TryParse(parts[4], out facing);
                    string animation = parts.Length > 5 ? parts[5] : null;
                    string message = parts.Length > 6 ? parts[6] : null;
                    if (Game1.timeOfDay > travelTime)
                    {
                        Monitor.Log($"Adding starting appointment at {Game1.timeOfDay}: {schedulesStrings[i]}", LogLevel.Trace);
                        schedule.Add(Game1.timeOfDay, robin.pathfindToNextScheduleLocation(robin.ScheduleKey, "BusStop", 10, 23, parts[1], x, y, facing, animation, message));
                        startIndex = i + 1;
                        break;
                    }
                }
                if (startIndex < schedulesStrings.Length)
                {
                    string lastLoc = null;
                    int lastX = -1;
                    int lastY = -1;
                    for (int i = startIndex; i < schedulesStrings.Length; i++)
                    {
                        string[] parts = schedulesStrings[i].Split(' ');
                        if (!int.TryParse(parts[0], out int time) || !int.TryParse(parts[2], out int x) || !int.TryParse(parts[3], out int y))
                            continue;
                        int facing = 0;
                        int.TryParse(parts[4], out facing);
                        string animation = parts.Length > 5 ? parts[5] : null;
                        string message = parts.Length > 6 ? parts[6] : null;
                        if (schedule.Count == 0)
                        {
                            Monitor.Log($"Adding starting appointment at {Game1.timeOfDay}: {schedulesStrings[i]}", LogLevel.Trace);
                            schedule.Add(Game1.timeOfDay, robin.pathfindToNextScheduleLocation(robin.ScheduleKey, "BusStop", 10, 23, parts[1], x, y, facing, animation, message));
                            break;
                        }
                        else
                        {
                            Monitor.Log($"Adding later appointment at {time}: {schedulesStrings[i]}", LogLevel.Trace);
                            schedule.Add(time, robin.pathfindToNextScheduleLocation(robin.ScheduleKey, lastLoc, lastX, lastY, parts[1], x, y, facing, animation, message));
                        }
                        lastLoc = parts[1];
                        lastX = x;
                        lastY = y;
                    }
                }
                robin.TryLoadSchedule(robin.ScheduleKey, schedule);
                robin.checkSchedule(Game1.timeOfDay);
            }
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
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Start Time",
                tooltip: () => "Use 24h #### format",
                getValue: () => Config.StartTime+"",
                setValue: delegate(string value) { if(int.TryParse(value, out int result) && result % 100 < 60) Config.StartTime = result; }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "End Time",
                tooltip: () => "Use 24h #### format",
                getValue: () => Config.EndTime+"",
                setValue: delegate (string value) { if (int.TryParse(value, out int result) && result % 100 < 60) Config.EndTime = result; }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Farm Travel Time",
                tooltip: () => "Number of minutes to travel to the farm and back",
                getValue: () => Config.FarmTravelTime+"",
                setValue: delegate (string value) { if (int.TryParse(value, out int result)) Config.FarmTravelTime = result; }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Backwoods Travel Time",
                tooltip: () => "Number of minutes to travel to the backwoods and back",
                getValue: () => Config.BackwoodsTravelTime+"",
                setValue: delegate (string value) { if (int.TryParse(value, out int result)) Config.BackwoodsTravelTime = result; }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Town Travel Time",
                tooltip: () => "Number of minutes to travel to the town and back",
                getValue: () => Config.TownTravelTime+"",
                setValue: delegate (string value) { if (int.TryParse(value, out int result)) Config.TownTravelTime = result; }
            );
        }
    }
}
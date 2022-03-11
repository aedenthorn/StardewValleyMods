using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
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
            startedWalking = false;
            if (!Config.EnableMod || Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
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
            if (!Config.EnableMod || !Game1.IsMasterGame || Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) || (!Game1.getFarm().isThereABuildingUnderConstruction() && Game1.player.daysUntilHouseUpgrade.Value <= 0 && (Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value <= 0))
                return;
            var robin = Game1.getCharacterFromName("Robin");
            if(robin is null)
            {
                Monitor.Log($"Couldn't find Robin", LogLevel.Warn);
                return;
            }

            string dest;
            int destX, destY;
            int travelTime;
            if(Game1.getFarm().isThereABuildingUnderConstruction() || Game1.player.daysUntilHouseUpgrade.Value > 0)
            {
                dest = "BusStop";
                travelTime = Config.FarmTravelTime;
                destX = -1;
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
                if (robin.currentLocation.Name == dest && robin.getTileX() == destX && robin.getTileY() == destY)
                {
                    Monitor.Log($"Robin is starting work in {dest} at {e.NewTime}", LogLevel.Debug);
                    AccessTools.Method(typeof(NPC), "updateConstructionAnimation").Invoke(robin, new object[0]);
                    return;
                }
                Monitor.Log($"Robin is walking to work in {dest} at {e.NewTime}");
                robin.ignoreScheduleToday = false;
                robin.reloadSprite();
                robin.lastAttemptedSchedule = -1;
                robin.temporaryController = null;
                robin.Schedule = new Dictionary<int, SchedulePathDescription>() { { Game1.timeOfDay, (SchedulePathDescription)AccessTools.Method(typeof(NPC), "pathfindToNextScheduleLocation").Invoke(robin, new object[] { robin.currentLocation.Name, robin.getTileX(), robin.getTileY(), dest, destX, destY, 3, null, null }) } }; 
                robin.checkSchedule(Game1.timeOfDay);
            }
            else if(e.NewTime >= Config.EndTime && robin.currentLocation == Game1.getFarm())
            {
                Monitor.Log($"Robin is ending work at {e.NewTime}", LogLevel.Debug);
                robin.shouldPlayRobinHammerAnimation.Value = false;
                robin.ignoreScheduleToday = false;
                robin.resetCurrentDialogue();
                Game1.warpCharacter(robin, "BusStop", new Vector2(0, 23));
                Game1.getFarm().removeTemporarySpritesWithIDLocal(16846f);

                robin.reloadSprite();
                robin.temporaryController = null;

                string scheduleString = GetTodayScheduleString(robin);
                if(scheduleString is null)
                {
                    scheduleString = "800 ScienceHouse 8 18 2/1700 Mountain 29 36 2/1930 ScienceHouse 16 5 0/2100 ScienceHouse 21 4 1 robin_sleep";
                }
                var schedule = new Dictionary<int, SchedulePathDescription>();
                var schedulesStrings = scheduleString.Split('/');
                int startIndex = 0;
                for(int i = schedulesStrings.Length - 1; i >= 0; i--)
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
                        Monitor.Log($"Adding starting appointment at {Game1.timeOfDay}: {schedulesStrings[i]}");
                        schedule.Add(Game1.timeOfDay, (SchedulePathDescription)AccessTools.Method(typeof(NPC), "pathfindToNextScheduleLocation").Invoke(robin, new object[] { "BusStop", 0, 23, parts[1], x, y, facing, animation, message }));
                        startIndex = i + 1;
                        break;
                    }
                }
                if(startIndex < schedulesStrings.Length)
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
                            Monitor.Log($"Adding starting appointment at {Game1.timeOfDay}: {schedulesStrings[i]}");
                            schedule.Add(Game1.timeOfDay, (SchedulePathDescription)AccessTools.Method(typeof(NPC), "pathfindToNextScheduleLocation").Invoke(robin, new object[] { "BusStop", 0, 23, parts[1], x, y, facing, animation, message }));
                            break;
                        }
                        else
                        {
                            Monitor.Log($"Adding later appointment at {time}: {schedulesStrings[i]}");
                            schedule.Add(time, (SchedulePathDescription)AccessTools.Method(typeof(NPC), "pathfindToNextScheduleLocation").Invoke(robin, new object[] { lastLoc, lastX, lastY, parts[1], x, y, facing, animation, message }));
                        }
                        lastLoc = parts[1];
                        lastX = x;
                        lastY = y;
                    }
                }
                robin.Schedule = schedule;
                robin.checkSchedule(Game1.timeOfDay);
            }
        }

        private string GetTodayScheduleString(NPC robin)
        {
            if (robin.isMarried())
            {
                if (robin.hasMasterScheduleEntry("marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth))
                {
                    return robin.getMasterScheduleEntry("marriage_" + Game1.currentSeason + "_" + Game1.dayOfMonth);
                }
                string day = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
                if (!Game1.isRaining && robin.hasMasterScheduleEntry("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
                {
                    return robin.getMasterScheduleEntry("marriage_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth));
                }
            }
            else
            {
                if (robin.hasMasterScheduleEntry(Game1.currentSeason + "_" + Game1.dayOfMonth))
                {
                    return robin.getMasterScheduleEntry(Game1.currentSeason + "_" + Game1.dayOfMonth);
                }
                int friendship = Utility.GetAllPlayerFriendshipLevel(robin);
                if (friendship >= 0)
                {
                    friendship /= 250;
                }
                while (friendship > 0)
                {
                    if (robin.hasMasterScheduleEntry(Game1.dayOfMonth.ToString() + "_" + friendship))
                    {
                        return robin.getMasterScheduleEntry(Game1.dayOfMonth.ToString() + "_" + friendship);
                    }
                    friendship--;
                }
                if (robin.hasMasterScheduleEntry(Game1.dayOfMonth.ToString()))
                {
                    return robin.getMasterScheduleEntry(Game1.dayOfMonth.ToString());
                }
                if (Game1.IsRainingHere(Game1.getLocationFromName(robin.DefaultMap)))
                {
                    if (Game1.random.NextDouble() < 0.5 && robin.hasMasterScheduleEntry("rain2"))
                    {
                        return robin.getMasterScheduleEntry("rain2");
                    }
                    if (robin.hasMasterScheduleEntry("rain"))
                    {
                        return robin.getMasterScheduleEntry("rain");
                    }
                }
                List<string> key = new List<string>
                {
                    Game1.currentSeason,
                    Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)
                };
                friendship = Utility.GetAllPlayerFriendshipLevel(robin);
                if (friendship >= 0)
                {
                    friendship /= 250;
                }
                while (friendship > 0)
                {
                    key.Add(friendship.ToString());
                    if (robin.hasMasterScheduleEntry(string.Join("_", key)))
                    {
                        return robin.getMasterScheduleEntry(string.Join("_", key));
                    }
                    friendship--;
                    key.RemoveAt(key.Count - 1);
                }
                if (robin.hasMasterScheduleEntry(string.Join("_", key)))
                {
                    return robin.getMasterScheduleEntry(string.Join("_", key));
                }
                if (robin.hasMasterScheduleEntry(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
                {
                    return robin.getMasterScheduleEntry(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth));
                }
                if (robin.hasMasterScheduleEntry(Game1.currentSeason))
                {
                    return robin.getMasterScheduleEntry(Game1.currentSeason);
                }
                if (robin.hasMasterScheduleEntry("spring_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth)))
                {
                    return robin.getMasterScheduleEntry("spring_" + Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth));
                }
                key.RemoveAt(key.Count - 1);
                key.Add("spring");
                friendship = Utility.GetAllPlayerFriendshipLevel(robin);
                if (friendship >= 0)
                {
                    friendship /= 250;
                }
                while (friendship > 0)
                {
                    key.Add(string.Empty + friendship.ToString());
                    if (robin.hasMasterScheduleEntry(string.Join("_", key)))
                    {
                        return robin.getMasterScheduleEntry(string.Join("_", key));
                    }
                    friendship--;
                    key.RemoveAt(key.Count - 1);
                }
                if (robin.hasMasterScheduleEntry("spring"))
                {
                    return robin.getMasterScheduleEntry("spring");
                }
            }
            return null;
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
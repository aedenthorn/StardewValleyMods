using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace WeddingTweaks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static Random myRand;
        public static ModConfig Config;
        
        public static IFreeLoveAPI freeLoveAPI;

        public static List<string> npcWitnessAsked = new List<string>();
        public static string dictionaryKey = "aedenthorn.WeddingTweaks/dictionary";
        public static string witnessKey = "aedenthorn.WeddingTweaks/witnessName";
        public static Dictionary<string, WeddingData> npcWeddingDict = new Dictionary<string, WeddingData>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = helper;

            myRand = new Random();

            FarmerPatches.Initialize(Monitor, Config, helper);
            Game1Patches.Initialize(Monitor, Config, helper);

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            Helper.ConsoleCommands.Add("Wedding", "Change upcoming wedding. Usage:\n\nWedding  // shows info about upcoming wedding.\nWedding cancel // cancels upcoming wedding.\nWedding set X // sets days until upcoming wedding (replace X with any whole number).", new Action<string, string[]>(WeddingCommand));


            var harmony = new Harmony(ModManifest.UniqueID);

            // Farmer patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getChildren)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_getChildren_Prefix))
            );


            // Game1 patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.prepareSpouseForWedding)),
               prefix: new HarmonyMethod(typeof(Game1Patches), nameof(Game1Patches.prepareSpouseForWedding_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.getCharacterFromName), new Type[] { typeof(string), typeof(bool), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(Game1Patches), nameof(Game1Patches.getCharacterFromName_Prefix))
            );

            harmony.PatchAll();
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            npcWitnessAsked.Clear();
            /*
            var task = new Task(delegate ()
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                IEnumerator new_day_task = StartWeddings();
                while (new_day_task.MoveNext())
                {
                }
            });
            */
        }
        public IEnumerator StartWeddings()
        {
            if (Game1.IsMasterGame)
            {
                Game1.queueWeddingsForToday();
                Game1.newDaySync.sendVar<NetRef<NetLongList>, NetLongList>("weddingsToday", new NetLongList(Game1.weddingsToday));
            }
            else
            {
                while (!Game1.newDaySync.isVarReady("weddingsToday"))
                {
                    yield return 0;
                }
                Game1.weddingsToday = new List<long>(Game1.newDaySync.waitForVar<NetRef<NetLongList>, NetLongList>("weddingsToday"));
            }
            Game1.weddingToday = false;
            foreach (long id in Game1.weddingsToday)
            {
                Farmer spouse_farmer = Game1.getFarmer(id);
                if (spouse_farmer != null && !spouse_farmer.hasCurrentOrPendingRoommate())
                {
                    Game1.weddingToday = true;
                    break;
                }
            }
            if (Game1.player.spouse != null && Game1.player.isEngaged() && Game1.weddingsToday.Contains(Game1.player.UniqueMultiplayerID))
            {
                Friendship friendship = Game1.player.friendshipData[Game1.player.spouse];
                if (friendship.CountdownToWedding <= 1)
                {
                    Game1.prepareSpouseForWedding(Game1.player);
                    friendship.Status = FriendshipStatus.Married;
                    friendship.WeddingDate = new WorldDate(Game1.Date);
                }
            }

            yield break;
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            npcWitnessAsked.Clear();

            npcWeddingDict = Helper.GameContent.Load<Dictionary<string, WeddingData>>(dictionaryKey);
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Strings/StringsFromCSFiles"))
            {
                e.Edit(EditDaysString);

            }
            else if (e.NameWithoutLocale.IsEquivalentTo(dictionaryKey))
            {
                e.LoadFrom(static () => new Dictionary<string, WeddingData>(), AssetLoadPriority.Exclusive);
            }
        }

        private void EditDaysString(IAssetData obj)
        {
            IDictionary<string, string> data = obj.AsDictionary<string, string>().Data;
            data["NPC.cs.3980"] = data["NPC.cs.3980"].Replace("3", Config.DaysUntilMarriage + "");
        }

        private void WeddingCommand(string arg1, string[] arg2)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Game not loaded.", LogLevel.Error);
                return;
            }

            if ((Game1.player.spouse == null || !Game1.player.friendshipData.ContainsKey(Game1.player.spouse) || !Game1.player.friendshipData[Game1.player.spouse].IsEngaged()))
            {
                Monitor.Log("No upcoming wedding.", LogLevel.Alert);
                return;
            }
            string fiancee = Game1.player.spouse;
            if (arg2.Length == 0)
            {
                Monitor.Log($"{Game1.player.Name} is engaged to {fiancee}. The wedding is in {Game1.player.friendshipData[fiancee].CountdownToWedding} days, on {Utility.getDateStringFor(Game1.player.friendshipData[fiancee].WeddingDate.DayOfMonth, Game1.player.friendshipData[fiancee].WeddingDate.SeasonIndex, Game1.player.friendshipData[fiancee].WeddingDate.Year)}.", LogLevel.Info);
            }
            else if(arg2.Length == 1 && arg2[0] == "cancel")
            {
                Game1.player.friendshipData[fiancee].Status = FriendshipStatus.Dating;
                Game1.player.friendshipData[fiancee].WeddingDate = null;
                Game1.player.spouse = null;

                foreach (var f in Game1.player.friendshipData.Pairs)
                {
                    if (f.Value.IsMarried())
                    {
                        Game1.player.spouse = f.Key;
                        break;
                    }
                }
                Monitor.Log($"{Game1.player.Name} engagement to {fiancee} was cancelled.", LogLevel.Info);
            }
            else if(arg2.Length == 2 && arg2[0] == "set")
            {
                if(!int.TryParse(arg2[1], out int days) || days <= 0)
                {
                    Monitor.Log($"Invalid number of days: {arg2[1]}.", LogLevel.Error);
                    return;
                }

                WorldDate weddingDate = new WorldDate(Game1.Date);
                weddingDate.TotalDays += days;
                Game1.player.friendshipData[fiancee].WeddingDate = weddingDate;
                Monitor.Log($"{Game1.player.Name} wedding with {fiancee} was changed to {Game1.player.friendshipData[fiancee].CountdownToWedding} days from now, on {Utility.getDateStringFor(Game1.player.friendshipData[fiancee].WeddingDate.DayOfMonth, Game1.player.friendshipData[fiancee].WeddingDate.SeasonIndex, Game1.player.friendshipData[fiancee].WeddingDate.Year)}.", LogLevel.Info);
            }
        }

        public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            try
            {
                freeLoveAPI = Helper.ModRegistry.GetApi<IFreeLoveAPI>("aedenthorn.FreeLove");
            }
            catch { }

            if (freeLoveAPI != null)
            {
                Monitor.Log("FreeLove API loaded");
            }

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {

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
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Days between proposal and wedding",
                    getValue: () => Config.DaysUntilMarriage,
                    setValue: value => Config.DaysUntilMarriage = Math.Max(1, value),
                    min: 1
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Fix Wedding Event Start",
                    getValue: () => Config.FixWeddingStart,
                    setValue: value => Config.FixWeddingStart = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Other spouses join weddings",
                    getValue: () => Config.AllSpousesJoinWeddings,
                    setValue: value => Config.AllSpousesJoinWeddings = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Other spouses wear marriage clothes",
                    getValue: () => Config.AllSpousesWearMarriageClothesAtWeddings,
                    setValue: value => Config.AllSpousesWearMarriageClothesAtWeddings = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Allow witnesses",
                    getValue: () => Config.AllowWitnesses,
                    setValue: value => Config.AllowWitnesses = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Witness min hearts",
                    getValue: () => Config.WitnessMinHearts,
                    setValue: value => Config.WitnessMinHearts = value,
                    min: 0,
                    max: 14
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Witness Accept Chance",
                    getValue: () => Config.WitnessAcceptPercent,
                    setValue: value => Config.WitnessAcceptPercent = value,
                    min: 0,
                    max: 100
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Heart Affect Percent",
                    getValue: () => Config.WitnessAcceptHeartFactorPercent,
                    setValue: value => Config.WitnessAcceptHeartFactorPercent = value,
                    min: 0,
                    max: 100
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Allow random witnesses",
                    getValue: () => Config.AllowRandomNPCWitnesses,
                    setValue: value => Config.AllowRandomNPCWitnesses = value
                );
            }
        }

    }
}
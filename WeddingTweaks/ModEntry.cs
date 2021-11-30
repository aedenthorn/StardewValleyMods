using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;

namespace WeddingTweaks
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static Random myRand;
        public static ModConfig Config;
        
        public static IFreeLoveAPI freeLoveAPI;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            myRand = new Random();

            FarmerPatches.Initialize(Monitor, Config, helper);
            Game1Patches.Initialize(Monitor, Config, helper);
            EventPatches.Initialize(Monitor, Config, helper);
            NPCPatches.Initialize(Monitor, Config, helper);

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            Helper.ConsoleCommands.Add("Wedding", "Change upcoming wedding. Usage:\n\nWedding  // shows info about upcoming wedding.\nWedding cancel // cancels upcoming wedding.\nWedding set X // sets days until upcoming wedding (replace X with any whole number).", new System.Action<string, string[]>(WeddingCommand));


            var harmony = new Harmony(ModManifest.UniqueID);

            // Farmer patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getChildren)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_getChildren_Prefix))
            );

            // NPC patches

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "engagementResponse"),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Postfix))
            );


            // Event patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), "setUpCharacters"),
               postfix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_setUpCharacters_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.command_loadActors)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_loadActors_Prefix)),
               postfix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_loadActors_Postfix))
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
                    setValue: value => Config.DaysUntilMarriage = value,
                    min: 1
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "All spouses join weddings? (Requires Free Love)",
                    getValue: () => Config.AllSpousesJoinWeddings,
                    setValue: value => Config.AllSpousesJoinWeddings = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "All spouses wear marriage clothes at weddings? (Requires Free Love)",
                    getValue: () => Config.AllSpousesWearMarriageClothesAtWeddings,
                    setValue: value => Config.AllSpousesWearMarriageClothesAtWeddings = value
                );
            }
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            if (asset.AssetNameEquals("Strings/StringsFromCSFiles"))
            {
                return true;
            }
            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            Monitor.Log("Editing asset " + asset.AssetName);
    
            if (asset.AssetNameEquals("Strings/StringsFromCSFiles"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                data["NPC.cs.3980"] = data["NPC.cs.3980"].Replace("3", Config.DaysUntilMarriage+"");
                Monitor.Log($"NPC.cs.3980 is set to \"{data["NPC.cs.3980"]}\"");
            }
        }
    }
}
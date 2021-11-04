using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FreeLove
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModConfig config;
        
        public static Multiplayer mp;
        public static Random myRand;
        public static string farmHelperSpouse = null;
        internal static NPC tempOfficialSpouse;
        public static int bedSleepOffset = 76;

        public static string spouseToDivorce = null;
        public static int divorceHeartsLost;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            myRand = new Random();

            helper.Events.GameLoop.GameLaunched += HelperEvents.GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += HelperEvents.GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += HelperEvents.GameLoop_DayStarted;
            helper.Events.GameLoop.OneSecondUpdateTicked += HelperEvents.GameLoop_OneSecondUpdateTicked;

            PathFindControllerPatches.Initialize(Monitor, config, helper);
            Integrations.Initialize(Monitor, config, helper);
            Divorce.Initialize(Monitor, config, helper);
            NPCPatches.Initialize(Monitor, config, helper);
            LocationPatches.Initialize(Monitor, config, helper);
            FarmerPatches.Initialize(Monitor, config, helper);
            UIPatches.Initialize(Monitor, config, helper);
            EventPatches.Initialize(Monitor, config, helper);
            HelperEvents.Initialize(Monitor, config, helper);
            Misc.Initialize(Monitor, config, helper);

            var harmony = new Harmony(ModManifest.UniqueID);


            // npc patches

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_marriageDuties_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.getSpouse)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_getSpouse_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isRoommate)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isRoommate_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarried)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarried_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarriedOrEngaged)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarriedOrEngaged_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Prefix)),
               transpiler: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "engagementResponse"),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.playSleepingAnimation)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_playSleepingAnimation_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.GetDispositionModifiedString)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_GetDispositionModifiedString_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_GetDispositionModifiedString_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "loadCurrentDialogue"),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_loadCurrentDialogue_Prefix)),
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_loadCurrentDialogue_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToRetrieveDialogue)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToRetrieveDialogue_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_checkAction_Prefix))
            );


            // Child patches

            harmony.Patch(
               original: typeof(Character).GetProperty("displayName").GetMethod,
               postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.Character_displayName_Getter_Postfix))
            );

            // Path patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(bool), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int), typeof(PathFindController.endBehavior) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(PathFindController), new Type[] { typeof(Character), typeof(GameLocation), typeof(Point), typeof(int) }),
               prefix: new HarmonyMethod(typeof(PathFindControllerPatches), nameof(PathFindControllerPatches.PathFindController_Prefix))
            );


            // Location patches

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.GetSpouseBed)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_GetSpouseBed_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.getSpouseBedSpot)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.FarmHouse_getSpouseBedSpot_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Beach), "resetLocalState"),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_resetLocalState_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "checkEventPrecondition"),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_checkEventPrecondition_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Desert), nameof(Desert.getDesertMerchantTradeStock)),
               postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Desert_getDesertMerchantTradeStock_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ManorHouse), nameof(ManorHouse.performAction)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.ManorHouse_performAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogue)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_answerDialogue_prefix))
            );


            // pregnancy patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.Utility_pickPersonalFarmEvent_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(QuestionEvent), nameof(QuestionEvent.setUp)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.QuestionEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.setUp)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.BirthingEvent_setUp_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.tickUpdate)),
               prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.BirthingEvent_tickUpdate_Prefix))
            );


            // Farmer patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doDivorce)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_doDivorce_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.isMarried)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_isMarried_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getSpouse)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_getSpouse_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.checkAction)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_checkAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetSpouseFriendship)),
               prefix: new HarmonyMethod(typeof(FarmerPatches), nameof(FarmerPatches.Farmer_GetSpouseFriendship_Prefix))
            );


            // UI patches

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawNPCSlot_prefix)),
               transpiler: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawSlot_transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(SocialPage), "drawFarmerSlot"),
               transpiler: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.SocialPage_drawSlot_transpiler))
            );

            harmony.Patch(
               original: typeof(DialogueBox).GetConstructor(new Type[] { typeof(List<string>) }),
               prefix: new HarmonyMethod(typeof(UIPatches), nameof(UIPatches.DialogueBox_Prefix))
            );


            // Event patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.answerDialogueQuestion)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_answerDialogueQuestion_Prefix))
            );

        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;

            if (asset.AssetNameEquals("Data/Events/HaleyHouse") || asset.AssetNameEquals("Data/Events/Saloon") || asset.AssetNameEquals("Data/EngagementDialogue") || asset.AssetNameEquals("Strings/StringsFromCSFiles") || asset.AssetNameEquals("Data/animationDescriptions"))
            {
                return true;
            }
            if (config.RomanceAllVillagers && (asset.AssetName.StartsWith("Characters/schedules") || asset.AssetName.StartsWith("Characters\\schedules")))
            {
                try
                {
                    string name = asset.AssetName.Replace("Characters/schedules/", "").Replace("Characters\\schedules\\", "");
                    NPC npc = Game1.getCharacterFromName(name);
                    if (npc != null && npc.Age < 2 && !(npc is Child))
                    {
                        string dispo = Helper.Content.Load<Dictionary<string, string>>("Data/NPCDispositions", ContentSource.GameContent)[name];
                        if (dispo.Split('/')[5] != "datable")
                        {
                            Monitor.Log($"can edit schedule for {name}");
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            Monitor.Log("Editing asset " + asset.AssetName);
            if (asset.AssetNameEquals("Data/Events/HaleyHouse"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"] = Regex.Replace(data["195012/f Haley 2500/f Emily 2500/f Penny 2500/f Abigail 2500/f Leah 2500/f Maru 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 38/e 2123343/e 10/e 901756/e 54/e 15/k 195019"], "(pause 1000/speak Maru \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 3", "");
                data["choseToExplain"] = Regex.Replace(data["choseToExplain"], "(pause 1000/speak Maru \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 4", "");
                data["lifestyleChoice"] = Regex.Replace(data["lifestyleChoice"], "(pause 1000/speak Maru \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-female")}$h\"/emote Haley 21 true/emote Emily 21 true/emote Penny 21 true/emote Maru 21 true/emote Leah 21 true/emote Abigail 21").Replace("/dump girls 4", "");
            }
            else if (asset.AssetNameEquals("Data/Events/Saloon"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

                string aData = data["195013/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/k 195099"];
                data["195013/f Shane 2500/f Sebastian 2500/f Sam 2500/f Harvey 2500/f Alex 2500/f Elliott 2500/o Abigail/o Penny/o Leah/o Emily/o Maru/o Haley/o Shane/o Harvey/o Sebastian/o Sam/o Elliott/o Alex/e 911526/e 528052/e 9581348/e 43/e 384882/e 233104/k 195099"] = Regex.Replace(aData, "(pause 1000/speak Sam \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 3", "");
                aData = data["choseToExplain"];
                data["choseToExplain"] = Regex.Replace(aData, "(pause 1000/speak Sam \\\")[^$]+.a\\\"", $"$1{PHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 4", "");
                aData = data["crying"];
                data["crying"] = Regex.Replace(aData, "(pause 1000/speak Sam \\\")[^$]+.a\\\"",$"$1{PHelper.Translation.Get("confrontation-male")}$h\"/emote Shane 21 true/emote Sebastian 21 true/emote Sam 21 true/emote Harvey 21 true/emote Alex 21 true/emote Elliott 21").Replace("/dump guys 4", "");
            }
            else if (asset.AssetNameEquals("Strings/StringsFromCSFiles"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                data["NPC.cs.3985"] = Regex.Replace(data["NPC.cs.3985"],  @"\.\.\.\$s.+", $"$n#$b#$c 0.5#{data["ResourceCollectionQuest.cs.13681"]}#{data["ResourceCollectionQuest.cs.13683"]}");
                Monitor.Log($"NPC.cs.3985 is set to \"{data["NPC.cs.3985"]}\"");
            }
            else if (asset.AssetNameEquals("Data/animationDescriptions"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                List<string> sleepKeys = data.Keys.ToList().FindAll((s) => s.EndsWith("_Sleep"));
                foreach(string key in sleepKeys)
                {
                    if (!data.ContainsKey(key.ToLower()))
                    {
                        Monitor.Log($"adding {key.ToLower()} to animationDescriptions");
                        data.Add(key.ToLower(), data[key]);
                    }
                }
            }
            else if (asset.AssetNameEquals("Data/EngagementDialogue"))
            {
                if (!config.RomanceAllVillagers)
                    return;
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                Farmer f = Game1.player;
                if (f == null)
                {
                    return;
                }
                foreach (string friend in f.friendshipData.Keys)
                {
                    if (!data.ContainsKey(friend+"0"))
                    {
                        data[friend + "0"] = "";
                    }
                    if (!data.ContainsKey(friend+"1"))
                    {
                        data[friend + "1"] = "";
                    }
                }
            }
            else if (asset.AssetName.StartsWith("Characters/schedules") || asset.AssetName.StartsWith("Characters\\schedules"))
            {
                IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                List<string> keys = new List<string>(data.Keys);
                foreach (string key in keys)
                {
                    if(!data.ContainsKey($"marriage_{key}"))
                        data[$"marriage_{key}"] = data[key]; 
                }
            }
        }

    }
}
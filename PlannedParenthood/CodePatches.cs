using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace PlannedParenthood
{
    public partial class ModEntry
    {
        public static int namePage;


        private static List<string> GetSpouseNames()
        {
            List<string> names = Game1.player.friendshipData.Keys.Where(k => Game1.player.friendshipData[k].IsMarried() && !Game1.player.friendshipData[k].RoommateMarriage && Game1.player.friendshipData[k].Points / 250 >= Config.MinHearts).ToList();
            SMonitor.Log($"Got {names.Count} spouses");
            for (int i = names.Count - 1; i >= 0; i--)
            {
                if(Game1.player.friendshipData[names[i]].DaysUntilBirthing >= 0)
                {
                    SMonitor.Log($"Found existing birth event with {names[0]}, preventing birth questions", LogLevel.Debug);
                    return new List<string>();
                }
                var npc = Game1.getCharacterFromName(names[i], true);
                if (npc is null || (Config.InBed && !npc.isSleeping.Value))
                    names.RemoveAt(i);
            }
            foreach (var kvp in Game1.player.team.friendshipData.Pairs)
            {
                if (kvp.Value?.IsMarried() == true)
                {
                    Farmer spouse = Game1.getFarmer((kvp.Key.Farmer1 == Game1.player.UniqueMultiplayerID ? kvp.Key.Farmer2 : kvp.Key.Farmer1));
                    if (kvp.Value.NextBirthingDate is not null)
                    {
                        SMonitor.Log($"Found existing birth event with {spouse.Name}, preventing birth questions", LogLevel.Debug);
                        return new List<string>();
                    }
                    if (spouse != null)
                    {
                        SMonitor.Log($"Adding PC spouse {spouse.Name} to list");
                        names.Add(spouse.Name);
                    }
                    else
                    {
                        SMonitor.Log($"Error getting partner for {kvp.Key.Farmer1} & {kvp.Key.Farmer2}; local: {Game1.player.UniqueMultiplayerID}");
                    }
                }
            }
            return names;
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
        public class GameLocation_answerDialogueAction_Patch
        {
            public static bool Prefix(GameLocation __instance, ref string questionAndAnswer, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                partnerName = null;
                if (questionAndAnswer == "Sleep_Baby")
                {
                    SMonitor.Log("Starting baby dialogue");

                    CreateNameListQuestion(__instance);

                    __result = true;
                    return false;
                }
                else if (questionAndAnswer.StartsWith("SleepBaby_"))
                {
                    string name = questionAndAnswer.Substring("SleepBaby_".Length);
                    if (name == "prev_page")
                    {
                        namePage--;
                        CreateNameListQuestion(__instance);
                    }
                    else if (name == "next_page")
                    {
                        namePage++;
                        CreateNameListQuestion(__instance);
                    }
                    else
                    {
                        partnerName = name;
                        SMonitor.Log($"Trying to make baby with {partnerName}");
                        questionAndAnswer = "Sleep_Yes";
                        return true;
                    }
                    __result = true;
                    return false;
                }
                return true;
            }
            private static void CreateNameListQuestion(GameLocation __instance)
            {
                var names = GetSpouseNames();

                int totalNames = names.Count;

                names = names.Skip(namePage * Config.NamesPerPage).Take(Config.NamesPerPage).ToList();

                List<Response> responses = new List<Response>();

                if (namePage > 0)
                    responses.Add(new Response("prev_page", "..."));
                foreach (var name in names)
                {
                    responses.Add(new Response(name, name));
                }
                if (Config.NamesPerPage * (namePage + 1) < totalNames)
                    responses.Add(new Response("next_page", "..."));

                __instance.createQuestionDialogue(SHelper.Translation.Get("which-npc"), responses.ToArray(), "SleepBaby");
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue), new Type[] { typeof(string), typeof(Response[]), typeof(string), typeof(Object) })]
        public class GameLocation_createQuestionDialogue_Patch
        {
            public static void Prefix(GameLocation __instance, ref string question, ref Response[] answerChoices, string dialogKey)
            {
                if (!Config.ModEnabled || dialogKey != "Sleep")
                    return;
                SMonitor.Log($"Showing sleep confirmation");

                var fh = Utility.getHomeOfFarmer(Game1.player);
                
                if (fh.upgradeLevel < 2)
                {
                    SMonitor.Log($"Cannot ask about pregnancy; farmhouse level {fh.upgradeLevel}", LogLevel.Debug);
                    return;
                }
                if (fh.cribStyle.Value <= 0)
                {
                    SMonitor.Log($"Cannot ask about pregnancy; no crib", LogLevel.Debug);
                    return;
                }

                partnerName = null;

                var names = GetSpouseNames();
                if (!names.Any())
                {
                    SMonitor.Log($"Cannot ask about pregnancy; no applicable spouses", LogLevel.Debug);
                    return;
                }

                var list = answerChoices.ToList();
                list.Add(new Response("Baby", SHelper.Translation.Get("make-baby")));
                answerChoices = list.ToArray();
            }

        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.pickPersonalFarmEvent))]
        public class Utility_pickPersonalFarmEvent_Patch
        {

            public static bool Prefix(ref FarmEvent __result)
            {
                if (!Config.ModEnabled || partnerName is null)
                    return true;
                if (Game1.weddingToday)
                {
                    __result = null;
                    return false;
                }
                if (freeLoveAPI is not null)
                    freeLoveAPI.SetLastPregnantSpouse(partnerName);

                if (Game1.player.friendshipData.ContainsKey(partnerName))
                {
                    SMonitor.Log($"creating NPC pregnancy event with {partnerName}");
                    __result = new QuestionEvent(1);
                }
                else if(Game1.getAllFarmers().ToList().Exists(f => f.Name == partnerName))
                {
                    SMonitor.Log($"creating PC pregnancy event with {partnerName}");
                    __result = new QuestionEvent(3);
                }
                return false;
            }
        }
    }
}
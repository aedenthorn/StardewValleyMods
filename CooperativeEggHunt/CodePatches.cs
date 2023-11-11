using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CooperativeEggHunt
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Event), "eggHuntWinner")]
        public class Event_eggHuntWinner_Patch
        {
            public static bool Prefix(Event __instance, HashSet<long> ___festivalWinners)
            {
                if (!Config.ModEnabled)
                    return true;
                Dictionary<string, int> hunterEggs = new Dictionary<string, int>();
                int eggs = 0;
                foreach (Farmer temp in Game1.getOnlineFarmers())
                {
                    hunterEggs[temp.Name] = temp.festivalScore;
                    eggs += temp.festivalScore;
                }
                foreach(string name in Config.NPCHunters.Split(','))
                {
                    var e = GetEggs(__instance.getActorByName(name));
                    if (e < 0)
                        continue;
                    hunterEggs[name] = e;
                    eggs += e;
                }
                List<string> dialogueArray = new();
                foreach(var h in hunterEggs)
                {
                    dialogueArray.Add(h.Value != 1 ? string.Format(SHelper.Translation.Get("results-x"), h.Key, h.Value) : string.Format(SHelper.Translation.Get("results-1"), h.Key));
                }
                var won = eggs >= Config.EggsToWin;
                dialogueArray.Add(string.Format(SHelper.Translation.Get("results-total"), eggs));
                dialogueArray.Add(won ? SHelper.Translation.Get("results-enough") : SHelper.Translation.Get("results-not-enough"));
                __instance.specialEventVariable1 = !won;

                if (won)
                {
                    foreach (Farmer temp in Game1.getOnlineFarmers())
                    {
                        ___festivalWinners.Add(temp.UniqueMultiplayerID);
                    }
                }

                string dialogue = string.Join("#$b#", dialogueArray);
                var lewis = __instance.getActorByName("Lewis");
                lewis.CurrentDialogue.Push(new Dialogue(lewis, null, dialogue));
                Game1.drawDialogue(lewis);

                return false;
            }
        }
        
        [HarmonyPatch(typeof(Game1), nameof(Game1.drawDialogue), new Type[] { typeof(NPC)})]
        public class Game1_drawDialogue_Patch
        {
            public static void Postfix(NPC speaker)
            {
                if (!Config.ModEnabled || !Game1.isFestival() || Game1.CurrentEvent is null || !AccessTools.FieldRefAccess<Event, Dictionary<string, string>>(Game1.CurrentEvent, "festivalData")["file"].Equals("spring13") || !Config.NPCHunters.Split(',').Contains(speaker.Name))
                    return;
                int talked = 0;
                if(speaker.modData.TryGetValue(talkedKey, out string t))
                    int.TryParse(t, out talked);
                talked++;
                speaker.modData[talkedKey] = talked + "";
                SMonitor.Log($"Talked to {speaker.Name} {talked} times");
            }
        }

    }
}
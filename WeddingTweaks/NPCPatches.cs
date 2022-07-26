using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;

namespace WeddingTweaks
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
        public class GameLocation_answerDialogueAction_Patch
        {
            public static bool Prefix(GameLocation __instance, ref string questionAndAnswer, ref bool __result)
            {
                if (!Config.EnableMod)
                    return true;
                if (questionAndAnswer.StartsWith("WeddingWitness_"))
                {
                    string name = questionAndAnswer.Substring("WeddingWitness_".Length);
                    if (!Game1.player.friendshipData.TryGetValue(name, out Friendship f))
                        return true;
                    string dialogue;
                    float chance;
                    WeddingData data = null;
                    npcWeddingDict.TryGetValue(name, out data);
                    if (data is not null && data.witnessAcceptChance >= 0)
                    {
                        chance = data.witnessAcceptChance / 100f;
                    }
                    else
                    {
                        chance = Config.WitnessAcceptPercent / 100f + Config.WitnessAcceptHeartFactorPercent / 100f * f.Points / (14f * 250);
                    }
                    SMonitor.Log($"accept percent chance: {chance * 100}: {Config.WitnessAcceptPercent / 100f} + {Config.WitnessAcceptHeartFactorPercent / 100f * f.Points / (14f * 250)}; points {f.Points}/{14 * 250}");
                    if(Game1.random.NextDouble() <= chance)
                    {
                        SMonitor.Log($"Setting witness to {name}");
                        Game1.player.modData[witnessKey] = name;
                        if (data is not null && data.witnessAcceptDialogue.Count > 0)
                        {
                            dialogue = data.witnessAcceptDialogue[Game1.random.Next(data.witnessAcceptDialogue.Count)];
                        }
                        else
                        {
                            dialogue = SHelper.Translation.Get("witness-accept");
                        }
                    }
                    else
                    {
                        SMonitor.Log($"{name} rejected witness request");
                        npcWitnessAsked.Add(name);
                        if (data is not null && data.witnessDeclineDialogue.Count > 0)
                        {
                            dialogue = data.witnessDeclineDialogue[Game1.random.Next(data.witnessDeclineDialogue.Count)];
                        }
                        else
                        {
                            dialogue = SHelper.Translation.Get("witness-decline");
                        }
                    }
                    NPC npc = Game1.getCharacterFromName(name, true);
                    if (npc != null)
                    {
                        npc.CurrentDialogue.Clear();
                        npc.CurrentDialogue.Push(new Dialogue(dialogue, npc));
                        Game1.drawDialogue(npc);
                    }
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.checkAction))]
        public static class NPC_checkAction_Patch
        {
            public static void Postfix(NPC __instance, Farmer who, ref bool __result, GameLocation l)
            {
                if (!Config.EnableMod || !Config.AllowWitnesses || __result || !who.IsLocalPlayer || !who.isEngaged() || npcWitnessAsked.Contains(__instance.Name) || who.modData.ContainsKey(witnessKey) || !who.friendshipData.TryGetValue(__instance.Name, out Friendship f) || f.IsEngaged() || f.Points < Config.WitnessMinHearts / 250)
                    return;

                SMonitor.Log($"Asking about witness for {__instance.Name}");
                var responses = l.createYesNoResponses();
                responses[0].responseKey = __instance.Name;
                l.createQuestionDialogue(string.Format(SHelper.Translation.Get("ask-x-to-witness"), __instance.Name), responses, "WeddingWitness", null);
                __result = true;
            }
        }

        [HarmonyPatch(typeof(NPC), "engagementResponse")]
        public static class NPC_engagementResponse_Patch
        {

            public static void Postfix(NPC __instance, Farmer who, bool asRoommate = false)
            {
                if (asRoommate)
                {
                    SMonitor.Log($"{__instance.Name} is roomate");
                    return;
                }
                if (!who.friendshipData.ContainsKey(__instance.Name))
                {
                    SMonitor.Log($"{who.Name} has no friendship data for {__instance.Name}", LogLevel.Error);
                    return;
                }
                Friendship friendship = who.friendshipData[__instance.Name];
                WorldDate weddingDate = new WorldDate(Game1.Date);
                weddingDate.TotalDays += Math.Max(1, Config.DaysUntilMarriage);
                while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
                {
                    weddingDate.TotalDays++;
                }
                friendship.WeddingDate = weddingDate;
            }
        }
    }
}

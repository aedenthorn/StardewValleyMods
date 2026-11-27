using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuestHelper
{
	public partial class ModEntry
    {
        [HarmonyPatch(typeof(NPC), nameof(NPC.draw), new Type[] {typeof(SpriteBatch), typeof(float) })]
        public class NPC_draw_Patch
        {
            public static void Postfix(NPC __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !Config.ShowQuestMarkers || !Context.IsPlayerFree)
                    return;
                bool show = false;
                foreach (var q in Game1.player.questLog)
                {
                    if (__instance.IsVillager)
                    {
                        if (q is ResourceCollectionQuest rq && !rq.completed.Value && rq.numberCollected.Value >= rq.number.Value && rq.target.Value == __instance.Name)
                        {
                            show = true;
                            break;
                        }
                        if (q is SocializeQuest sq && !sq.completed.Value && sq.whoToGreet.Contains(__instance.Name))
                        {
                            show = true;
                            break;
                        }
                        if (q is SecretLostItemQuest lq && lq.itemFound.Value && lq.npcName.Value == __instance.Name && Game1.player.Items.ContainsId(lq.ItemId.Value) && !lq.completed.Value)
                        {
                            show = true;
                            break;
                        }
                        if (q is LostItemQuest iq && iq.itemFound.Value && iq.npcName.Value == __instance.Name && Game1.player.Items.ContainsId(iq.ItemId.Value) && !iq.completed.Value)
                        {
                            show = true;
                            break;
                        }
                        if (q is ItemDeliveryQuest dq && dq.target.Value == __instance.Name && Game1.player.Items.CountId(dq.ItemId.Value) >= dq.number.Value && !dq.completed.Value)
                        {
                            show = true;
                            break;
                        }
                        if (q is FishingQuest fq && fq.target.Value == __instance.Name && fq.numberFished.Value >= fq.numberToFish.Value && !fq.completed.Value)
                        {
                            show = true;
                            break;
                        }
                        if (q is SlayMonsterQuest mq && mq.target.Value == __instance.Name && mq.numberKilled.Value >= mq.numberToKill.Value && !mq.completed.Value)
                        {
                            show = true;
                            break;
                        }
                    }
                    if(__instance is Monster m)
                    {
                        if (q is SlayMonsterQuest mq && !mq.completed.Value && mq.numberKilled.Value < mq.numberToKill.Value && (m.Name.Contains(mq.monsterName.Value) || (mq.id.Value == "15" && IsSlimeName(m.Name))))
                        {
                            show = true;
                            break;
                        }
                    }
                }
                if (show)
                {
                    DrawQuestMarker(b, __instance.Position);
                }
            }
        }
        [HarmonyPatch(typeof(Quest), nameof(Quest.GetDescription))]
        public class Quest_GetDescription_Patch
        {
            public static void Postfix(Quest __instance, ref string __result)
            {
                if (!Config.ModEnabled || !Config.ShowQuestDetails || __instance.completed.Value)
                    return;
                List<string> output;
                if (__instance is SocializeQuest sq && !sq.completed.Value && sq.whoToGreet.Count > 0)
                {
                    __result += "\n\n" + string.Format(SHelper.Translation.Get("left-to-greet"), sq.whoToGreet.Select(n => Game1.getCharacterFromName(n)?.displayName ?? n).Join());
                }
                else if (__instance is FishingQuest fq)
                {
                    output = GetFishInfo(fq.ItemId.Value);
                    if (output != null)
                    {
                        __result += "\n\n" + string.Join("\n\n", output);
                    }
                }
                else if (__instance is ItemDeliveryQuest dq)
                {
                    output = GetItemInfo(dq.ItemId.Value);
                    if (output != null)
                    {
                        __result += "\n\n" + string.Join("\n\n", output);
                    }
                }
                else if (__instance.id.Value == "22")
                {
                    output = GetFishInfo("(O)136");
                    if (output != null)
                    {
                        __result += "\n\n" + string.Join("\n\n", output);
                    }
                }
                else if (__instance.id.Value == "127")
                {
                    output = GetItemInfo("220");
                    if (output != null)
                    {
                        __result += "\n\n" + string.Join("\n\n", output);
                    }
                }
            }
        }
    }
}

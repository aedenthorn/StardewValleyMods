using HarmonyLib;
using StardewValley;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StardewRPG
{
    public partial class ModEntry
    {
        private static bool NPC_engagementResponse_Prefix(NPC __instance, Farmer who)
        {
            if (!Config.EnableMod || !Config.ChaRollRomanceChance)
                return true;
            bool success = Game1.random.Next(20) < GetStatValue(who, "cha", Config.BaseStatValue);
            if (success)
                return true;
            who.reduceActiveItemByOne();
            SMonitor.Log("cha check failed on proposal");
            Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("cha-check-failed"), 3));
            Game1.playSound("cancel");
            __instance.CurrentDialogue.Push(__instance.TryGetDialogue("RejectMermaidPendant_Under10Hearts") ?? new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3972", "3973"), false));
            Game1.drawDialogue(__instance);
            who.changeFriendship(-20, __instance);
            who.friendshipData[__instance.Name].ProposalRejected = true;
            return false;
        }
        public static IEnumerable<CodeInstruction> NPC_tryToReceiveActiveObject_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            SMonitor.Log($"Transpiling NPC.tryToReceiveActiveObject");

            var codes = new List<CodeInstruction>(instructions);
            var label = generator.DefineLabel();
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldloc_1 && codes[i + 1].opcode == OpCodes.Brtrue_S)
                {
                    SMonitor.Log("Adding bouquet fail");
                    codes[i].labels.Add(label);
                    codes.Insert(i, new CodeInstruction(OpCodes.Ret));
                    codes.Insert(i, new CodeInstruction(OpCodes.Brtrue, label));
                    codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckDatingChance))));
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_1));
                    codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        private static bool CheckDatingChance(NPC npc, Farmer who)
        {
            if (!Config.EnableMod || !Config.ChaRollRomanceChance)
                return true;
            bool success = Game1.random.Next(20) < GetStatValue(who, "cha", Config.BaseStatValue);
            if (success)
                return true;
            SMonitor.Log("cha check failed on date");
            Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("cha-check-failed"), 3));
            Game1.playSound("cancel");
            Stack<Dialogue> currentDialogue9 = npc.CurrentDialogue;
            Dialogue dialogue17;
            if ((dialogue17 = npc.TryGetDialogue("RejectBouquet_LowHearts")) == null)
            {
                dialogue17 = npc.TryGetDialogue("RejectBouquet") ?? new Dialogue(npc, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3960", "3961"), false);
            }
            currentDialogue9.Push(dialogue17);
            Game1.drawDialogue(npc);
            return false;
        }
    }
}
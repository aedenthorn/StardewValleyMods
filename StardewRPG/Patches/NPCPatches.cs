using StardewValley;
using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

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
            __instance.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3973"), __instance));
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
                if (i < codes.Count - 2 && codes[i].opcode == OpCodes.Stloc_S && codes[i + 1].opcode == OpCodes.Ldloc_S && codes[i].operand == codes[i + 1].operand && codes[i + 2].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 2].operand == AccessTools.Method(typeof(Friendship), nameof(Friendship.IsDating)))
                {
                    SMonitor.Log("Adding bouquet fail");
                    codes[i + 1].labels.Add(label);
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ret));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Brtrue_S, label));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.CheckDatingChance))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_1));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
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
            who.reduceActiveItemByOne();
            SMonitor.Log("cha check failed on date");
            Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("cha-check-failed"), 3));
            Game1.playSound("cancel");
            npc.CurrentDialogue.Push(new Dialogue((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3960") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3961"), npc));
            Game1.drawDialogue(npc);
            who.friendshipData[npc.Name].ProposalRejected = true;
            return false;
        }
    }
}
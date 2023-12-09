using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FreeLove
{
    public static class UIPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static void SocialPage_drawNPCSlot_prefix(SocialPage __instance, int i)
        {
            try
            {
                SocialPage.SocialEntry entry = __instance.GetSocialEntry(i);
                if (entry.IsChild)
                {
                    if (entry.DisplayName.EndsWith(")"))
                    {
                        AccessTools.FieldRefAccess<SocialPage.SocialEntry, string>(entry, "DisplayName") = string.Join(" ", entry.DisplayName.Split(' ').Reverse().Skip(1).Reverse());
                        __instance.SocialEntries[i] = entry;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(SocialPage_drawNPCSlot_prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static bool SocialPage_isMarriedToAnyone_Prefix(SocialPage.SocialEntry __instance, ref bool __result)
        {
            try
            {
                foreach (Farmer farmer in Game1.getAllFarmers())
                {
                    if (farmer.spouse == __instance.InternalName && farmer.friendshipData.TryGetValue(__instance.InternalName, out Friendship friendship) && friendship.IsMarried())
                    {
                        __result = true;
                    }
                }
                __result = false;
                return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(SocialPage_isMarriedToAnyone_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        public static IEnumerable<CodeInstruction> SocialPage_drawSlot_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            if (Helper.ModRegistry.IsLoaded("SG.Partners"))
            {
                Monitor.Log("Keep Your Partners mod is loaded, not patching social page.");
                return codes.AsEnumerable();
            }
            try
            {
                MethodInfo m_IsMarried = AccessTools.Method(typeof(Farmer), "isMarried", null, null);
                int index = codes.FindIndex((CodeInstruction c) => c.operand != null && c.operand is MethodInfo && (MethodInfo)c.operand == m_IsMarried);
                if(index > -1)
                {
                    codes[index - 1].opcode = OpCodes.Nop;
                    codes[index].opcode = OpCodes.Nop;
                    codes[index + 1].opcode = OpCodes.Nop;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(SocialPage_drawSlot_transpiler)}:\n{ex}", LogLevel.Error);
            }
            return codes.AsEnumerable();
        }

        public static void DialogueBox_Prefix(ref List<string> dialogues)
        {
            try
            {
                if (dialogues == null || dialogues.Count < 2)
                    return;

                if (dialogues[1] == Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1826"))
                {
                    List<string> newDialogues = new List<string>()
                    {
                        dialogues[0]
                    };



                    List<NPC> spouses = ModEntry.GetSpouses(Game1.player,true).Values.OrderBy(o => Game1.player.friendshipData[o.Name].Points).Reverse().Take(4).ToList();

                    List<int> which = new List<int>{ 0, 1, 2, 3 };

                    ModEntry.ShuffleList(ref which);

                    List<int> myWhich = new List<int>(which).Take(spouses.Count).ToList();

                    for(int i = 0; i < spouses.Count; i++)
                    {
                        switch (which[i])
                        {
                            case 0:
                                newDialogues.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1827", spouses[i].displayName));
                                break;
                            case 1:
                                newDialogues.Add(((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1832") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1834")) + " " + ((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", spouses[i].displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", spouses[i].displayName[0])));
                                break;
                            case 2:
                                newDialogues.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1843", spouses[i].displayName));
                                break;
                            case 3:
                                newDialogues.Add(((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1831") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1833")) + " " + ((spouses[i].Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", spouses[i].displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", spouses[i].displayName[i])));
                                break;
                        }
                    }
                    dialogues = new List<string>(newDialogues);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DialogueBox_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
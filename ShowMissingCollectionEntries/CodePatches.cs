using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ShowMissingCollectionEntries
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(CollectionsPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class CollectionsPage_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling CollectionsPage.ctor");

                bool start = false;
                bool found = false;
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!start && codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(Farmer), nameof(Farmer.achievements)))
                    {
                        start = true;
                    }
                    else if (start && codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(String), nameof(String.Equals), new Type[] { typeof(string) }))
                    {
                        SMonitor.Log("Adding achievement achieved");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.AchievementAchieved))));
                        if (found)
                        {
                            break;
                        }
                        found = true;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(CollectionsPage), nameof(CollectionsPage.draw), new Type[] { typeof(SpriteBatch) })]
        public class CollectionsPage_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling CollectionsPage.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.Black)))
                    {
                        SMonitor.Log("Adding color replace");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ModifyColor))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(PowersTab), nameof(PowersTab.draw), new Type[] { typeof(SpriteBatch) })]
        public class PowersTab_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling PowersTab.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.Black)))
                    {
                        SMonitor.Log("Adding color replace");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ModifyColor))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(CollectionsPage), nameof(CollectionsPage.performHoverAction))]
        public class CollectionsPage_performHoverAction_Patch
        {

            public static bool Prefix(CollectionsPage __instance, int x, int y, ref string ___hoverText, ref int ___value)
            {
                if (!Config.ModEnabled || __instance.currentTab == 7 || (__instance.currentTab == 5 && !Config.ShowMissingAchievementNames) || (__instance.currentTab != 5 && !Config.ShowMissingItemNames))
                    return true;
                ___hoverText = "";
                ___value = -1;
                bool found = false;
                foreach (ClickableTextureComponent cc in __instance.collections[__instance.currentTab][__instance.currentPage])
                {
                    if (cc.containsPoint(x, y, 2))
                    {
                        cc.scale = Math.Min(cc.scale + 0.02f, cc.baseScale + 0.1f);
                        string[] data_split = ArgUtility.SplitBySpace(cc.name);
                        if ((data_split.Length > 1 && Convert.ToBoolean(data_split[1])) || (data_split.Length > 2 && Convert.ToBoolean(data_split[2])))
                        {
                            return true;
                        }
                        else
                        {
                            if ((__instance.currentTab != 5 && Config.ShowMissingItemDetails) || (__instance.currentTab == 5 && (Config.ShowMissingAchievementDetails || Game1.achievements[int.Parse(data_split[0])].Split('^')[2] == "true")))
                            {
                                ___hoverText = __instance.createDescription(data_split[0]);
                            }
                            else
                            { 
                                if(__instance.currentTab == 5)
                                {
                                    int index = int.Parse(data_split[0]);
                                    string[] split = Game1.achievements[index].Split('^', StringSplitOptions.None);
                                    ___hoverText = split[0];
                                }
                                else
                                {
                                    ParsedItemData data = ItemRegistry.GetDataOrErrorItem(data_split[0]);
                                    ___hoverText = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + data.ItemId + "_CollectionsTabName", true) ?? data.DisplayName;
                                }
                            }
                        }
                        found = true;
                    }
                    else
                    {
                        cc.scale = Math.Max(cc.scale - 0.02f, cc.baseScale);
                    }
                }
                return !found;
            }
        }

        [HarmonyPatch(typeof(PowersTab), nameof(PowersTab.performHoverAction))]
        public class PowersTab_performHoverAction_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling PowersTab.performHoverAction");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(ClickableTextureComponent), nameof(ClickableTextureComponent.drawShadow)))
                    {
                        SMonitor.Log("Adding replace to allow name");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(AllowName))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
            public static void Postfix(PowersTab __instance)
            {
                if (!Config.ModEnabled || Config.ShowMissingPowerDetails || string.IsNullOrEmpty(__instance.hoverText))
                    return;
                foreach(var p in __instance.powers[__instance.currentPage])
                {
                    if(p.label == __instance.hoverText && !p.drawShadow)
                    {
                        __instance.descriptionText = "";
                    }
                }
            }
        }
    }
}
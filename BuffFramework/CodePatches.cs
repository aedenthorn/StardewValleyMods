using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace BuffFramework
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.newDayAfterFade))]
        public class Game1_newDayAfterFade_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
                foreach (var kvp in farmerBuffs.Value)
                {
                    Game1.buffsDisplay.removeOtherBuff(kvp.Value.which);
                }
                farmerBuffs.Value.Clear();
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.Update))]
        public class Farmer_Update_Patch
        {
            public static void Prefix(Farmer __instance)
            {
                if (!Config.ModEnabled || !__instance.IsLocalPlayer || farmerBuffs.Value is null)
                    return;
                foreach(var fb in farmerBuffs.Value.Values)
                {
                    Buff? buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == fb.which);
                    if (buff == null)
                    {
                        Game1.buffsDisplay.addOtherBuff(
                            buff = fb
                        );
                    }
                    buff.millisecondsDuration = 50;
                }
            }
        }
        [HarmonyPatch(typeof(BuffsDisplay), nameof(BuffsDisplay.performHoverAction))]
        public class BuffsDisplay_performHoverAction_Patch
        {
            public static bool Prefix(BuffsDisplay __instance, int x, int y, Dictionary<ClickableTextureComponent, Buff> ___buffs)
            {
                if (!Config.ModEnabled)
                    return true;

                __instance.hoverText = "";
                foreach (KeyValuePair<ClickableTextureComponent, Buff> c in ___buffs)
                {
                    if (c.Key.containsPoint(x, y))
                    {
                        bool showTime = c.Value.millisecondsDuration / 60000 > 0 || c.Value.millisecondsDuration % 60000 / 10000 + c.Value.millisecondsDuration % 60000 % 10000 / 1000 > 0;
                        __instance.hoverText = c.Key.hoverText + (showTime ? Environment.NewLine + c.Value.getTimeLeft() : "");
                        c.Key.scale = Math.Min(c.Key.baseScale + 0.1f, c.Key.scale + 0.02f);
                        return false;
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(Buff), nameof(Buff.getClickableComponents))]
        public class Buff_getClickableComponents_Patch
        {
            public static void Postfix(Buff __instance, ref List<ClickableTextureComponent> __result)
            {
                if (!Config.ModEnabled)
                    return;
                
                foreach (var kvp in farmerBuffs.Value)
                {
                    object texturePath = null;
                    object description = null;
                    if (kvp.Value.which == __instance.which)
                    {
                        var hasTex = buffDict[kvp.Key].TryGetValue("texturePath", out texturePath);
                        var hasDesc = buffDict[kvp.Key].TryGetValue("description", out description);
                        if (hasTex || hasDesc)
                        {
                            var tex = texturePath is not null ? SHelper.GameContent.Load<Texture2D>((string)texturePath) : (__result.Any() ? __result[0].texture : null);
                            var cc = new ClickableTextureComponent("", Rectangle.Empty, null, description is not null ? (string)description : (__result.Any() ? __result[0].hoverText : null), tex, texturePath is not null ? new Rectangle(0, 0, tex.Width, tex.Height) : (__result.Any() ? __result[0].sourceRect : new Rectangle()), 4f, false);

                            if (buffDict[kvp.Key].TryGetValue("separate", out var separate) && (bool)separate)
                            {
                                __result.Insert(0, cc);
                            }
                            else
                            {
                                __result = new() {
                                    cc
                                };
                            }
                        }
                        return;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Buff), nameof(Buff.addBuff))]
        public class Buff_addBuff_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Buff.addBuff");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 0.05f && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand is MethodInfo && (MethodInfo)codes[i + 1].operand == AccessTools.Method(typeof(Character), nameof(Character.startGlowing)))
                    {
                        SMonitor.Log($"adding check for custom glow rate");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckGlowRate))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }
                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(Buff), nameof(Buff.removeBuff))]
        public class Buff_removeBuff_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Buff.removeBuff");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 0.05f && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand is MethodInfo && (MethodInfo)codes[i + 1].operand == AccessTools.Method(typeof(Character), nameof(Character.startGlowing)))
                    {
                        SMonitor.Log($"adding check for custom glow rate");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckGlowRate))));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }
                return codes.AsEnumerable();
            }
        }
    }
}
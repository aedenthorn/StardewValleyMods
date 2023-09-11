using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;
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
                if (!Config.ModEnabled || Game1.buffsDisplay is null)
                    return;
                if(farmerBuffs.Value is null)
                {
                    farmerBuffs.Value = new();
                    return;
                }
                foreach (var kvp in farmerBuffs.Value)
                {
                    Game1.buffsDisplay.removeOtherBuff(kvp.Value.which);
                }
                farmerBuffs.Value.Clear();
                ClearCues();
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.Update))]
        public class Farmer_Update_Patch
        {
            public static void Prefix(Farmer __instance)
            {
                if (!Config.ModEnabled || !__instance.IsLocalPlayer || farmerBuffs.Value is null)
                    return;
                foreach(var key in farmerBuffs.Value.Keys.ToArray())
                {
                    var fb = farmerBuffs.Value[key];
                    Buff? buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == fb.which);

                    if(fb.totalMillisecondsDuration <= 50)
                    {
                        if (buff is not null && buff.totalMillisecondsDuration > 50)
                        {
                            Game1.buffsDisplay.removeOtherBuff(buff.which);
                            buff = null;
                        }
                        if (buff == null)
                        {
                            Game1.buffsDisplay.addOtherBuff(
                                buff = fb
                            );
                        }
                        buff.millisecondsDuration = 50;
                    }
                    else
                    {
                        if (buff == fb)
                        {
                            if (buff.millisecondsDuration <= 50 && buff.totalMillisecondsDuration > 50)
                            {
                                farmerBuffs.Value.Remove(key);
                                if (cues.TryGetValue(key, out var cue))
                                {
                                    if (cue.IsPlaying)
                                    {
                                        cue.Stop(AudioStopOptions.Immediate);
                                    }
                                    cues.Remove(key);
                                }
                            }
                        }
                        else if (buff != null)
                        {
                            if (buff.totalMillisecondsDuration <= 50) // present persistant buff
                            {
                                farmerBuffs.Value.Remove(key); // no effect
                            }
                            else 
                            {
                                Game1.buffsDisplay.removeOtherBuff(buff.which);
                                buff = null;
                            }
                        }
                        if(buff == null)
                        {
                            Game1.buffsDisplay.addOtherBuff(fb);
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.doneEating))]
        public class Farmer_doneEating_Patch
        {
            public static void Prefix(Farmer __instance)
            {
                if (!Config.ModEnabled || !__instance.IsLocalPlayer || farmerBuffs.Value is null)
                    return;
                foreach (var kvp in buffDict)
                {
                    int duration = 50;
                    var dataDict = kvp.Value;
                    if (!dataDict.TryGetValue("consume", out var food))
                        continue;
                    if (!Game1.player.isEating || Game1.player.itemToEat is not Object || (Game1.player.itemToEat as Object).Name != (string)food)
                        continue;
                    if (dataDict.TryGetValue("duration", out var dur))
                    {
                        duration = GetInt(dur) * 1000;
                    }
                    Buff buff = CreateBuff(kvp.Key, kvp.Value, food, duration);
                    foreach(var key in farmerBuffs.Value.Keys.ToArray())
                    {
                        Buff oldBuff = farmerBuffs.Value[key];
                        if(oldBuff.which == buff.which)
                        {
                            if(oldBuff.totalMillisecondsDuration > 50)
                            {
                                Game1.buffsDisplay.removeOtherBuff(oldBuff.which);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    farmerBuffs.Value[kvp.Key] = buff;
                }
            }
        }
        [HarmonyPatch(typeof(Farmer), "farmerInit")]
        public class Farmer_farmerInit_Patch
        {
            public static void Postfix(Farmer __instance)
            {
                __instance.hat.fieldChangeEvent += Hat_fieldChangeEvent;
                __instance.shirtItem.fieldChangeEvent += ShirtItem_fieldChangeEvent;
                __instance.pantsItem.fieldChangeEvent += PantsItem_fieldChangeEvent;
                __instance.boots.fieldChangeEvent += Boots_fieldChangeEvent;
                __instance.leftRing.fieldChangeEvent += LeftRing_fieldChangeEvent;
                __instance.rightRing.fieldChangeEvent += RightRing_fieldChangeEvent;
            }

            public static void RightRing_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Ring> field, StardewValley.Objects.Ring oldValue, StardewValley.Objects.Ring newValue)
            {
                UpdateBuffs();
            }

            public static void LeftRing_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Ring> field, StardewValley.Objects.Ring oldValue, StardewValley.Objects.Ring newValue)
            {
                UpdateBuffs();
            }

            public static void Boots_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Boots> field, StardewValley.Objects.Boots oldValue, StardewValley.Objects.Boots newValue)
            {
                UpdateBuffs();
            }

            public static void PantsItem_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Clothing> field, StardewValley.Objects.Clothing oldValue, StardewValley.Objects.Clothing newValue)
            {
                UpdateBuffs();
            }

            public static void ShirtItem_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Clothing> field, StardewValley.Objects.Clothing oldValue, StardewValley.Objects.Clothing newValue)
            {
                UpdateBuffs();
            }

            public static void Hat_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Hat> field, StardewValley.Objects.Hat oldValue, StardewValley.Objects.Hat newValue)
            {
                UpdateBuffs();
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
                    if (kvp.Value.which == __instance.which)
                    {
                        object texturePath = null;
                        object description = null;
                        var hasTex = buffDict[kvp.Key].TryGetValue("texturePath", out texturePath);
                        var hasDesc = buffDict[kvp.Key].TryGetValue("description", out description);
                        if (hasTex || hasDesc)
                        {
                            Texture2D tex = (__result.Any() ? __result[0].texture : null);
                            Rectangle sourceRect = __instance.sheetIndex > -1 && __result.Any() ? __result[0].sourceRect : new Rectangle();
                            float scale = 4;
                            if (texturePath is not null)
                            {
                                tex = SHelper.GameContent.Load<Texture2D>((string)texturePath);
                                var sourceX = buffDict[kvp.Key].TryGetValue("textureX", out var x) ? GetInt(x) : 0;
                                var sourceY = buffDict[kvp.Key].TryGetValue("textureY", out var y) ? GetInt(y) : 0;
                                var sourceW = buffDict[kvp.Key].TryGetValue("textureWidth", out var w) ? GetInt(w) : tex.Width;
                                var sourceH = buffDict[kvp.Key].TryGetValue("textureHeight", out var h) ? GetInt(h) : tex.Height;
                                sourceRect = new Rectangle(sourceX, sourceY, sourceW, sourceH);
                                if(buffDict[kvp.Key].TryGetValue("textureScale", out var s))
                                    scale = GetFloat(s);
                            }
                            var cc = new ClickableTextureComponent("", Rectangle.Empty, null, description is not null ? (string)description + "\n" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.508") + __instance.displaySource : (__result.Any() ? __result[0].hoverText : null), tex, sourceRect, scale, false);

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
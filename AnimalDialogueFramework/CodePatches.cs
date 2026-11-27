using HarmonyLib;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace AnimalDialogueFramework
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Horse), nameof(Horse.canTalk))]
        public class Horse_canTalk_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                __result = true;
                return false;
            }

        }
        [HarmonyPatch(typeof(Horse), nameof(Horse.checkAction))]
        public class Horse_checkAction_Patch
        {
            public static bool Prefix(Horse __instance, ref bool __result, Farmer who, GameLocation l)
            {
                if (!Config.ModEnabled || __instance.CurrentDialogue is null || __instance.CurrentDialogue.Count == 0 || __instance.Name.Length <= 0)
                    return true;
                var method = typeof(NPC).GetMethod("checkAction");
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Func<Farmer, GameLocation, bool>)Activator.CreateInstance(typeof(Func<Farmer, GameLocation, bool>), __instance, ftn);
                __result = func.Invoke(who, l);
                return false;
            }

        }
        [HarmonyPatch(typeof(Junimo), nameof(Junimo.canTalk))]
        public class Junimo_canTalk_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                __result = true;
                return false;
            }

        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.canTalk))]
        public class Pet_canTalk_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                __result = true;
                return false;
            }

        }
        [HarmonyPatch(typeof(Pet), nameof(Pet.checkAction))]
        public class Pet_checkAction_Patch
        {
            public static bool Prefix(Pet __instance, ref bool __result, Farmer who, GameLocation l)
            {
                if (!Config.ModEnabled || __instance.CurrentDialogue is null || __instance.CurrentDialogue.Count == 0 || __instance.Name.Length <= 0)
                    return true;
                var method = typeof(NPC).GetMethod("checkAction");
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Func<Farmer, GameLocation, bool>)Activator.CreateInstance(typeof(Func<Farmer, GameLocation, bool>), __instance, ftn);
                __result = func.Invoke(who, l);
                return false;
            }

        }
        [HarmonyPatch(typeof(Child), nameof(Child.checkAction))]
        public class Child_checkAction_Patch
        {
            public static bool Prefix(Child __instance, ref bool __result, Farmer who, GameLocation l)
            {
                if (!Config.ModEnabled || __instance.CurrentDialogue is null || __instance.CurrentDialogue.Count == 0)
                    return true;
                var method = typeof(NPC).GetMethod("checkAction");
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Func<Farmer, GameLocation, bool>)Activator.CreateInstance(typeof(Func<Farmer, GameLocation, bool>), __instance, ftn);
                __result = func.Invoke(who, l);
                return false;
            }

        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GameLocation.checkAction");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(Character), nameof(Character.IsMonster)))
                    {
                        SMonitor.Log($"adding check for monsters allowed");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckMonster))));
                    }
                }

                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.Dialogue))]
        [HarmonyPatch(MethodType.Getter)]
        public class NPC_Dialogue_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling NPC.Dialogue");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Isinst)
                    {
                        SMonitor.Log($"adding check for {codes[i].operand}");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckType))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
        }


        [HarmonyPatch(typeof(NPC), nameof(NPC.getTextureName))]
        public class NPC_getTextureName_Patch
        {
            public static bool Prefix(NPC __instance, ref string __result)
            {
                if (!Config.ModEnabled || !IsAnimal(__instance))
                    return true;
                if (genericPortraitList.TryGetValue(__instance, out var b))
                {
                    __result = b ? __instance.GetType().Name : __instance.Name;
                    return false;
                }
                try
                {
                    Game1.content.Load<Texture2D>("Portraits\\" + __instance.Name);
                    genericPortraitList.Add(__instance, false);
                    __result = __instance.Name;
                }
                catch
                {
                    genericPortraitList.Add(__instance, true);
                    __result = __instance.GetType().Name;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(NPC), nameof(NPC.GetDialogueSheetName))]
        public class NPC_GetDialogueSheetName_Patch
        {
            public static bool Prefix(NPC __instance, ref string __result)
            {
                if (!Config.ModEnabled || !IsAnimal(__instance))
                    return true;
                if (genericDialogueList.TryGetValue(__instance, out var b))
                {
                    __result = b ? __instance.GetType().Name : __instance.Name;
                    return false;
                }
                try
                {
                    Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + __instance.Name);
                    genericDialogueList.Add(__instance, false);
                    __result = __instance.Name;
                }
                catch
                {
                    genericDialogueList.Add(__instance, true);
                    __result = __instance.GetType().Name;
                }
                return false;
            }
        }
    }
}
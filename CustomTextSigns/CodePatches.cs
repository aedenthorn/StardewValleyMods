using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace CustomTextSigns
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.IsTextSign))]
        public static class Object_IsTextSign_Patch
        {
            public static void Postfix(Object __instance, ref bool __result)
            {
                if (!Config.ModEnabled || __result || SHelper.Input.IsDown(Config.SuppressKey))
                    return;
                __result = __instance.GetContextTags().Contains("text_sign");
            }
        }
        [HarmonyPatch(typeof(Sign), nameof(Sign.checkForAction))]
        public static class Sign_CheckForAction_Patch
        {
            public static bool Prefix(Sign __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
            {
                if (!Config.ModEnabled || SHelper.Input.IsDown(Config.SuppressKey) || !__instance.GetContextTags().Contains("text_sign"))
                    return true;
                __result = (bool)AccessTools.Method(typeof(Object), "CheckForActionOnTextSign").Invoke(__instance, new object[] { who, justCheckingForActivity });
                return !__result;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Object_Draw_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) 
            {
                SMonitor.Log($"Transpiling Object.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(SpriteText), nameof(SpriteText.drawSmallTextBubble)))
                    {
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawSmallTextBubble));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Object), "CheckForActionOnTextSign")]
        public static class Object_CheckForActionOnTextSign_Patch
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) 
            {
                SMonitor.Log($"Transpiling Object.CheckForActionOnTextSign");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == 60)
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetMaxLength))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }

    }
}
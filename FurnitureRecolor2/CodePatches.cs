using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace FurnitureRecolor
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(SliderBar), nameof(SliderBar.click))]
        public static class SliderBar_click_Patch
        {
            public static bool Prefix(SliderBar __instance, int x, int y, ref int __result)
            {
                if (!Config.ModEnabled || !Config.FixSliderBar)
                {
                    return true;
                }
                if (__instance.bounds.Contains(x, y))
                {
                    float fx = x - __instance.bounds.X;
                    __instance.value = (int)Math.Ceiling(fx / __instance.bounds.Width * 100f);
                }
                __result = __instance.value;
                return false;
            }
        }
        [HarmonyPatch(typeof(ColorPicker), nameof(ColorPicker.click))]
        public static class ColorPicker_click_Patch
        {
            public static void Prefix(ColorPicker __instance, Rectangle ___bounds, ref int x)
            {
                if (!Config.ModEnabled || !Config.FixSliderBar)
                {
                    return;
                }
                if(x < ___bounds.X && ___bounds.X - x < 8)
                {
                    x = ___bounds.X;
                }
            }
        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public static class Furniture_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Furniture.draw");
                var codes = new List<CodeInstruction>(instructions);
                var count = 0;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }))
                    {
                        SMonitor.Log($"adding check for color {count}");
                        var code = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckTexture)));
                        var code2 = new CodeInstruction(OpCodes.Ldarg_0);
                        code2.MoveLabelsFrom(codes[i]);
                        codes[i] = code;
                        codes.Insert(i, code2);
                        i++;

                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool), })]
        public static class Furniture_drawInMenu_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Furniture.drawInMenu");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw), new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }))
                    {
                        SMonitor.Log($"adding check for color");
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(CheckTexture));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}
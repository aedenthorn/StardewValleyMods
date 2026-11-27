using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomSplashScreen
{
	public partial class ModEntry
    {
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.draw))]
        public class TitleMenu_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling TitleMenu.draw");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 8; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(Game1), nameof(Game1.staminaRect)) && codes[i + 8].opcode == OpCodes.Call && codes[i + 8].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)))
                    {
                        SMonitor.Log($"adding method to change background color");
                        codes.Insert(i + 9, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeColor))));
                        codes.Insert(i + 9, new CodeInstruction(OpCodes.Ldarg_1));
                    }
                }

                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.receiveLeftClick))]
        public class TitleMenu_receiveLeftClick_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling TitleMenu.receiveLeftClick");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Random), nameof(Random.NextDouble)) && codes[i + 1].opcode == OpCodes.Ldc_R8 && (double)codes[i + 1].operand == 0.02)
                    {
                        SMonitor.Log($"adding method to change alt surprise background chance");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeAltSurpriseChance))));
                    }
                }

                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.update))]
        public class TitleMenu_update_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling TitleMenu.update");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand is string str && str == "MainTheme")
                    {
                        SMonitor.Log($"adding method to change music");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeMusic))));
                    }
                }

                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.skipToTitleButtons))]
        public class TitleMenu_skipToTitleButtons_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling TitleMenu.skipToTitleButtons");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand is string str && str == "MainTheme")
                    {
                        SMonitor.Log($"adding method to change music");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeMusic))));
                    }
                }

                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(TitleMenu), new Type[0])]
        [HarmonyPatch(MethodType.Constructor)]
        public class TitleMenu_Patch
        {
            public static void Postfix()
            {
                if(Config.ModEnabled && Config.StartMusicAtSplash && !string.IsNullOrEmpty(Config.MenuMusic))
                {
                    Game1.changeMusicTrack(Config.MenuMusic, false, MusicContext.Default);
                }
            }

        }
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.receiveKeyPress))]
        public class TitleMenu_receiveKeyPress_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling TitleMenu.receiveKeyPress");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand is string str && str == "MainTheme")
                    {
                        SMonitor.Log($"adding method to change music");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ChangeMusic))));
                    }
                }

                return codes.AsEnumerable();
            }

        }
    }
}

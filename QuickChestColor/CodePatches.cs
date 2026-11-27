using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using System;
using System.Reflection.Emit;
using System.Reflection;

namespace QuickChestColor
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Chest), nameof(Chest.draw), [typeof(SpriteBatch),typeof(int),typeof(int),typeof(float)])]
        public static class Chest_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Chest.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Chest), nameof(Chest.playerChoiceColor)))
                    {
                        SMonitor.Log("Overriding playerChoiceColor");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(GetPlayerChoiceColor))));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }

    }
}

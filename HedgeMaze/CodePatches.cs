using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace HedgeMaze
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories))]
        public class FarmerRenderer_drawHairAndAccesories_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FarmerRenderer.drawHairAndAccesories");
                var codes = new List<CodeInstruction>(instructions);
                CodeInstruction texture = null;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i > 0 && i < codes.Count -1 && codes[i].opcode == OpCodes.Ldarg_S && (byte)codes[i].operand == 4 && codes[i + 1].opcode == OpCodes.Ldarg_S && (byte)codes[i + 1].operand == 5)
                    {
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawAboveAlwaysFrontLayer))]
        public class GameLocation_drawAboveAlwaysFrontLayer_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
            }
        }
    }
}
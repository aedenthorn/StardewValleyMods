using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace AdvancedFluteBlocks
{
    public partial class ModEntry
    {
        public static IEnumerable<CodeInstruction> Object_FluteBlock_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Object");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Game1), nameof(Game1.soundBank)) && codes[i + 1].opcode == OpCodes.Ldstr && (string)codes[i + 1].operand == "flute")
                {
                    SMonitor.Log("Overriding get flute cue");
                    codes[i].opcode = OpCodes.Ldarg_0;
                    codes[i].operand = null;
                    codes[i + 2].opcode = OpCodes.Call;
                    codes[i + 2].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FluteCueOverride));
                }
            }

            return codes.AsEnumerable();
        }
        public static bool Game1_pressSwitchToolButton_Prefix()
        {
            return !Config.EnableMod || (!SHelper.Input.IsDown(Config.PitchModKey) && !SHelper.Input.IsDown(Config.ToneModKey) || !Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out Object obj) || !obj.Name.Equals("Flute Block"));
        }

        private static ICue FluteCueOverride(Object obj, string flute)
        {
            if(Config.EnableMod && obj.modData.TryGetValue("aedenthorn.AdvancedFluteBlocks/tone", out string tone))
            {
                return Game1.soundBank.GetCue(tone);
            }
            return Game1.soundBank.GetCue(flute);
        }
    }
}
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TrashCanReactions
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{

		public static ModConfig Config;
        private static IMonitor PMonitor;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			if (!Config.Enabled)
				return;

			PMonitor = Monitor;

			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

			harmony.Patch(
				original: AccessTools.Method(typeof(Town), nameof(Town.checkAction)),
			   transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Town_checkAction_transpiler))
			);
		}
        public static IEnumerable<CodeInstruction> Town_checkAction_transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions); 
            bool startLooking = false;
            bool stopLooking = false;
            int emote = Config.LinusEmote;
            int points = Config.LinusPoints;
            for (int i = 0; i < codes.Count; i++)
            {
                if (startLooking)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_S && codes[i+2].opcode == OpCodes.Ldc_I4_1 && codes[i+3].opcode == OpCodes.Callvirt)
                    {
                        PMonitor.Log($"changing emote from {codes[i + 1].opcode}:{codes[i + 1].operand} to {emote}");
                        codes[i+1] = new CodeInstruction(OpCodes.Ldc_I4_S, emote);
                    }
                    else if (codes[i].opcode == OpCodes.Ldarg_3 && codes[i+2].opcode == OpCodes.Ldloc_S && codes[i+3].opcode == OpCodes.Isinst)
                    {
                        PMonitor.Log($"changing friendship from {codes[i+1].opcode}:{codes[i + 1].operand} to {points}");
                        codes[i+1] = new CodeInstruction(OpCodes.Ldc_I8, points);
                        if (stopLooking)
                            break;
                    }
                    else if (codes[i].operand as string == "Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus")
                    {
                        PMonitor.Log($"got dialogue string {codes[i].operand}!");
                        codes[i].operand = Config.LinusDialogue;
                        emote = Config.ChildEmote;
                        points = Config.ChildPoints;
                    }
                    else if (codes[i].operand as string == "Data\\ExtraDialogue:Town_DumpsterDiveComment_Child")
                    {
                        PMonitor.Log($"got dialogue string {codes[i].operand}!");
                        codes[i].operand = Config.ChildDialogue;
                        emote = Config.TeenEmote;
                        points = Config.TeenPoints;
                    }
                    else if (codes[i].operand as string == "Data\\ExtraDialogue:Town_DumpsterDiveComment_Teen")
                    {
                        PMonitor.Log($"got dialogue string {codes[i].operand}!");
                        codes[i].operand = Config.TeenDialogue;
                        emote = Config.AdultEmote;
                        points = Config.AdultPoints;
                    }
                    else if (codes[i].operand as string == "Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult")
                    {
                        PMonitor.Log($"got dialogue string {codes[i].operand}!");
                        codes[i].operand = Config.AdultDialogue;
                        stopLooking = true;
                    }
                }
                else if ((codes[i].operand as string) == "TrashCan")
                {
                    PMonitor.Log($"got string 'TrashCan'!");
                    startLooking = true;
                }
            }

            return codes.AsEnumerable();
        }
    }
}
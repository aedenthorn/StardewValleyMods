using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace TrashCanReactions
{
    public class ModEntry : Mod
    {

        public static ModConfig Config;
        private static IMonitor PMonitor;
        private static IModHelper PHelper;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.Enabled)
                return;

            PMonitor = Monitor;
            PHelper = Helper;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Town), nameof(Town.checkAction)),
                transpiler: new HarmonyMethod(typeof(ModEntry), nameof(Town_checkAction_transpiler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.isThereAFarmerOrCharacterWithinDistance)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Utility_isThereAFarmerOrCharacterWithinDistance_postfix))
            );
        }

        public static void Utility_isThereAFarmerOrCharacterWithinDistance_postfix(ref Character __result, Vector2 tileLocation, int tilesAway, GameLocation environment)
        {
            if (!Config.Enabled || __result == null || !(__result is NPC) || __result is Horse || !(environment is Town) || !Config.SpecificCharacters.ContainsKey(__result.name))
            {
                return;
            }
            string s = environment.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Action", "Buildings");
            int whichCan = (s != null) ? Convert.ToInt32(s.Split(' ')[1]) : -1;
            NetArray<bool, NetBool> garbageChecked = PHelper.Reflection.GetField<NetArray<bool, NetBool>>(environment, "garbageChecked").GetValue();
            if (whichCan >= 0 && whichCan < garbageChecked.Length)
            {
                PMonitor.Log($"trash voyer");
                Reactor r = Config.SpecificCharacters[__result.name];
                __result.doEmote(r.emote, true);
                (__result as NPC).setNewDialogue(Game1.content.LoadString(r.dialogue), true, true);
                Game1.player.changeFriendship(r.points, __result as NPC);
                Game1.drawDialogue(__result as NPC);
                __result = null;
            }
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
                        codes[i+1] = new CodeInstruction(OpCodes.Ldc_I4, points);
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
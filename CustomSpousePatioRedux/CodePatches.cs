using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Layers;
using xTile.Tiles;

namespace CustomSpousePatioRedux
{
    public partial class ModEntry
    {


        public static bool Farm_CacheOffBasePatioArea_Prefix(Farm __instance)
        {
            if (!Config.EnableMod)
                return true;

            baseSpouseAreaTiles = new Dictionary<string, Dictionary<string, Dictionary<Point, Tile>>>();
            CacheOffBasePatioArea("default", __instance, __instance.GetSpouseOutdoorAreaCorner());

            if (outdoorAreas == null || outdoorAreas.dict.Count == 0)
                return false;

            foreach(var data in outdoorAreas.dict)
            {
                CacheOffBasePatioArea(data.Key);
            }

            return false;
        }

        public static bool Farm_ReapplyBasePatioArea_Prefix(Farm __instance)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.dict.Count == 0)
                return true;
            if (addingExtraAreas)
                return false;

            foreach (var kvp in baseSpouseAreaTiles)
            {
                ReapplyBasePatioArea(kvp.Key);
            }
            return false;
        }
        public static IEnumerable<CodeInstruction> Farm_addSpouseOutdoorArea_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Farm.addSpouseOutdoorArea");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 0 && codes[i - 1].opcode == OpCodes.Ldarg_0 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Farm), nameof(Farm.GetSpouseOutdoorAreaCorner)))
                {
                    SMonitor.Log("Overriding Farm.GetSpouseOutdoorAreaCorner");
                    codes[i - 1] = new CodeInstruction(OpCodes.Ldarg_1);
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetSpouseOutdoorAreaCorner)));
                }
                else if (i < codes.Count - 15 && codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(GameLocation), nameof(GameLocation.ApplyMapOverride), new System.Type[] {typeof(string), typeof(string), typeof(Rectangle?), typeof(Rectangle?) }))
                {
                    SMonitor.Log("Overriding GameLocation.ApplyMapOverride");
                    codes[i - 15].opcode = OpCodes.Ldarg_1;
                    codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ApplyMapOverride));
                }
            }

            return codes.AsEnumerable();
        }

        public static bool addingExtraAreas = false;

        public static void Farm_addSpouseOutdoorArea_Postfix(Farm __instance, string spouseName)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.dict.Count == 0 || spouseName == "" || spouseName == null)
                return;
            spousePositions[spouseName] = __instance.spousePatioSpot;
            if (addingExtraAreas)
                return;
            addingExtraAreas = true;
            foreach(var name in outdoorAreas.dict.Keys)
            {
                if(name != spouseName)
                    __instance.addSpouseOutdoorArea(name);
            }
            addingExtraAreas = false;
        }

        
        public static bool NPC_setUpForOutdoorPatioActivity_Prefix(NPC __instance)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.dict.Count == 0 || !outdoorAreas.dict.ContainsKey(__instance.Name))
            {
                if(Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && Game1.MasterPlayer.spouse != __instance.Name)
                {
                    SMonitor.Log($"preventing {__instance.Name} from going to spouse patio");
                    return false;
                }
                return true;
            }

            Vector2 patio_location = __instance.GetSpousePatioPosition();
            if (NPC.checkTileOccupancyForSpouse(Game1.getLocationFromName(outdoorAreas.dict[__instance.Name].location), patio_location, ""))
            {
                return false;
            }
            Game1.warpCharacter(__instance, outdoorAreas.dict[__instance.Name].location, patio_location);
            __instance.popOffAnyNonEssentialItems();
            __instance.currentMarriageDialogue.Clear();
            __instance.addMarriageDialogue("MarriageDialogue", "patio_" + __instance.Name, false, new string[0]);
            __instance.Schedule = new Dictionary<int, SchedulePathDescription>();
            __instance.setTilePosition((int)patio_location.X, (int)patio_location.Y);
            __instance.shouldPlaySpousePatioAnimation.Value = true;

            return false;
        }
        public static bool NPC_GetSpousePatioPosition_Prefix(NPC __instance, ref Vector2 __result)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.dict.Count == 0 || !spousePositions.ContainsKey(__instance.Name))
                return true;
            __result = spousePositions[__instance.Name].ToVector2();
            return false;
        }
    }
}
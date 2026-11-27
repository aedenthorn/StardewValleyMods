using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

namespace FarmSwitch
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Farm), new Type[] { typeof(string), typeof(string) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class Farm_Patch
        {
            public static void Postfix(string mapPath, string name)
            {
                if (!Config.EnableMod || name != "Farm")
                    return;

                foreach (var f in GetFarms())
                {
                    string path = "Maps\\" + f;
                    if (path != mapPath)
                    {
                        Farm farm = new Farm(path, "FarmSwitch_"+f);
                        Game1.locations.Add(farm);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Farm), nameof(Farm.DayUpdate))]
        public class Farm_DayUpdate_Patch
        {
            public static bool Prefix(Farm __instance)
            {
                return !Config.EnableMod || !__instance.Name.StartsWith("FarmSwitch_");
            }
        }
        [HarmonyPatch(typeof(Farm), nameof(Farm.updateEvenIfFarmerIsntHere))]
        public class Farm_updateEvenIfFarmerIsntHere_Patch
        {
            public static bool Prefix(Farm __instance)
            {
                return !Config.EnableMod || !__instance.Name.StartsWith("FarmSwitch_");
            }
        }
        [HarmonyPatch(typeof(Farm), nameof(Farm.checkAction))]
        public class Farm_checkAction_Patch
        {
            public static bool Prefix(Farm __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
            {
                if (!Config.EnableMod)
                    return true;
                int tileIndex = __instance.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet");
                if(tileIndex == 835)
                {
                    List<Response> responses = new();
                    foreach(var f in GetFarms())
                    {
                        responses.Add(new Response("FarmSwitch_" + f, f));
                    }
                    responses.Add(new Response("cancel", SHelper.Translation.Get("cancel")));
                    Game1.player.currentLocation.createQuestionDialogue(SHelper.Translation.Get("which-farm"), responses.ToArray(), "FarmSwitch_Which");
                    
                    return false;
                }
                return true;
            }
        }
    }
}
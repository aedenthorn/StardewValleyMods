using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using xTile.Dimensions;

namespace SewerSlimes
{
    public partial class ModEntry
    { 
        [HarmonyPatch(typeof(Monster), nameof(Monster.behaviorAtGameTick))]
        public class Monster_behaviorAtGameTick_Patch
        {
            public static void Postfix(Monster __instance)
            {
                if(!Config.ModEnabled)
                    return;
                if(__instance is GreenSlime && (int)AccessTools.Field(typeof(GreenSlime), "readyToJump").GetValue(__instance) != -1 && __instance.Player is not null && __instance.Player.currentLocation.map.GetLayer("Back").Tiles[__instance.Player.getTileX(), __instance.Player.getTileY()] != null && __instance.Player.currentLocation.map.GetLayer("Back").Tiles[__instance.Player.getTileX(), __instance.Player.getTileY()].Properties.ContainsKey("NPCBarrier"))
                {
                    AccessTools.Field(typeof(GreenSlime), "readyToJump").SetValue(__instance, -1);
                }
            }
        }
    }
}
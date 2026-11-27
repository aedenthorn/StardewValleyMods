using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Locations;
using Object = StardewValley.Object;

namespace PortableBasements
{
	public partial class ModEntry
    {

        internal static void Object_placementAction_Postfix(Object __instance, GameLocation location, int x, int y, Farmer who, bool __result)
        {
            if (!Config.ModEnabled || !__result || who is null || __instance.Name != ladderDownKey)
                return;
            Game1.activeClickableMenu = new PortableBasementMenu(__instance, location, x, y);
        }
        internal static void Object_performRemoveAction_Postfix(Object __instance)
        {
            if (!Config.ModEnabled ||__instance.Name != ladderDownKey || !__instance.modData.TryGetValue(modKey, out var dataString))
                return;
            var data = dataString.Split(',');
            var loc = Game1.locations.FirstOrDefault(l => l.Name == data[0]);
            if (loc is null)
            {
                SMonitor.Log($"Destination map {data[0]} no longer exists");
                return;
            }
            loc.removeObject(new(int.Parse(data[1]), int.Parse(data[2])), false);
        }
        internal static void Object_checkForAction_Postfix(Object __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            if (!Config.ModEnabled || who is null || justCheckingForActivity)
                return;

            if((__instance.Name == ladderDownKey ||__instance.Name == ladderUpKey) && __instance.modData.TryGetValue(modKey, out var dataString))
            {

                var data = dataString.Split(',');
                var loc = Game1.locations.FirstOrDefault(l => l.Name == data[0]);
                if (loc is null)
                {
                    SMonitor.Log($"Destination map {data[0]} no longer exists");
                    Game1.showRedMessage(SHelper.Translation.Get("DestinationMissing"), true);
                    return;
                }
                if(!loc.Objects.TryGetValue(new Vector2(int.Parse(data[1]), int.Parse(data[2])), out var obj) || !obj.modData.ContainsKey(modKey))
                {
                    Game1.showRedMessage(SHelper.Translation.Get("LadderMissing"), true);
                    who.currentLocation.removeObject(__instance.TileLocation, __instance.Name == ladderDownKey);
                }
                Game1.warpFarmer(data[0], int.Parse(data[1]), int.Parse(data[2]) + 1, 2);
                Game1.player.temporarilyInvincible = true;
                Game1.player.temporaryInvincibilityTimer = 0;
                Game1.player.flashDuringThisTemporaryInvincibility = false;
                Game1.player.currentTemporaryInvincibilityDuration = 1000;
                var x = Game1.xLocationAfterWarp;
                Game1.player.currentLocation.playSound(__instance.Name == ladderDownKey ? "stairsdown" : "stairsdown");
                Game1.player.currentLocation = loc;
                __result = true;
            }
        }
    }
}

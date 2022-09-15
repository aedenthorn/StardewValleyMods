using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MapTeleport
{
    public partial class ModEntry
    {
 
        [HarmonyPatch(typeof(MapPage), nameof(MapPage.receiveLeftClick))]
        public class MapPage_receiveLeftClick_Patch
        {
            public static bool Prefix(MapPage __instance, int x, int y)
            {
                if (!Config.EnableMod)
                    return true;
                foreach (ClickableComponent c in __instance.points)
                {
                    if (c.containsPoint(x, y))
                    {
                        var coordinates = SHelper.GameContent.Load<CoordinatesList>(dictPath);
                        Coordinates co = coordinates.coordinates.Find(o => o.id == c.myID);
                        if (co == null)
                        {
                            SMonitor.Log($"Teleport location {c.myID} {c.name} not found!", LogLevel.Warn);
                            break;
                        }
                        SMonitor.Log($"Teleporting to {c.name} ({c.myID}), {co.mapName}, {co.x},{co.y}", LogLevel.Debug);
                        Game1.activeClickableMenu?.exitThisMenu(true);
                        Game1.warpFarmer(co.mapName, co.x, co.y, false);
                        return false;
                    }
                }
                return true;
            }
        }
   }
}
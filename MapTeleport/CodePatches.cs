using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;

namespace MapTeleport
{
    public partial class ModEntry
    {
        protected static CoordinatesList addedCoordinates;
        public static bool CheckClickableComponents(List<ClickableComponent> components, int topX, int topY, int x, int y)
        {
            if (!Config.ModEnabled)
                return false;
            if (addedCoordinates == null)
            {
                addedCoordinates = SHelper.Data.ReadJsonFile<CoordinatesList>("coordinates.json");
                if (addedCoordinates == null) addedCoordinates = new CoordinatesList();
            }
            var coordinates = SHelper.GameContent.Load<CoordinatesList>(dictPath);
            bool added = false;
            bool found = false;
            // Sort boundries so that the function will warp to the smallest overlapping area
            components.Sort(delegate (ClickableComponent a, ClickableComponent b)
            {
                return (a.bounds.Height * a.bounds.Width).CompareTo(b.bounds.Height * b.bounds.Width);
            });
            foreach (ClickableComponent c in components)
            { 
                // Offset so that this works for any screen size
                string altId = $"{c.bounds.X - topX}.{c.bounds.Y - topY}";
                Predicate<Coordinates> findMatch = (o) => o.id == c.myID || (c.myID == ClickableComponent.ID_ignore && o.altId == altId);
                Coordinates co = coordinates.coordinates.Find(findMatch);
                if (co == null)
                {
                    co = addedCoordinates.coordinates.Find(findMatch);
                    if (co == null)
                    {
                        if (c.myID == ClickableComponent.ID_ignore)
                        {
                            addedCoordinates.Add(new Coordinates() { name = c.name, altId = altId, enabled = false });
                        }
                        else
                        {
                            addedCoordinates.Add(new Coordinates() { name = c.name, id = c.myID, enabled = false });
                        }
                        SMonitor.Log($"Added: {{ \"name\":\"{c.name}\", \"id\":{c.myID}, \"altId\":\"{altId}\" }}", LogLevel.Trace);
                        added = true;
                    }
                    // else check if the coordinate is enabled
                }

                if (c.containsPoint(x, y) && co.enabled)
                {
                    SMonitor.Log($"Teleporting to {c.name} ({(co.altId != null ? co.altId : co.id)}), {co.mapName}, {co.x},{co.y}", LogLevel.Debug);
                    Game1.activeClickableMenu?.exitThisMenu(true);
                    Game1.warpFarmer(co.mapName, co.x, co.y, false);
                    found = true;
                    break;
                }
            }
            if (added)
            {
                SHelper.Data.WriteJsonFile("coordinates.json", addedCoordinates);
            }
            return found;

        }

        [HarmonyPatch(typeof(MapPage), nameof(MapPage.receiveLeftClick))]
        public class MapPage_receiveLeftClick_Patch
        {
            public static bool Prefix(MapPage __instance, int x, int y)
            {
                
                bool found = CheckClickableComponents(__instance.points, __instance.xPositionOnScreen, __instance.yPositionOnScreen, x, y);
                return !found;
            }
        }

        [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.receiveLeftClick))]
        public class RSVMapPage_receiveLeftClick_Patch
        {
            public static bool Prefix(IClickableMenu __instance, int x, int y)
            {

                bool found = false;
                if (__instance.allClickableComponents != null && __instance.GetType().Name == "RSVWorldMap")
                {
                    // RSV uses component x,y's that are not offset, however they need to be offset to check for the mouse position
                    found = CheckClickableComponents(__instance.allClickableComponents, 0, 0, x - __instance.xPositionOnScreen, y - __instance.yPositionOnScreen);
                }
                return !found;
            }
        }
    }
}
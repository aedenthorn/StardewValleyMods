using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.HomeRenovations;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace Renovations
{
    public class CodePatches
    {
        public static void HouseRenovation_GetAvailableRenovations_Postfix(List<ISalable> __result)
        {
            if (!ModEntry.config.EnableMod || !Game1.player.IsMainPlayer)
                return;

            Dictionary<string, HomeRenovation> data = ModEntry.PHelper.Content.Load<Dictionary<string, HomeRenovation>>("Data/HomeRenovations", ContentSource.GameContent);

            foreach (ISalable i in __result)
            {
                HouseRenovation hr = i as HouseRenovation;
                
                foreach(RenovationValue rv in data[hr.Name].Requirements)
                {
                    if(rv.Type == "Mod" && rv.Key == "GameLocation")
                    {
                        GameLocation gl = Game1.getLocationFromName(rv.Value);
                        if (gl != null)
                        {
                            hr.location = gl;
                            ModEntry.PMonitor.Log($"Switching renovation {hr.Name} location to {gl.Name}");
                        }
                    }
                }
            }
        }
        public static void GameLocation_MakeMapModifications_Postfix(GameLocation __instance)
        {
            if (!ModEntry.config.EnableMod || !Game1.player.IsMainPlayer)
                return;

            Dictionary<string, CustomRenovationData> renovationDict = ModEntry.PHelper.Content.Load<Dictionary<string, CustomRenovationData>>("CustomRenovations", ContentSource.GameContent);

            ModEntry.PMonitor.Log($"Checking {renovationDict.Count} renovations with {__instance.Name}");
            foreach (var kvp in renovationDict)
            {
                ModEntry.PMonitor.Log($"Checking {kvp.Key} location {kvp.Value.gameLocation} with {__instance.Name}; mail: {Game1.player.mailReceived.Contains(kvp.Key)}");
                if (kvp.Value.gameLocation == __instance.Name && Game1.player.mailReceived.Contains(kvp.Key))
                {
                    ModEntry.PMonitor.Log($"Adding renovation {kvp.Key} to {__instance.Name}");
                    __instance.ApplyMapOverride(kvp.Value.mapPath, kvp.Value.SourceRect(), kvp.Value.SourceRect());
                }
            }
        }
        public static bool Farmer_performRenovation_Prefix(Farmer __instance, string location_name)
        {
            if (!ModEntry.config.EnableMod || !__instance.IsMainPlayer)
                return true;

            GameLocation location = Game1.getLocationFromName(location_name);

            if (location is FarmHouse)
                return true;

            ModEntry.PMonitor.Log($"Applying renovations to {location_name}");

            location.MakeMapModifications(true);

            return false;
        }
    }
}
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace FarmerHelper
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool Utility_tryToPlaceItem_Prefix(GameLocation location, Item item, int x, int y, ref bool __result )
        {
            if (!Config.EnableMod || !Config.PreventLatePlant || item is not Object obj || (new int[] { 495, 496, 497, 498, 770 }).Contains(obj.ParentSheetIndex) || obj.Category != -74)
                return true;
            if (location.SeedsIgnoreSeasonsHere())
                return true;
            Vector2 tileLocation = new Vector2((float)(x / 64), (float)(y / 64));
            if (!location.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature f) || f is not HoeDirt)
                return true;
            Crop c = new Crop(item.ItemId, 0, 0, location);
            if (c == null)
                return true;
            if (c.phaseDays.Count == 0 || EnoughDaysLeft(c, f as HoeDirt))
                return true;
            __result = false;
            Game1.showRedMessage(string.Format(SHelper.Translation.Get("too-late-message"), item.Name));
            return false;
        }
        private static bool Object_placementAction_Prefix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            if (!Config.EnableMod || !Config.PreventLatePlant || __instance.Category != -74)
                return true;

            Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));

            if (!location.terrainFeatures.TryGetValue(placementTile, out TerrainFeature f) || f is not HoeDirt)
                return true;

            if ((new int[] { 495, 496, 497, 498, 770 }).Contains(__instance.ParentSheetIndex))
                return true;

            if (location.SeedsIgnoreSeasonsHere())
                return true;

            Crop c = new Crop(__instance.ItemId, x, y, location);
            if (c == null)
                return true;
            if (c.phaseDays.Count == 0 || EnoughDaysLeft(c, f as HoeDirt))
                return true;
            SMonitor.Log($"Preventing planting {__instance.Name}");
            __result = false;
            Game1.showRedMessage(string.Format(SHelper.Translation.Get("too-late-message"), __instance.Name));
            return false;
        }
        private static void IClickableMenu_drawToolTip_Prefix(string hoverText, ref string hoverTitle, Item hoveredItem)
        {
            if (!Config.EnableMod || !Config.LabelLatePlanting || hoveredItem == null)
                return;

            Crop crop = new Crop(hoveredItem.ItemId, 0, 0, Game1.currentLocation);
            if (crop == null || crop.phaseDays.Count == 0 || !crop.IsInSeason(Game1.currentLocation) || EnoughDaysLeft(crop, null) || (new int[] { 495, 496, 497, 498, 770 }).Contains(hoveredItem.ParentSheetIndex))
                return;

            hoverTitle = string.Format(SHelper.Translation.Get("too-late"), hoverTitle);
        }
        private static void GameLocation_createQuestionDialogue_Prefix(ref string question, string dialogKey)
        {
            if (!Config.EnableMod || dialogKey != "Sleep")
                return;

            List<string> logMessage = new List<string>();
            if (Config.WarnAboutPlantsUnwateredBeforeSleep)
            {
                bool added = false;

                foreach (var l in Game1.locations)
                    foreach (var terrainFeature in l.terrainFeatures.Values)
                    {
                        if (terrainFeature is HoeDirt && (terrainFeature as HoeDirt).crop != null && !(terrainFeature as HoeDirt).hasPaddyCrop() && (terrainFeature as HoeDirt).state.Value == 0 && (terrainFeature as HoeDirt).crop.currentPhase.Value < (terrainFeature as HoeDirt).crop.phaseDays.Count - 1)
                        {
                            logMessage.Add($"Crop with harvest index {(terrainFeature as HoeDirt).crop.indexOfHarvest.Value} at {l.NameOrUniqueName} {terrainFeature.Tile.X},{terrainFeature.Tile.Y} is unwatered");
                            if (!added)
                            {
                                added = true;
                                question = string.Format(SHelper.Translation.Get("plants-need-watering"), question);
                            }
                        }
                    }
            }
            if (Config.WarnAboutPlantsUnharvestedBeforeSleep)
            {
                bool added = false;

                var ignoreCrops = Config.IgnoreHarvestCrops.Split(',');
                foreach (var l in Game1.locations)
                {
                    foreach (HoeDirt hd in l.terrainFeatures.Values.Where(t => t is HoeDirt h && h.crop?.indexOfHarvest.Value is not null))
                    {
                        var harvest = ItemRegistry.Create(hd.crop.indexOfHarvest.Value);
                        if (hd.readyForHarvest() && (!Config.IgnoreFlowers || harvest.Category != -80) && (!ignoreCrops.Contains(hd.crop.indexOfHarvest.Value + "")))
                        {

                            logMessage.Add($"Crop with harvest index {hd.crop.indexOfHarvest.Value} at {l.NameOrUniqueName} {hd.Tile.X},{hd.Tile.Y} is ready to harvest");
                            if (!added)
                            {
                                added = true;
                                question = string.Format(SHelper.Translation.Get("plants-ready-for-harvest"), question);
                            }
                        }
                    }
                }
            }
            if (Config.WarnAboutAnimalsOutsideBeforeSleep)
            {
                var added = false;
                foreach (var l in Game1.locations)
                    if (l.buildings.Any() && l.IsOutdoors && l.Animals.Count() > 0)
                    {
                        logMessage.Add($"{Game1.getFarm().Animals.Count()} animals outside on {l.NameOrUniqueName}.");
                        if (!added)
                        {
                            added= true;
                            question = string.Format(SHelper.Translation.Get("animals-outside"), question);
                        }
                    }
            }
            if (Config.WarnAboutAnimalsUnharvestedBeforeSleep)
            {
                bool added = false;
                foreach (var l in Game1.locations)
                    foreach (FarmAnimal animal in l.Animals.Values)
                    {
                        if (animal.currentProduce.Value != null && !animal.type.Value.Contains("Pig"))
                        {
                            logMessage.Add($"{animal.type.Value} {animal.Name} on {l.NameOrUniqueName} is ready to harvest");
                            if (!added)
                            {
                                question = string.Format(SHelper.Translation.Get("animals-need-harvesting"), question);
                                added = true;
                            }
                        }
                    }
            }
            if (Config.WarnAboutAnimalsNotPetBeforeSleep)
            {
                bool added = false;
                foreach (var l in Game1.locations)
                    foreach (FarmAnimal animal in l.Animals.Values)
                    {
                        if (!animal.wasPet.Value && !animal.wasAutoPet.Value)
                        {
                            logMessage.Add($"{animal.type.Value} {animal.Name} on {l.NameOrUniqueName} needs petting");
                            if (!added)
                            {
                                question = string.Format(SHelper.Translation.Get("animals-need-petting"), question);
                                added = true;
                            }
                        }
                    }
            }
            if (logMessage.Any())
            {
                SMonitor.Log($"Warnings:\n\n\t{string.Join("\n\t", logMessage)}");
            }
        }
    }
}
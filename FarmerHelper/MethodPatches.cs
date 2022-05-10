using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Linq;
using Object = StardewValley.Object;

namespace FarmerHelper
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool Utility_tryToPlaceItem_Prefix(GameLocation location, Item item, ref bool __result )
        {
            if (!Config.EnableMod || !Config.PreventLatePlant || (new int[] { 495, 496, 497, 498, 770 }).Contains(item.ParentSheetIndex) || !(item is Object) || ((Object)item).Category != -74)
                return true;
            if (location.SeedsIgnoreSeasonsHere())
                return true;
            Crop c = new Crop(item.ParentSheetIndex, 0, 0);
            if (c == null)
                return true;
            if (c.phaseDays.Count == 0 || EnoughDaysLeft(c))
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

            if (!location.terrainFeatures.ContainsKey(placementTile) || !(location.terrainFeatures[placementTile] is HoeDirt))
                return true;

            if ((new int[] { 495, 496, 497, 498, 770 }).Contains(__instance.ParentSheetIndex))
                return true;

            if (location.SeedsIgnoreSeasonsHere())
                return true;

            Crop c = new Crop(__instance.ParentSheetIndex, x, y);
            if (c == null)
                return true;
            if (c.phaseDays.Count == 0 || EnoughDaysLeft(c))
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

            Crop crop = new Crop(hoveredItem.ParentSheetIndex, 0, 0);
            if (crop == null || crop.phaseDays.Count == 0 || !crop.seasonsToGrowIn.Contains(Game1.currentSeason) || EnoughDaysLeft(crop) || (new int[] { 495, 496, 497, 498, 770 }).Contains(hoveredItem.ParentSheetIndex))
                return;

            hoverTitle = string.Format(SHelper.Translation.Get("too-late"), hoverTitle);
        }
        private static void GameLocation_createQuestionDialogue_Prefix(ref string question, string dialogKey)
        {
            if (!Config.EnableMod || dialogKey != "Sleep")
                return;

            foreach (var terrainFeature in Game1.getFarm().terrainFeatures.Values)
            {
                if (terrainFeature is HoeDirt && Config.WarnAboutPlantsUnwateredBeforeSleep && (terrainFeature as HoeDirt).crop != null && !(terrainFeature as HoeDirt).hasPaddyCrop() && (terrainFeature as HoeDirt).state.Value == 0 && (terrainFeature as HoeDirt).crop.currentPhase.Value < (terrainFeature as HoeDirt).crop.phaseDays.Count - 1)
                {
                    question = string.Format(SHelper.Translation.Get("plants-need-watering"), question);
                    break;
                }
            }
            var ignoreCrops = Config.IgnoreHarvestCrops.Split(',');
            foreach (var obj in Game1.getFarm().terrainFeatures.Values)
            {
                if (!(obj is HoeDirt))
                    continue;
                if (Config.WarnAboutPlantsUnharvestedBeforeSleep && (obj as HoeDirt).readyForHarvest() && (!ignoreCrops.Contains((obj as HoeDirt).crop?.indexOfHarvest.Value+"")))
                {
                    question = string.Format(SHelper.Translation.Get("plants-ready-for-harvest"), question);
                    break;
                }
            }
            if (Config.WarnAboutAnimalsOutsideBeforeSleep)
            {
                if(Game1.getFarm().Animals.Count() > 0)
                    question = string.Format(SHelper.Translation.Get("animals-outside"), question);
            }
            if (Config.WarnAboutAnimalsUnharvestedBeforeSleep)
            {
                bool found = false;
                foreach (FarmAnimal animal in Game1.getFarm().Animals.Values)
                {
                    if (animal.currentProduce.Value > 0)
                    {
                        question = string.Format(SHelper.Translation.Get("animals-need-harvesting"), question);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    foreach (Building building in Game1.getFarm().buildings)
                    {
                        if (found)
                            break;
                        if(!(building.indoors.Value is AnimalHouse))
                            continue;
                        foreach(FarmAnimal animal in (building.indoors.Value as AnimalHouse).animals.Values)
                        {
                            if (animal.currentProduce.Value > 0)
                            {
                                question = string.Format(SHelper.Translation.Get("animals-need-harvesting"), question);
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (Config.WarnAboutAnimalsNotPetBeforeSleep)
            {
                bool found = false;
                foreach (FarmAnimal animal in Game1.getFarm().Animals.Values)
                {
                    if (!animal.wasPet.Value && !animal.wasAutoPet.Value)
                    {
                        question = string.Format(SHelper.Translation.Get("animals-need-petting"), question);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    foreach (Building building in Game1.getFarm().buildings)
                    {
                        if (found)
                            break;
                        if(!(building.indoors.Value is AnimalHouse))
                            continue;
                        foreach(FarmAnimal animal in (building.indoors.Value as AnimalHouse).animals.Values)
                        {
                            if (!animal.wasPet.Value && !animal.wasAutoPet.Value)
                            {
                                question = string.Format(SHelper.Translation.Get("animals-need-petting"), question);
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
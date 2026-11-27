using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Dimensions;

namespace Tent
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool isTenting;
        private static bool GameLocation_checkAction_Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            if (!Config.EnableMod)
                return true;

            Vector2 vect = new Vector2(tileLocation.X, tileLocation.Y);
            if (__instance.objects.ContainsKey(vect) && __instance.objects[vect].Name == "Tent")
            {

                SMonitor.Log($"Clicking on {__instance.objects[vect].Name}");
                isTenting = true;
                __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), __instance.createYesNoResponses(), "Sleep", null);
                __result = true;
                return false;
            }
            return true;
        }
        private static void GameLocation_answerDialogueAction_Prefix(string questionAndAnswer, string[] questionParams)
        {
            if (!Config.EnableMod || questionAndAnswer != "Sleep_Yes" || !isTenting)
                return;
            SMonitor.Log($"Sleeping in tent");

            Game1.player.isInBed.Value = true;
        }
        private static bool BedFurniture_ApplyWakeUpPosition_Prefix()
        {
            return !Config.EnableMod || !isTenting;
        }
        private static bool Farmer_draw_Prefix()
        {
            return !Config.EnableMod || !isTenting;
        }
        private static bool MineShaft_clearActiveMines_Prefix()
        {
            return !Config.EnableMod || !isTenting || !(Game1.player.currentLocation is MineShaft);
        }
        private static bool VolcanoDungeon_ClearAllLevels_Prefix()
        {
            return !Config.EnableMod || !isTenting || !(Game1.player.currentLocation is VolcanoDungeon);
        }
        private static bool SaveGameMenu_Prefix()
        {
            if (!Config.EnableMod || Config.SaveOnTent || !isTenting)
                return true;

            SMonitor.Log($"Preventing save");
            Game1.game1.IsSaving = false;
            if (Game1.IsMasterGame && Game1.newDaySync != null && !Game1.newDaySync.hasSaved())
            {
                Game1.newDaySync.flagSaved();
            }

            return false;
        }
        private static bool SaveGameMenu_update_Prefix(SaveGameMenu __instance)
        {
            if (!Config.EnableMod || Config.SaveOnTent || !isTenting)
                return true;
            Game1.exitActiveMenu();

            return false;

        }
        private static bool SaveGameMenu_draw_Prefix(SaveGameMenu __instance)
        {
            return (!Config.EnableMod || Config.SaveOnTent || !isTenting);


        }
    }
}
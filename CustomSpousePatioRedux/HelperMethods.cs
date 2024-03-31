using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace CustomSpousePatioRedux
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {
        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {

            if(Game1.player.IsMainPlayer)
            {
                Helper.Data.WriteSaveData(saveKey, outdoorAreas);
            }
        }
        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            spousePositions.Clear();
            outdoorAreas = new OutdoorAreaData();
        }
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                SMonitor.Log($"Not the host player, this copy of the mod will not do anything.", LogLevel.Warn);
                return;
            }
            LoadSpouseAreaData();
            DefaultSpouseAreaLocation = Game1.getFarm().GetSpouseOutdoorAreaCorner();

        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;
            if (!Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat"))
            {
                Farmer farmer = Game1.player;
                //Game1.getFarm().addSpouseOutdoorArea(Game1.player.spouse == null ? "" : Game1.player.spouse);
                var spouses = farmer.friendshipData.Pairs.Where(f => f.Value.IsMarried()).Select(f => f.Key).ToList();
                NPC ospouse = farmer.getSpouse();
                if (ospouse != null)
                {
                    spouses.Add(ospouse.Name);
                }
                foreach (string name in spouses)
                {
                    NPC npc = Game1.getCharacterFromName(name);

                    if (outdoorAreas.dict.ContainsKey(name) || (farmer.spouse.Equals(npc.Name) && name != "Krobus"))
                    {
                        SMonitor.Log($"placing {name} outdoors");
                        npc.setUpForOutdoorPatioActivity();
                    }
                }
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //Monitor.Log($"last question key: {Game1.player?.currentLocation?.lastQuestionKey}");

            if(!Config.EnableMod || !Context.IsWorldReady) 
                return;

            if (Context.CanPlayerMove && e.Button == Config.PatioWizardKey)
            {
                StartWizard();
            }
            else if (Game1.activeClickableMenu != null && Game1.player?.currentLocation?.lastQuestionKey?.StartsWith("CSP_Wizard_Questions") == true)
            {

                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;

                DialogueBox db = menu as DialogueBox;
                int resp = db.selectedResponse;
                List<Response> resps = db.responses;

                if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
                    return;
                Monitor.Log($"Answered {Game1.player.currentLocation.lastQuestionKey} with {resps[resp].responseKey}");

                currentPage = 0;
                CSPWizardDialogue(Game1.player.currentLocation.lastQuestionKey, resps[resp].responseKey);
                return;
            }
        }
    }

}
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class HelperEvents
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }

        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ModEntry.outdoorSpouse = null;
            ModEntry.kitchenSpouse = null;
            ModEntry.bedSpouse = null;
            ModEntry.spouseToDivorce = null;
            ModEntry.spouseRolesDate = -1;
            ModEntry.allRandomSpouses = null;
            ModEntry.bedSleepOffset = 48;
            ModEntry.allBedmates = null;
            ModEntry.bedMadeToday = false;
            ModEntry.kidsRoomExpandedToday = false;
            ModEntry.officialSpouse = null;
            Misc.SetAllNPCsDatable();
            FileIO.LoadTMXSpouseRooms();
            Misc.ResetSpouses(Game1.player);
        }


        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            FileIO.LoadKissAudio();
        }

        public static void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
            ModEntry.allRandomSpouses = null;
            ModEntry.kidsRoomExpandedToday = false;
            ModEntry.bedMadeToday = false;
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            Misc.ResetSpouses(Game1.player);
            ModEntry.allRandomSpouses = Misc.GetRandomSpouses(true).Keys.ToList();

            Utility.getHomeOfFarmer(Game1.player).showSpouseRoom();
            Maps.BuildSpouseRooms(Utility.getHomeOfFarmer(Game1.player));
            Misc.PlaceSpousesInFarmhouse();
        }

        public static void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
            ModEntry.spouses.Clear();
            ModEntry.outdoorSpouse = null;
            ModEntry.kitchenSpouse = null;
            ModEntry.bedSpouse = null;
            ModEntry.officialSpouse = null;
            ModEntry.spouseToDivorce = null;
            ModEntry.spouseRolesDate = -1;
            ModEntry.allRandomSpouses = null;
            ModEntry.allBedmates = null;
            ModEntry.bedMadeToday = false;
            ModEntry.kidsRoomExpandedToday = false;
        }

        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
            {
                if (Game1.currentLocation == null || Game1.currentLocation.lastQuestionKey != "divorce")
                    return;
                
                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;
                int resp = (int)typeof(DialogueBox).GetField("selectedResponse", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);
                List<Response> resps = (List<Response>)typeof(DialogueBox).GetField("responses", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);

                if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
                    return;

                string key = resps[resp].responseKey; 

                foreach (NPC spouse in ModEntry.spouses.Values)
                {
                    if (key == spouse.Name || key == "Krobus" || key == Game1.player.spouse)
                    {
                        if (Game1.player.Money >= 50000 || key == "Krobus")
                        {
                            if (!Game1.player.isRoommate(key))
                            {
                                Game1.player.Money -= 50000;
                                ModEntry.spouseToDivorce = key;
                            }
                            Game1.player.divorceTonight.Value = true;
                            string s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed_" + key);
                            if (s == null)
                            {
                                s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
                            }
                            Game1.drawObjectDialogue(s);
                            if (!Game1.player.isRoommate(Game1.player.spouse))
                            {
                                ModEntry.mp.globalChatInfoMessage("Divorce", new string[]
                                {
                                    Game1.player.Name
                                });
                            }
                        }
                        else
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
                        }
                        break;
                    }
                }
            }
        }
        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (Game1.player == null)
                return;
            FarmHouse fh = Utility.getHomeOfFarmer(Game1.player);
            if (fh == null)
                return;

            int bedWidth = Misc.GetBedWidth(fh);
            Point bedStart = Misc.GetBedStart(fh);
            foreach (NPC character in fh.characters)
            {
                if (ModEntry.allRandomSpouses.Contains(character.Name))
                {
                    if (Misc.IsInBed(character.GetBoundingBox()))
                    {
                        character.farmerPassesThrough = true;
                        if (Game1.timeOfDay >= 2000 && !character.isMoving())
                        {
                            Vector2 bedPos = Misc.GetSpouseBedPosition(fh, character.name);
                            character.position.Value = bedPos;
                        }
                    }
                    else
                    {
                        character.farmerPassesThrough = false;
                    }
                }
            }
            if (ModEntry.config.AllowSpousesToKiss)
            {
                Kissing.TrySpousesKiss();
            }
        }

    }
}
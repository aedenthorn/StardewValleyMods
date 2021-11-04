using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

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
            ModEntry.spouseToDivorce = null;
            Misc.SetAllNPCsDatable();
            FileIO.LoadTMXSpouseRooms();
            Misc.ResetSpouses(Game1.player);
            Misc.SetNPCRelations();
        }

        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            FileIO.LoadKissAudio();
        }

        public static void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Misc.ResetDivorces();
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            Misc.ResetSpouses(Game1.player);


            foreach (GameLocation location in Game1.locations)
            {
                if(ReferenceEquals(location.GetType(),typeof(FarmHouse)))
                {
                    FarmHouse fh = (location as FarmHouse);
                    fh.showSpouseRoom();
                    Maps.BuildSpouseRooms(fh);
                    Misc.PlaceSpousesInFarmhouse(fh);
                    //location.resetForPlayerEntry();
                }
            }
            if (Game1.IsMasterGame)
            {
                Game1.getFarm().addSpouseOutdoorArea(Game1.player.spouse == null ? "" : Game1.player.spouse);
                ModEntry.farmHelperSpouse = Misc.GetRandomSpouse(Game1.MasterPlayer);
            }
            foreach(Farmer f in Game1.getAllFarmers())
            {
                var spouses = Misc.GetSpouses(f, -1).Keys;
                foreach(string s in spouses)
                {
                    Monitor.Log($"{f.Name} is married to {s}");
                }
            }
        }

        public static void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
            ModEntry.spouseToDivorce = null;
        }

        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
        }


        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            foreach(GameLocation location in Game1.locations)
            {

                if(location is FarmHouse)
                {
                    FarmHouse fh = location as FarmHouse;
                    if (fh.owner == null)
                        continue;

                    List<string> allSpouses = Misc.GetSpouses(fh.owner, 1).Keys.ToList();
                    List<string> bedSpouses = Misc.ReorderSpousesForSleeping(allSpouses.FindAll((s) => ModEntry.config.RoommateRomance || !fh.owner.friendshipData[s].RoommateMarriage));

                    foreach (NPC character in fh.characters)
                    {
                        if (!(character.currentLocation == fh))
                            continue;

                        if (allSpouses.Contains(character.Name))
                        {

                            if (Misc.IsInBed(fh, character.GetBoundingBox()))
                            {
                                character.farmerPassesThrough = true;
                                if (!character.isMoving() && !Kissing.kissingSpouses.Contains(character.Name))
                                {
                                    Vector2 bedPos = Misc.GetSpouseBedPosition(fh, bedSpouses, character.Name);
                                    if(Game1.timeOfDay >= 2000 || Game1.timeOfDay <= 600)
                                    {
                                        character.position.Value = bedPos;
                                        character.ignoreScheduleToday = true;
                                        if (!character.isSleeping.Value)
                                        {
                                            Monitor.Log($"putting {character.Name} to sleep");
                                            character.isSleeping.Value = true;

                                        }
                                        if(character.Sprite.CurrentAnimation == null)
                                        {
                                            if (!Misc.HasSleepingAnimation(character.Name))
                                            {
                                                character.Sprite.StopAnimation();
                                                character.faceDirection(0);
                                            }
                                            else
                                            {
                                                character.playSleepingAnimation();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        character.faceDirection(3);
                                        character.isSleeping.Value = false;
                                    }
                                }
                                else
                                {
                                    character.isSleeping.Value = false;
                                }
                                character.HideShadow = true;
                            }
                            else if (Game1.timeOfDay < 2000 && Game1.timeOfDay > 600)
                            {
                                character.farmerPassesThrough = false;
                                character.HideShadow = false;
                                character.isSleeping.Value = false;
                            }
                        }
                    }
                    if (location == Game1.player.currentLocation && ModEntry.config.AllowSpousesToKiss)
                    {
                        Kissing.TrySpousesKiss();
                    }
                }
            }
        }
    }
}
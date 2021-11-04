using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

namespace FreeLove
{
    /// <summary>The mod entry point.</summary>
    public class HelperEvents
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Integrations.LoadModApis();
        }

        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Misc.SetAllNPCsDatable();
            Misc.ResetSpouses(Game1.player);
            Misc.SetNPCRelations();
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Misc.ResetDivorces();
            Misc.ResetSpouses(Game1.player);


            foreach (GameLocation location in Game1.locations)
            {
                if(ReferenceEquals(location.GetType(),typeof(FarmHouse)))
                {
                    Misc.PlaceSpousesInFarmhouse(location as FarmHouse);
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


        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            foreach (GameLocation location in Game1.locations)
            {

                if (location is FarmHouse)
                {
                    FarmHouse fh = location as FarmHouse;
                    if (fh.owner == null)
                        continue;

                    List<string> allSpouses = Misc.GetSpouses(fh.owner, 1).Keys.ToList();
                    List<string> bedSpouses = Misc.ReorderSpousesForSleeping(allSpouses.FindAll((s) => ModEntry.config.RoommateRomance || !fh.owner.friendshipData[s].RoommateMarriage));

                    using(IEnumerator<NPC> characters = fh.characters.GetEnumerator())
                    {
                        while (characters.MoveNext())
                        {
                            var character = characters.Current;
                            if (!(character.currentLocation == fh))
                            {
                                character.farmerPassesThrough = false;
                                character.HideShadow = false;
                                character.isSleeping.Value = false;
                                continue;
                            }

                            if (allSpouses.Contains(character.Name))
                            {

                                if (Misc.IsInBed(fh, character.GetBoundingBox()))
                                {
                                    character.farmerPassesThrough = true;
                                    if (!character.isMoving() && (Integrations.kissingAPI == null || !Integrations.kissingAPI.IsKissing(character.Name)))
                                    {
                                        Vector2 bedPos = Misc.GetSpouseBedPosition(fh, character.Name);
                                        if (Game1.timeOfDay >= 2000 || Game1.timeOfDay <= 600)
                                        {
                                            character.position.Value = bedPos;

                                            if (Game1.timeOfDay >= 2200)
                                            {
                                                character.ignoreScheduleToday = true;
                                            }
                                            if (!character.isSleeping.Value)
                                            {
                                                character.isSleeping.Value = true;

                                            }
                                            if (character.Sprite.CurrentAnimation == null)
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
                    }
                }
            }
        }
    }
}
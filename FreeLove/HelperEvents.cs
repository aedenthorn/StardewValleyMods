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
    public partial class ModEntry
    {

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Points To Marry",
                getValue: () => Config.MinPointsToMarry,
                setValue: value => Config.MinPointsToMarry = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Min Points To Date",
                getValue: () => Config.MinPointsToDate,
                setValue: value => Config.MinPointsToDate = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Prevent Hostile Divorces",
                getValue: () => Config.PreventHostileDivorces,
                setValue: value => Config.PreventHostileDivorces = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Complex Divorces",
                getValue: () => Config.ComplexDivorce,
                setValue: value => Config.ComplexDivorce = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Roommate Romance",
                getValue: () => Config.RoommateRomance,
                setValue: value => Config.RoommateRomance = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max children",
                getValue: () => Config.MaxChildren,
                setValue: value => Config.MaxChildren = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Parent Names",
                getValue: () => Config.ShowParentNames,
                setValue: value => Config.ShowParentNames = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Buy Pendants Anytime",
                getValue: () => Config.BuyPendantsAnytime,
                setValue: value => Config.BuyPendantsAnytime = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Pendant Price",
                getValue: () => Config.PendantPrice,
                setValue: value => Config.PendantPrice = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Percent Chance For In Bed",
                getValue: () => Config.PercentChanceForSpouseInBed,
                setValue: value => Config.PercentChanceForSpouseInBed = value,
                min: 0,
                max:100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Chance For In Kitchen",
                getValue: () => Config.PercentChanceForSpouseInKitchen,
                setValue: value => Config.PercentChanceForSpouseInKitchen = value,
                min: 0,
                max:100
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Chance For In Patio",
                getValue: () => Config.PercentChanceForSpouseAtPatio,
                setValue: value => Config.PercentChanceForSpouseAtPatio = value,
                min: 0,
                max:100
            );

            LoadModApis();
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            currentSpouses.Clear();
            currentUnofficialSpouses.Clear();
        }
        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            SetAllNPCsDatable();
            ResetSpouses(Game1.player);
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            ResetDivorces();
            ResetSpouses(Game1.player);


            foreach (GameLocation location in Game1.locations)
            {
                if(ReferenceEquals(location.GetType(),typeof(FarmHouse)))
                {
                    PlaceSpousesInFarmhouse(location as FarmHouse);
                }
            }
            if (Game1.IsMasterGame)
            {
                Game1.getFarm().addSpouseOutdoorArea(Game1.player.spouse == null ? "" : Game1.player.spouse);
                farmHelperSpouse = GetRandomSpouse(Game1.MasterPlayer);
            }
            foreach(Farmer f in Game1.getAllFarmers())
            {
                var spouses = GetSpouses(f, true).Keys;
                foreach(string s in spouses)
                {
                    SMonitor.Log($"{f.Name} is married to {s}");
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

                    List<string> allSpouses = GetSpouses(fh.owner, true).Keys.ToList();
                    List<string> bedSpouses = ReorderSpousesForSleeping(allSpouses.FindAll((s) => Config.RoommateRomance || !fh.owner.friendshipData[s].RoommateMarriage));

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

                                if (IsInBed(fh, character.GetBoundingBox()))
                                {
                                    character.farmerPassesThrough = true;

                                    if (!character.isMoving() && (kissingAPI == null || kissingAPI.LastKissed(character.Name) < 0 || kissingAPI.LastKissed(character.Name) > 2))
                                    {
                                        Vector2 bedPos = GetSpouseBedPosition(fh, character.Name);
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
                                                if (!HasSleepingAnimation(character.Name))
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
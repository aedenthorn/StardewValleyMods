using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using System.Collections.Generic;

namespace PetBed
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Pet), nameof(Pet.warpToFarmHouse)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pet_warpToFarmHouse_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Pet), nameof(Pet.setAtFarmPosition)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pet_setAtFarmPosition_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Pet), nameof(Pet.dayUpdate)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pet_dayUpdate_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pet_dayUpdate_Postfix))
            );

        }

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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Is Bed?",
                getValue: () => Config.IsBed,
                setValue: value => Config.IsBed = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bed Chance",
                getValue: () => Config.BedChance,
                setValue: value => Config.BedChance = value,
                min: 0,
                max: 100
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Indoor Bed Name",
                tooltip: () => "Name of furniture to use as indoor bed. Or X,Y sleep coordinates.",
                getValue: () => Config.IndoorBedName,
                setValue: value => Config.IndoorBedName = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Outdoor Bed Name",
                tooltip: () => "Name of furniture to use as outdoor bed. Or X,Y sleep coordinates.",
                getValue: () => Config.OutdoorBedName,
                setValue: value => Config.OutdoorBedName = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Indoor Bed Offset X,Y",
                tooltip: () => "In pixels.",
                getValue: () => Config.IndoorBedOffset,
                setValue: value => Config.IndoorBedOffset = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Outdoor Bed Offset X,Y",
                tooltip: () => "In pixels.",
                getValue: () => Config.OutdoorBedOffset,
                setValue: value => Config.OutdoorBedOffset = value
            );
        }
        private static bool WarpPetToBed(Pet pet, GameLocation location, ref int ____currentBehavior, bool outdoor)
        {

            string which = outdoor ? Config.OutdoorBedName : Config.IndoorBedName;
            List<string> names = new List<string>();
            if (which.Contains(";"))
            {
                string[] parts = which.Split(';');
                foreach (string s in parts)
                {
                    if (s.StartsWith(pet.Name + ":"))
                    {
                        names.Add(s.Substring((pet.Name + ":").Length));
                    }
                    else if (!s.Contains(":"))
                        names.Add(s);
                }
            }
            else
            {
                names.Add(which);
            }
            SMonitor.Log($"{names.Count} possible beds for {pet.Name}");
            if (names.Count > 0)
            {
                Vector2 sleeping_tile = new Vector2(-1, -1);
                ShuffleList(names);
                foreach (string name in names)
                {
                    SMonitor.Log($"Checking bed {name}");
                    if (name.Contains(","))
                    {
                        string[] parts = name.Split(',');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int X) && int.TryParse(parts[1], out int Y))
                        {
                            if (location.isCharacterAtTile(sleeping_tile) != null)
                                continue;
                            sleeping_tile = new Vector2(X, Y);
                            SMonitor.Log($"Setting sleeping tile manually to {sleeping_tile}");
                        }
                    }
                    if (sleeping_tile.X == -1)
                    {
                        List<Furniture> flist = new List<Furniture>();
                        foreach (Furniture furniture in location.furniture)
                        {
                            SMonitor.Log($"Checking furniture {furniture.Name} is {name}");

                            if ((furniture.Name == name || furniture.Name.EndsWith($"/{name}") ) && location.isCharacterAtTile(furniture.TileLocation) == null)
                                flist.Add(furniture);
                        }
                        if (flist.Count > 0)
                        {
                            sleeping_tile = flist[Game1.random.Next(0, flist.Count)].TileLocation;
                            SMonitor.Log($"Found ped bed {name} at {sleeping_tile}");
                        }
                    }
                    if (sleeping_tile.X > -1)
                    {
                        Vector2 offset = Vector2.Zero;
                        string offsetString = outdoor ? Config.OutdoorBedOffset : Config.IndoorBedOffset;
                        var offsetParts = offsetString.Split(',');
                        if (offsetParts.Length == 2 && int.TryParse(offsetParts[0].Trim(), out int oX) && int.TryParse(offsetParts[1].Trim(), out int oY))
                        {
                            offset = new Vector2(oX, oY);
                        }
                        SMonitor.Log($"Moving pet to {sleeping_tile}, pixel offset {offset}");

                        pet.isSleepingOnFarmerBed.Value = Config.IsBed;
                        pet.faceDirection(2);
                        Game1.warpCharacter(pet, location, sleeping_tile);
                        pet.position.Value += offset;

                        pet.UpdateSleepingOnBed();
                        ____currentBehavior = pet.CurrentBehavior;
                        pet.Halt();
                        pet.Sprite.CurrentAnimation = null;
                        pet.OnNewBehavior();
                        pet.Sprite.UpdateSourceRect();
                        return true;
                    }
                }
            }

            

            SMonitor.Log($"No bed found for '{pet.Name}'");
            return false;
        }
        public static List<T> ShuffleList<T>(List<T> _list)
        {
            List<T> list = new List<T>(_list);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }

}
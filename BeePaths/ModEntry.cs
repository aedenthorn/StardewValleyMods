using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;

namespace BeePaths
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Dictionary<string, Dictionary<Vector2, HiveData>> hiveDict = new();
        public static string flooringKey = "aedenthorn.BeePaths/flooring";
        public static bool drawingTiles;
        public static Texture2D beeDot;
        public static ICue buzz;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            beeDot = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            beeDot.SetData(new Color[] { Color.White });
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || !hiveDict.TryGetValue(Game1.currentLocation.Name, out var dict) || !dict.Any())
                return;
            foreach(var key in dict.Keys.ToArray())
            {
                Crop c = Utility.findCloseFlower(Game1.currentLocation, key, 5, (Crop crop) => !crop.forageCrop.Value);
                if(c is null)
                {
                    dict.Remove(key);
                }
                else
                {
                    dict[key].cropTile = AccessTools.FieldRefAccess<Crop, Vector2>(c, "tilePosition");
                }

            }
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if(!Config.ModEnabled || !Context.IsPlayerFree || Game1.IsRainingHere(Game1.currentLocation)) return;
            bool buzzing = false;
            float buzzDistance = float.MaxValue;
            foreach (var kvp in Game1.currentLocation.objects.Pairs)
            {
                if(!kvp.Value.name.Equals("Bee House") || !Utility.isOnScreen(kvp.Key * 64, 64))
                    continue;
                if (!hiveDict.TryGetValue(Game1.currentLocation.Name, out var dict))
                {
                    hiveDict[Game1.currentLocation.Name] = dict = new();
                }
                if(!dict.TryGetValue(kvp.Key, out var hive))
                {
                    Crop c = Utility.findCloseFlower(Game1.currentLocation, kvp.Key, 5, (Crop crop) => !crop.forageCrop.Value);
                    if (c is null)
                    {
                        continue;
                    }
                    dict[kvp.Key] = hive = new();
                    var cropTile = AccessTools.FieldRefAccess<Crop, Vector2>(c, "tilePosition");
                    hive.cropTile = cropTile;
                    while (hive.bees.Count < Config.NumberBees)
                    {
                        var reverse = Game1.random.NextDouble() < 0.25;
                        hive.bees.Add(reverse ? GetBee(cropTile, kvp.Key) : GetBee(kvp.Key, cropTile));
                    }
                }
                if (hive.bees.Count < Config.NumberBees && Game1.random.NextDouble() < 0.25)
                {
                    var reverse = Game1.random.NextDouble() < 0.5;
                    hive.bees.Add(reverse ? GetBee(hive.cropTile, kvp.Key, false) : GetBee(kvp.Key, hive.cropTile, false));
                }
                for (int i = hive.bees.Count - 1; i >= 0; i--)
                {
                    var bee = hive.bees[i];
                    Vector2 drawPos = bee.pos + Vector2.Normalize(Vector2.Transform(bee.pos, Matrix.CreateRotationX(90f * (float)Math.PI / 180f))) * 5 * (float)Math.Sin(Vector2.Distance(bee.startPos, bee.pos) / 20);
                    e.SpriteBatch.Draw(beeDot, Game1.GlobalToLocal(drawPos), null, Config.BeeColor, -(float)Math.Atan((bee.endPos - bee.pos).Y / (bee.endPos - bee.pos).X), Vector2.Zero, Config.BeeScale, SpriteEffects.None, 1);
                    if(Config.BeeDamage > 0 && Game1.random.Next(100) < Config.BeeStingChance)
                    {
                        foreach (var f in Game1.getAllFarmers())
                        {
                            if (f.currentLocation == Game1.currentLocation && f.GetBoundingBox().Contains(bee.pos + new Vector2(0, 32)))
                                f.takeDamage(Config.BeeDamage, true, null);
                        }
                    }
                    var distance = Vector2.Distance(Game1.player.getTileLocation(), bee.pos / 64 + new Vector2(-0.5f, 0.5f));
                    if (!string.IsNullOrEmpty(Config.BeeSound) && distance < Config.MaxSoundDistance && distance < buzzDistance)
                    {
                        buzzing = true;
                        buzzDistance = distance;
                    }

                    if (Vector2.Distance(hive.bees[i].endPos, hive.bees[i].pos) > Config.BeeSpeed)
                    {
                        hive.bees[i].pos = Vector2.Lerp(hive.bees[i].pos, hive.bees[i].endPos, Config.BeeSpeed / Vector2.Distance(hive.bees[i].endPos, hive.bees[i].pos));
                    }
                    else
                    {
                        hive.bees.RemoveAt(i);
                    }
                }
                if (buzzing)
                {
                    if(buzz is null)
                        buzz = Game1.soundBank.GetCue(Config.BeeSound);
                    var vol = 100 - 100 * buzzDistance / Config.MaxSoundDistance - 10;
                    buzz.Pitch = 0;
                    buzz.SetVariable("Volume", vol);
                    buzz.SetVariable("Pitch", 0f);
                    if (!buzz.IsPlaying)
                        buzz.Play();
                }
            }
            if(!buzzing && buzz is not null)
            {
                buzz.Stop(AudioStopOptions.AsAuthored);
            }
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
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show When Raining",
                getValue: () => Config.ShowWhenRaining,
                setValue: value => Config.ShowWhenRaining = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Fix Flower Find",
                getValue: () => Config.FixFlowerFind,
                setValue: value => Config.FixFlowerFind = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Number of Bees",
                getValue: () => Config.NumberBees,
                setValue: value => Config.NumberBees = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bee Range",
                getValue: () => Config.BeeRange,
                setValue: value => Config.BeeRange = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bee Damage",
                getValue: () => Config.BeeDamage,
                setValue: value => Config.BeeDamage = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Sting % Chance",
                getValue: () => Config.BeeStingChance,
                setValue: value => Config.BeeStingChance = value,
                min: 0,
                max: 100
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Bee Scale",
                getValue: () => Config.BeeScale+"",
                setValue: delegate(string value){ if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) Config.BeeScale = f; }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Bee Speed",
                getValue: () => Config.BeeSpeed+"",
                setValue: delegate(string value){ if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) Config.BeeSpeed = f; }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Bee Sound",
                getValue: () => Config.BeeSound,
                setValue: value => Config.BeeSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Bee Sound Distance",
                getValue: () => Config.MaxSoundDistance + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) Config.MaxSoundDistance = f; }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bee Color R",
                getValue: () => Config.BeeColor.R,
                setValue: value => Config.BeeColor = new Color(value, Config.BeeColor.G, Config.BeeColor.B, Config.BeeColor.A),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bee Color G",
                getValue: () => Config.BeeColor.G,
                setValue: value => Config.BeeColor = new Color(Config.BeeColor.R, value, Config.BeeColor.B, Config.BeeColor.A),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bee Color B",
                getValue: () => Config.BeeColor.B,
                setValue: value => Config.BeeColor = new Color(Config.BeeColor.R, Config.BeeColor.G, value, Config.BeeColor.A),
                min: 0,
                max: 255
            );
        }
    }
}
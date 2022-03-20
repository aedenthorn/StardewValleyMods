using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectTriggers
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static readonly string dictPath = "aedenthorn.ObjectTriggers/dictionary";
        private static Dictionary<string, ObjectTriggerData> objectTriggerDataDict = new Dictionary<string, ObjectTriggerData>();
        private static Dictionary<long, List<ObjectTriggerInstance>> farmerTrippingDict = new Dictionary<long, List<ObjectTriggerInstance>>();
        private static IParticleEffectAPI particleEffectAPI;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            foreach (var kvp in objectTriggerDataDict)
            {
                switch (kvp.Value.tripperType)
                {
                    case "time":

                        break;

                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            objectTriggerDataDict = Helper.Content.Load<Dictionary<string, ObjectTriggerData>>(dictPath, ContentSource.GameContent);
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady)
                return;
            foreach(var kvp in objectTriggerDataDict)
            {
                switch (kvp.Value.tripperType)
                {
                    case "farmer":
                        foreach (var farmer in Game1.getAllFarmers())
                        {
                            CheckTrigger(farmer, farmer.currentLocation, farmer.getTileLocation(), kvp.Key);
                        }
                        break;
                }
            }
            foreach (var key in farmerTrippingDict.Keys.ToArray())
            {
                for (int j = farmerTrippingDict[key].Count - 1; j >= 0; j--)
                {
                    Farmer farmer = Game1.getFarmer(key);
                    if (farmer != null && !CheckObjectTrigger(farmer.currentLocation, farmer.getTileLocation(), farmerTrippingDict[farmer.UniqueMultiplayerID][j].triggerKey, farmerTrippingDict[farmer.UniqueMultiplayerID][j].tilePosition))
                        ResetTrigger(farmer, farmerTrippingDict[farmer.UniqueMultiplayerID][j].tilePosition, farmerTrippingDict[farmer.UniqueMultiplayerID][j].triggerKey);
                }
            }

            var keyArray = farmerTrippingDict.Keys.ToArray();
            for (int i = 0; i < keyArray.Length; i++)
            {
                var key = keyArray[i];
                Farmer farmer = Game1.getFarmer(key);
                if (farmer == null)
                    continue;
                for (int j = 0; j < farmerTrippingDict[key].Count; j++)
                {
                    var oti = farmerTrippingDict[key][j];
                    var otd = objectTriggerDataDict[oti.triggerKey];
                    if (otd.interval <= 0)
                        continue;
                    oti.elapsed++;
                    if (oti.elapsed > otd.interval) 
                    {
                        oti.elapsed = 0;
                        if (Game1.random.NextDouble() > otd.tripChance)
                            continue;
                        farmerTrippingDict[key][j].elapsed = 0;
                        float amount = objectTriggerDataDict[oti.triggerKey].effectAmountMin + (float)Game1.random.NextDouble() * (objectTriggerDataDict[oti.triggerKey].effectAmountMax - objectTriggerDataDict[oti.triggerKey].effectAmountMin);
                        switch (otd.triggerEffectType)
                        {
                            case "damage":
                                farmer.takeDamage((int)amount, true, null);
                                if (otd.tripSound != null)
                                    farmer.currentLocation.playSound(otd.tripSound);
                                break;
                            case "sound":
                                if (otd.tripSound != null)
                                    farmer.currentLocation.playSound(otd.tripSound);
                                break;
                            case "heal":
                                amount = Math.Min(farmer.maxHealth - farmer.health, amount);
                                if(amount > 0)
                                {
                                    if(otd.tripSound != null)
                                        farmer.currentLocation.playSound(otd.tripSound);
                                    farmer.health += (int)amount;
                                    farmer.currentLocation.debris.Add(new Debris((int)amount, new Vector2((farmer.getStandingX() + 8), farmer.getStandingY()), Color.Green, 1f, farmer));
                                }
                                break;
                            case "energize":
                                amount = Math.Min(farmer.MaxStamina - farmer.Stamina, amount);
                                if (amount > 0)
                                {
                                    if (otd.tripSound != null)
                                        farmer.currentLocation.playSound(otd.tripSound);
                                    farmer.Stamina += amount;
                                    farmer.currentLocation.debris.Add(new Debris((int)amount, new Vector2((farmer.getStandingX() + 8), farmer.getStandingY()), Color.Yellow, 1f, farmer));
                                }
                                break;
                            case "exhaust":
                                amount = Math.Min(farmer.Stamina, amount);
                                if (amount > 0)
                                {
                                    if (otd.tripSound != null)
                                        farmer.currentLocation.playSound(otd.tripSound);
                                    farmer.Stamina += amount;
                                    farmer.currentLocation.debris.Add(new Debris((int)amount, new Vector2((farmer.getStandingX() + 8), farmer.getStandingY()), Color.Yellow, 1f, farmer));
                                }
                                break;
                        }
                    }
                }
            }
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {

            particleEffectAPI = Helper.ModRegistry.GetApi<IParticleEffectAPI>("aedenthorn.ParticleEffects");

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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(dictPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log("Loading dictionary");

            return (T)(object)new Dictionary<string, ObjectTriggerData>();
        }
    }
}
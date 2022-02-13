using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace InstanceEditor
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        
        public static readonly string dictPath = "InstanceEditor_Dict";

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

            Helper.Events.GameLoop.GameLaunched += GameLaunched;
            
            Helper.Events.GameLoop.SaveLoaded += SaveLoaded;
            
            Helper.Events.GameLoop.DayStarted += DayStarted;
            Helper.Events.GameLoop.DayEnding += DayEnding;

            //Helper.Events.GameLoop.UpdateTicking += UpdateTicking;
            //Helper.Events.GameLoop.UpdateTicked += UpdateTicked; ;

            Helper.Events.GameLoop.OneSecondUpdateTicking += OneSecondUpdateTicking;
            Helper.Events.GameLoop.OneSecondUpdateTicked += OneSecondUpdateTicked;
            
            Helper.Events.GameLoop.Saving += Saving;
            Helper.Events.GameLoop.Saved += Saved;

            Helper.Events.GameLoop.TimeChanged += TimeChanged;

            var harmony = new Harmony(ModManifest.UniqueID);

        }

        private void TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            CheckForEdits("TimeChanged");
        }

        private void Saved(object sender, StardewModdingAPI.Events.SavedEventArgs e)
        {
            CheckForEdits("Saved");
        }

        private void Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            CheckForEdits("Saving");
        }

        private void OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            CheckForEdits("OneSecondUpdateTicked");
        }

        private void OneSecondUpdateTicking(object sender, StardewModdingAPI.Events.OneSecondUpdateTickingEventArgs e)
        {
            CheckForEdits("OneSecondUpdateTicking");
        }

        private void UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            CheckForEdits("UpdateTicked");
        }

        private void UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            CheckForEdits("UpdateTicking");
        }

        private void DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            CheckForEdits("DayStarted");
        }

        private void DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            CheckForEdits("DayEnding");
        }

        private void SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            CheckForEdits("SaveLoaded");
            CheckForEdits("DayStarted");
        }

        private void CheckForEdits(string trigger)
        {
            if (!Context.IsWorldReady)
                return;
            //Monitor.Log($"Checking for edits on {trigger}");
            //Stopwatch s = new Stopwatch();
            //s.Start();
            var dict = Game1.content.Load<Dictionary<string, InstanceEditData>>(dictPath) ?? new Dictionary<string, InstanceEditData>();
            foreach (var data in dict.Values)
            {
                if (data.checks.Contains(trigger))
                {
                    var type = typeof(InstanceGame).Assembly.GetType(data.className);
                    if (type == null || !GetFieldInfos(type, data.matchFields) || !GetFieldInfos(type, data.changeFields))
                        continue;

                    if (typeof(NPC).IsAssignableFrom(type))
                    {
                        foreach (var location in Game1.locations)
                        {
                            for(int i = 0; i < location.characters.Count; i++)
                            {
                                EditInstance(location.characters[i], type, data);
                                
                            }
                        }
                    }
                }
            }
            //Monitor.Log($"Time: {s.Elapsed}");
        }

        private bool GetFieldInfos(Type type, Dictionary<string, FieldEditData> fields)
        {
            foreach(var key in fields.Keys)
            {
                FieldInfo info = AccessTools.Field(type, key);
                if (info == null)
                    return false;
                fields[key].fieldInfo = info;
            }
            return true;
        }

        private void EditInstance(NPC c, Type type, InstanceEditData data)
        {
            if (c.GetType() != type)
                return;
            foreach (var f in data.matchFields)
            {
                if (f.Value.value != null)
                {
                    if (f.Value.fieldInfo.GetValue(c) != f.Value)
                        return;
                }
                else if (f.Value.fields != null)
                {
                    foreach (var field in f.Value.fields)
                    {
                        var subField = AccessTools.Field(f.Value.fieldInfo.FieldType, field.Key);
                        //SMonitor.Log($"current {info.Name}: {subField.GetValue(info.GetValue(c))}, check: {field.Value}, types {subField.FieldType} {field.Value.GetType()}, {subField.GetValue(info.GetValue(c)).Equals(field.Value)}");
                        if (!subField.GetValue(f.Value.fieldInfo.GetValue(c)).Equals(field.Value))
                            return;
                    }
                }
            }
            foreach (var f in data.changeFields)
            {
                if (f.Value.value != null)
                {
                    f.Value.fieldInfo.SetValue(c, f.Value.value);
                }
                else if (f.Value.fields != null)
                {
                    foreach (var field in f.Value.fields)
                    {
                        AccessTools.Field(f.Value.fieldInfo.FieldType, field.Key).SetValue(f.Value.fieldInfo.GetValue(c), field.Value);
                        Monitor.Log($"set {field.Key} to {field.Value} for {c.Name}");
                    }
                }
            }
        }

        private void GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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

            return (T)(object)new Dictionary<string, InstanceEditData>();
        }
    }
}

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace StardewImpact
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string dictPath = "aedenthorn.StardewImpact/characters";
        
        public static string slotPrefix = "aedenthorn.StardewImpact/slot";
        public static string currentSlotKey = "aedenthorn.StardewImpact/currentSlot";
        
        public static string framePath = "aedenthorn.StardewImpact/frame";
        public static string backPath = "aedenthorn.StardewImpact/back";
        public static string whitePath = "aedenthorn.StardewImpact/white";
        public static string skillIconPath = "aedenthorn.StardewImpact/skillIcon";
        public static string burstIconPath = "aedenthorn.StardewImpact/burstIcon";
        

        public static Texture2D frameTexture;
        public static Texture2D backTexture;
        public static Texture2D whiteTexture;
        public static Texture2D defaultSkillIcon;
        public static Texture2D defaultBurstIcon;

        public static Dictionary<string, CharacterData> characterDict = new Dictionary<string, CharacterData>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            foreach(var key in characterDict.Keys.ToArray())
            {
                LoadTextures(key);
            }
        }

        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            foreach(var key in characterDict.Keys.ToArray())
            {
                if (characterDict[key].SkillCooldownValue > 0)
                {
                    characterDict[key].SkillCooldownValue -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                }
                if (characterDict[key].BurstCooldownValue > 0)
                {
                    characterDict[key].BurstCooldownValue -= (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

        public override object GetApi()
        {
            return new StardewImpactApi();
        }


        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(framePath))
            {
                e.LoadFromModFile<Texture2D>("assets/frame.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(backPath))
            {
                e.LoadFromModFile<Texture2D>("assets/back.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(whitePath))
            {
                e.LoadFromModFile<Texture2D>("assets/white.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(skillIconPath))
            {
                e.LoadFromModFile<Texture2D>("assets/skill.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(burstIconPath))
            {
                e.LoadFromModFile<Texture2D>("assets/burst.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, CharacterData>(), StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            frameTexture = Game1.content.Load<Texture2D>(framePath);
            backTexture = Game1.content.Load<Texture2D>(backPath);
            whiteTexture = Game1.content.Load<Texture2D>(whitePath);
            defaultSkillIcon = Game1.content.Load<Texture2D>(skillIconPath);
            defaultBurstIcon = Game1.content.Load<Texture2D>(burstIconPath);
            var dict = Game1.content.Load<Dictionary<string, CharacterData>>(dictPath);
            var count = 0;
            foreach(var key in dict.Keys.ToArray())
            {
                if (characterDict.ContainsKey(key))
                {
                    Monitor.Log($"Error loading character {key}; character already exists", LogLevel.Error);
                    continue;
                }
                NPC npc = Game1.getCharacterFromName(key);
                if (npc is null)
                {
                    Monitor.Log($"Error loading character {key}; npc not found", LogLevel.Error);
                    dict.Remove(key);
                    continue;
                }
                characterDict[key] = dict[key];
                LoadTextures(key);
                count++;
            }
            Monitor.Log($"Loaded {count} characters; total: {dict.Count}");
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
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Portrait Scale",
                getValue: () => Config.PortraitScale+ "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result)){ Config.PortraitScale = result; } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Portrait Spacing",
                getValue: () => Config.PortraitSpacing,
                setValue: value => Config.PortraitSpacing = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Skills Offset X",
                getValue: () => Config.CurrentSkillOffsetX,
                setValue: value => Config.CurrentSkillOffsetX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Skills Offset Y",
                getValue: () => Config.CurrentSkillOffsetY,
                setValue: value => Config.CurrentSkillOffsetY = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Skills Scale",
                getValue: () => Config.SkillsScale + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result)) { Config.SkillsScale = result; } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Skills Spacing",
                getValue: () => Config.SkillsSpacing,
                setValue: value => Config.SkillsSpacing = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch to 1",
                getValue: () => Config.Button1,
                setValue: value => Config.Button1 = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch to 2",
                getValue: () => Config.Button2,
                setValue: value => Config.Button2 = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch to 3",
                getValue: () => Config.Button3,
                setValue: value => Config.Button3 = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch to 4",
                getValue: () => Config.Button4,
                setValue: value => Config.Button4 = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch to Farmer",
                getValue: () => Config.Button5,
                setValue: value => Config.Button5 = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Skill Button",
                getValue: () => Config.SkillButton,
                setValue: value => Config.SkillButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Burst Button",
                getValue: () => Config.BurstButton,
                setValue: value => Config.BurstButton = value
            );

        }
    }
}
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

namespace SDIEmily
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
        
        
        public static string skillIconPath = "aedenthorn.SDIEmily/skillIcon";
        public static string burstIconPath = "aedenthorn.SDIEmily/burstIcon";
        

        public static Texture2D frameTexture;
        public static Texture2D backTexture;
        public static Texture2D skillIcon;
        public static Texture2D burstIcon;
        
        public static IStardewImpactApi sdiAPI;

        private static RunningSkill runningSkill;

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

            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled || runningSkill is null)
                return;
            
            bool here = Game1.currentLocation == runningSkill.currentLocation;
                
            if (runningSkill.currentTick == 0)
            {
                if (here)
                    runningSkill.currentLocation.playSound("swordswipe");
                runningSkill.currentFrame = 0;
                runningSkill.currentStartPos = GetRandomPointOnCircle(runningSkill.center, Config.SkillRadius);
                runningSkill.currentEndPos = runningSkill.center - (runningSkill.currentStartPos - runningSkill.center);
                runningSkill.totalTicks = (int)(Config.SkillRadius * 2 / (16 * Config.SkillSpeed));
                Monitor.Log($"start {runningSkill.currentStartPos}, start {runningSkill.center}, end {runningSkill.currentEndPos}, length {Vector2.Distance(runningSkill.currentEndPos, runningSkill.currentStartPos)}");
            }
            runningSkill.currentPos = Vector2.Lerp(runningSkill.currentStartPos, runningSkill.currentEndPos, runningSkill.currentTick / (float)runningSkill.totalTicks);
            if(here)
                e.SpriteBatch.Draw(SHelper.GameContent.Load<Texture2D>("LooseSprites\\parrots"), runningSkill.currentPos, new Rectangle(48 + runningSkill.currentFrame * 24, 0, 24, 24), Color.White, 0, Vector2.Zero, 4, (runningSkill.currentStartPos.X < runningSkill.currentEndPos.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 1f);
            runningSkill.currentTick++;
            if(runningSkill.currentTick % (runningSkill.totalTicks / 6) == 0)
            {
                runningSkill.currentFrame++;
            }
            if(runningSkill.currentTick >= runningSkill.totalTicks)
            {
                runningSkill.currentTick = 0;
                runningSkill.currentLoop++;
                if (runningSkill.isCaster && runningSkill.weapon is not null)
                {
                    DealDamage(runningSkill.currentLocation, runningSkill.center, runningSkill.weapon);
                }
                if(runningSkill.currentLoop >= Config.SkillHits)
                {
                    runningSkill = null;
                    return;
                }
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(skillIconPath))
            {
                e.LoadFromModFile<Texture2D>("assets/skill.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(burstIconPath))
            {
                e.LoadFromModFile<Texture2D>("assets/burst.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (sdiAPI is not null)
            {
                sdiAPI.AddCharacter("Emily", Color.PaleGreen, 0, 20, 10, 80, 5, 10, null, null, skillIconPath, burstIconPath, new List<Action<string, Farmer>>() { SkillEvent }, new List<Action<string, Farmer>>() { BurstEvent });
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            sdiAPI = Helper.ModRegistry.GetApi<IStardewImpactApi>("aedenthorn.StardewImpact");


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
        }


    }
}
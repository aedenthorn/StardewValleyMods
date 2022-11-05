using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;

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

        private static List<RunningSkill> runningSkills = new List<RunningSkill>();
        private static List<RunningBurst> runningBursts = new List<RunningBurst>();

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
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            
            if(runningSkills.Count > 0)
            {
                for(int i = runningSkills.Count - 1; i >= 0; i--)
                {
                    var runningSkill = runningSkills[i];
                    bool here = Game1.currentLocation == runningSkill.currentLocation;

                    if (runningSkill.currentTick == 0)
                    {
                        if (here)
                            runningSkill.currentLocation.playSound("swordswipe");
                        runningSkill.currentFrame = 0;
                        if (runningSkill.intro)
                        {
                            runningSkill.currentEndPos = runningSkill.center;
                            runningSkill.totalTicks = (int)(Config.SkillRadius * 2 / (16 * Config.SkillSpeed));
                            runningSkill.intro = false;
                        }
                        else
                        {
                            runningSkill.currentStartPos = GetRandomPointOnCircle(runningSkill.center, Config.SkillRadius);
                            runningSkill.currentEndPos = runningSkill.center - (runningSkill.currentStartPos - runningSkill.center);
                            runningSkill.totalTicks = (int)(Config.SkillRadius * 2 / (16 * Config.SkillSpeed));
                        }
                    }
                    runningSkill.currentPos = Vector2.Lerp(runningSkill.currentStartPos, runningSkill.currentEndPos, runningSkill.currentTick / (float)runningSkill.totalTicks);
                    runningSkill.currentTick++;
                    if (here)
                        e.SpriteBatch.Draw(SHelper.GameContent.Load<Texture2D>("LooseSprites\\parrots"), Game1.GlobalToLocal(runningSkill.currentPos), new Microsoft.Xna.Framework.Rectangle(48 + runningSkill.currentFrame * 24, 0, 24, 24), Color.White, 0, Vector2.Zero, 4, (runningSkill.currentStartPos.X < runningSkill.currentEndPos.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 1f);
                    if (runningSkill.currentTick % (runningSkill.totalTicks / 6) == 0)
                    {
                        runningSkill.currentFrame++;
                    }
                    if (runningSkill.currentTick >= runningSkill.totalTicks)
                    {
                        runningSkill.currentTick = 0;
                        runningSkill.currentLoop++;
                        if (runningSkill.isCaster && runningSkill.weapon is not null)
                        {
                            DealDamage(runningSkill.currentLocation, runningSkill.center, runningSkill.weapon, runningSkill.color, Config.SkillDamageMult, Config.SkillRadius);
                        }
                        if (runningSkill.currentLoop >= Config.SkillHits)
                        {
                            runningSkills.RemoveAt(i);
                            continue;
                        }
                    }
                    runningSkills[i] = runningSkill;
                }
            }
            if(runningBursts.Count > 0)
            {
                float burstSpeed = Config.BurstSpeed * 8;
                for (int i = runningBursts.Count - 1; i >= 0; i--)
                {
                    var runningBurst = runningBursts[i];
                    bool here = Game1.currentLocation == runningBurst.currentLocation;
                    if (here)
                    {
                        for(int j = 0; j < 4; j++)
                        {
                            var radAngle = runningBurst.currentAngle * Math.PI / 180 + Math.PI / 2 * j;
                            var currentPos = new Vector2(runningBurst.center.X + (float)Math.Cos(radAngle) * runningBurst.currentRadius, runningBurst.center.Y + (float)Math.Sin(radAngle) * runningBurst.currentRadius);
                            e.SpriteBatch.Draw(SHelper.GameContent.Load<Texture2D>("LooseSprites\\parrots"), Game1.GlobalToLocal(currentPos), new Microsoft.Xna.Framework.Rectangle(48 + runningBurst.currentFrame * 24, 0, 24, 24), Color.White, (float)(radAngle + Math.PI / 2), Vector2.Zero, 4, SpriteEffects.FlipHorizontally, 1f);
                        }
                    }
                    runningBurst.currentAngle = (runningBurst.currentAngle + (int)burstSpeed) % 360;
                    if (runningBurst.currentAngle % (360 / 6) < burstSpeed)
                    {
                        runningBurst.currentFrame = (runningBurst.currentFrame + 1) % 6;
                    }
                    if (runningBurst.currentAngle % 90 < burstSpeed)
                    {
                        runningBurst.currentRadius -= burstSpeed * 2;

                        if (runningBurst.isCaster && runningBurst.weapon is not null)
                        {
                            runningBurst.currentLocation.playSound("swordswipe");
                            for (int j = 0; j < runningBurst.currentLocation.characters.Count; j++)
                            {
                                if (runningBurst.currentLocation.characters[j] is Monster && Vector2.Distance(runningBurst.currentLocation.characters[j].Position, runningBurst.center) <= Config.BurstRadius)
                                {
                                    var pos = (runningBurst.currentLocation.characters[j] as Monster).Position;
                                    (runningBurst.currentLocation.characters[j] as Monster).Position = Vector2.Lerp(pos, new Vector2(runningBurst.center.X, runningBurst.center.Y), 64 / Vector2.Distance(pos, runningBurst.center));
                                }
                            }
                            DealDamage(runningBurst.currentLocation, runningBurst.center, runningBurst.weapon, runningBurst.color, Config.BurstDamageMult,  Config.BurstRadius);
                        }

                    }
                    if (runningBurst.currentRadius <= Config.BurstEndRadius * 4)
                    {
                        runningBursts.RemoveAt(i);
                    }
                    else
                    {
                        runningBursts[i] = runningBurst;
                    }
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
                sdiAPI.AddCharacter("Emily", Color.PaleGreen, 20, 10, 80, 5, 10, null, null, skillIconPath, burstIconPath, new List<Action<string, Farmer>>() { SkillEvent }, new List<Action<string, Farmer>>() { BurstEvent });
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
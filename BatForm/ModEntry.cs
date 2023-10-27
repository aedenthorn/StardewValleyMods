using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace BatForm
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        
        public static string batFormKey = "aedenthorn.BatForm";

        public static PerScreen<ICue> batSound = new();
        public static PerScreen<int> height = new();
        public static PerScreen<AnimatedSprite> batSprite = new();

        public enum BatForm
        {
            Inactive,
            SwitchingTo,
            SwitchingFrom,
            Active
        }

        //public static string dictPath = "aedenthorn.BatForm/dictionary";
        //public static Dictionary<string, BatFormData> dataDict = new();

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

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            helper.Events.Player.Warped += Player_Warped;
            
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            ResetBat();
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (!Config.ModEnabled || e.Player != Game1.player || BatFormStatus(e.Player) == BatForm.Inactive)
                return;
            if(Config.OutdoorsOnly && !e.NewLocation.IsOutdoors)
            {
                ResetBat();
            }
            if(Game1.CurrentEvent != null)
            {
                ResetBat();
            }
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            ResetBat();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ResetBat();
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if(!Config.ModEnabled || BatFormStatus(Game1.player) != BatForm.Active || Config.StaminaUse <= 0) 
                return;
            if(Game1.player.Stamina <= Config.StaminaUse)
            {
                TransformBat();
                return;
            }
            Game1.player.Stamina -= Config.StaminaUse;
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled || BatFormStatus(Game1.player) == BatForm.Inactive)
                return;
            if (batSprite.Value is null)
            {
                batSprite.Value = new AnimatedSprite("Characters\\Monsters\\Bat");
            }
            e.SpriteBatch.Draw(batSprite.Value.Texture, Game1.player.getLocalPosition(Game1.viewport) + new Vector2(32f, -height.Value * 8), new Rectangle?(batSprite.Value.SourceRect), Color.White, 0f, new Vector2(8f, 16f), (1 + height.Value / 50f) * 4f, SpriteEffects.None, Game1.player.getStandingY() / 10000 + 0.05f + height.Value / 750f);
            batSprite.Value.Animate(Game1.currentGameTime, 0, 4, 80f);
            if (batSprite.Value.currentFrame % 3 == 0 && Game1.soundBank != null && (batSound.Value is null || !batSound.Value.IsPlaying) && Game1.player.currentLocation == Game1.currentLocation)
            {
                batSound.Value = Game1.soundBank.GetCue("batFlap");
                batSound.Value.Play();
            }
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if(!Config.ModEnabled || !Context.IsWorldReady || Game1.killScreen || Game1.player is null || Game1.player.health <= 0 || Game1.timeOfDay >= 2600 || Game1.eventUp || Game1.CurrentEvent != null)
            {
                ResetBat();
                return;
            }
            var status = BatFormStatus(Game1.player);
            if (status != BatForm.Inactive)
                Game1.player.temporarilyInvincible = true;
            else
                return;
            if (status != BatForm.Active)
            {
                if (status == BatForm.SwitchingFrom)
                {
                    height.Value = Math.Max(0, height.Value - 1);
                    if (height.Value == 0)
                    {
                        PlayTransform();
                        Game1.player.ignoreCollisions = false;
                        Game1.player.modData[batFormKey] = BatForm.Inactive + "";
                    }
                }
                else
                {
                    Game1.player.ignoreCollisions = true;
                    if (height.Value == 0)
                    {
                        PlayTransform();
                    }
                    height.Value = Math.Min(Config.MaxHeight, height.Value + 1);
                    if (height.Value == Config.MaxHeight)
                    {
                        Game1.player.modData[batFormKey] = BatForm.Active + "";
                    }
                }
                Game1.forceSnapOnNextViewportUpdate = true;
                Game1.game1.refreshWindowSettings();
            }
        }

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if(!Config.ModEnabled || !Context.CanPlayerMove || !Config.TransformKey.JustPressed() || BatFormStatus(Game1.player) == BatForm.SwitchingFrom || BatFormStatus(Game1.player) == BatForm.SwitchingTo || (Config.NightOnly && Game1.timeOfDay < 1800) || (Config.OutdoorsOnly && !Game1.player.currentLocation.IsOutdoors)) 
                return;
            TransformBat();
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
                name: () => "Night Only",
                getValue: () => Config.NightOnly,
                setValue: value => Config.NightOnly = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Outdoors Only",
                getValue: () => Config.OutdoorsOnly,
                setValue: value => Config.OutdoorsOnly = value
            );
            
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => "Transform Key",
                getValue: () => Config.TransformKey,
                setValue: value => Config.TransformKey = value
            );
            
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Move Speed",
                getValue: () => Config.MoveSpeed,
                setValue: value => Config.MoveSpeed = value
            );
            
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Stamina Use",
                getValue: () => Config.StaminaUse,
                setValue: value => Config.StaminaUse = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Transform Sound",
                getValue: () => Config.TransformSound,
                setValue: value => Config.TransformSound = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Height",
                getValue: () => Config.MaxHeight,
                setValue: value => Config.MaxHeight = value
            );
        }
    }
}
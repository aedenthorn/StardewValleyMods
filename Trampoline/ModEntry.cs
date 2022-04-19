using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Util;
using System;
using Object = StardewValley.Object;

namespace Trampoline
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static int jumpHeight;
        public static float jumpSpeed;
        public static int jumpTicks;
        public static double lastPoint;
        public static bool goingDown;
        public static bool isEmoting;
        public static bool goingHigher;
        public static bool goingLower;
        public static bool goingSlower;
        public static bool goingFaster;

        private static Texture2D trampolineTexture;

        private static string trampolineKey = "aedenthorn.Trampoline/trampoline";

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (IsOnTrampoline())
            {
                if (jumpTicks  >= (jumpHeight / jumpSpeed) * 2 || jumpHeight < 64)
                {
                    jumpTicks = 0;
                    if (goingLower || Helper.Input.IsDown(Config.LowerKey))
                    {
                        jumpHeight -= 16;
                        if (jumpHeight < 0)
                            jumpHeight = 0;
                        goingLower = false;
                    }
                    else if (goingHigher || Helper.Input.IsDown(Config.HigherKey))
                    {
                        jumpHeight += 16;
                        goingHigher = false;
                    }
                    else if (goingSlower || Helper.Input.IsDown(Config.SlowerKey))
                    {
                        jumpSpeed -= 1;
                        if (jumpSpeed < 1)
                            jumpSpeed = 1;
                        goingSlower = false;
                    }
                    else if (goingFaster || Helper.Input.IsDown(Config.FasterKey))
                    {
                        jumpSpeed += 1;
                        goingFaster = false;
                    }
                }
                if (jumpHeight < 64)
                    return;
                var currentPoint = Math.Sin(jumpTicks / (jumpHeight / jumpSpeed) * Math.PI);
                Game1.player.yOffset = (int)Math.Round((currentPoint + 0.75) * (currentPoint > -1.7 ? jumpHeight : (int)Math.Round(Math.Sqrt(jumpHeight) * 8)));
                if (jumpTicks / (jumpHeight / jumpSpeed) > Math.PI / 2f && lastPoint < Math.PI / 2f)
                {
                    Game1.currentLocation.playSound("bob");
                }
                lastPoint = jumpTicks / (jumpHeight / jumpSpeed);
                jumpTicks++;
                if(currentPoint < 0)
                    jumpTicks++;

                if (jumpHeight >= 128 && currentPoint > -0.25f)
                {
                    isEmoting = true;
                    int facing = Game1.player.FacingDirection;
                    int frame = 94;
                    bool flipped = false;
                    switch (facing)
                    {
                        case 0:
                        case 2:
                            break;
                        case 1:
                            frame = 97;
                            break;
                        case 3:
                            frame = 97;
                            flipped = true;
                            break;
                    }
                    Game1.player.completelyStopAnimatingOrDoingAction();
                    Game1.player.FarmerSprite.setCurrentAnimation(new FarmerSprite.AnimationFrame[]
                        {
                            new FarmerSprite.AnimationFrame(frame, 1500, false, flipped, null, false)
                        }
                    );
                    Game1.player.FarmerSprite.PauseForSingleAnimation = true;
                    Game1.player.FarmerSprite.loop = true;
                    Game1.player.FarmerSprite.loopThisAnimation = true;
                    Game1.player.Sprite.currentFrame = 0;
                }
                else
                {
                    isEmoting = false;
                    Game1.player.completelyStopAnimatingOrDoingAction();
                }

            }
            else
            {
                if(isEmoting)
                {
                    isEmoting = false;
                    Game1.player.completelyStopAnimatingOrDoingAction();
                }
                jumpTicks = 0;
                jumpHeight = 128;
                jumpSpeed = Config.JumpSpeed;
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree)
                return;
            if (IsOnTrampoline())
            {
                KeyboardState currentKBState = Game1.GetKeyboardState();

                if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveUpButton))
                {
                    Game1.player.faceDirection(0);
                }
                else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveDownButton))
                {
                    Game1.player.faceDirection(2);
                }
                else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveRightButton))
                {
                    Game1.player.faceDirection(1);
                }
                else if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveLeftButton))
                {
                    Game1.player.faceDirection(3);
                }
                if (e.Button == Config.HigherKey)
                {
                    goingHigher = true;
                }
                else if (e.Button == Config.LowerKey)
                {
                    goingLower = true;
                }
                else if (e.Button == Config.SlowerKey)
                {
                    goingSlower = true;
                }
                else if (e.Button == Config.FasterKey)
                {
                    goingFaster = true;
                }
            }
            else if(e.Button == Config.ConvertKey)
            {
                foreach (var f in Game1.currentLocation.furniture)
                {
                    if (f.isGroundFurniture() && f.boundingBox.Width == 128 && f.boundingBox.Height == 128 && f.boundingBox.Value.Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()))
                    {
                        if (f.modData.ContainsKey(trampolineKey))
                            f.modData.Remove(trampolineKey);
                        else
                            f.modData[trampolineKey] = "true";
                        Helper.Input.Suppress(e.Button);

                        return;
                    }
                }
            }
        }
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                trampolineTexture = Game1.content.Load<Texture2D>("aedenthorn.Trampoline/trampoline");
                Monitor.Log("Loaded custom pieces sheet");
            }
            catch
            {
                trampolineTexture = Helper.Content.Load<Texture2D>("assets/trampoline.png");
                Monitor.Log("Loaded default pieces sheet");
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Jump Sound",
                getValue: () => Config.JumpSound,
                setValue: value => Config.JumpSound = value
            );
            
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Convert Key",
                getValue: () => Config.ConvertKey,
                setValue: value => Config.ConvertKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Higher Key",
                getValue: () => Config.HigherKey,
                setValue: value => Config.HigherKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Lower Key",
                getValue: () => Config.LowerKey,
                setValue: value => Config.LowerKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Faster Key",
                getValue: () => Config.FasterKey,
                setValue: value => Config.FasterKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Slower Key",
                getValue: () => Config.SlowerKey,
                setValue: value => Config.SlowerKey = value
            );
        }
    }
}
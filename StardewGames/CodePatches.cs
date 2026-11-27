using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Audio;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Mods;
using StardewValley.SDKs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StardewGames
{
	public partial class ModEntry
    {
        public static ClickableTextureComponent playButton;
        public static bool hovering;
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.setUpIcons))]
        public class TitleMenu_setUpIcons_Patch
        {
            public static void Postfix(TitleMenu __instance)
            {
                if (!Config.ModEnabled)
                {
                    return;
                }
                playButton = new ClickableTextureComponent("play games", new Rectangle(__instance.width + -22 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, __instance.height - 25 * TitleMenu.pixelZoom * 3 - 24 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom, 25 * TitleMenu.pixelZoom), null, "", SHelper.ModContent.Load<Texture2D>("assets/play.png"), new Rectangle(0, 0, 27, 25), TitleMenu.pixelZoom, false);
            }
        }
        [HarmonyPatch(typeof(MineCart), new Type[] { typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class MineCart_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling MineCart");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo mi && mi.Name == "RequireCharacter")
                    {
                        SMonitor.Log($"replacing RequireCharacter");
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(RequireCharacter));
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(TitleMenu), "ShouldDrawCursor")]
        public class TitleMenu_draw_Patch
        {
            public static void Prefix(TitleMenu __instance)
            {
                if (!Config.ModEnabled || TitleMenu.subMenu is not null || __instance.isTransitioningButtons || !__instance.titleInPosition || __instance.transitioningCharacterCreationMenu || !__instance.HasActiveUser)
                {
                    return;
                }
                playButton?.draw(Game1.spriteBatch);
                if (hovering)
                {
                    IClickableMenu.drawHoverText(Game1.spriteBatch, SHelper.Translation.Get("games"), Game1.smallFont);
                }
            }
        }
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.receiveLeftClick))]
        public class TitleMenu_receiveLeftClick_Patch
        {
            public static void Postfix(TitleMenu __instance, int x, int y)
            {
                if (!Config.ModEnabled || TitleMenu.subMenu is not null || __instance.isTransitioningButtons)
                {
                    return;
                }
                if (returnToMenu)
                {
                    TitleMenu.subMenu = new GamesLoadGameMenu();
                    returnToMenu = false;
                }
                else if (playButton.visible && playButton.containsPoint(x, y))
                {
                    TitleMenu.subMenu = new GamesLoadGameMenu();
                    Game1.playSound("newArtifact", null);
                }
            }
        }
        [HarmonyPatch(typeof(TitleMenu), nameof(TitleMenu.performHoverAction))]
        public class TitleMenu_performHoverAction_Patch
        {
            public static void Postfix(TitleMenu __instance, int x, int y)
            {
                if (!Config.ModEnabled || (TitleMenu.subMenu is not null && !TitleMenu.subMenu.readyToClose()) || __instance.isTransitioningButtons)
                {
                    return;
                }
                hovering = false;
                if (playButton.visible)
                {
                    playButton.tryHover(x, y, 0.25f);
                    if (playButton.containsPoint(x, y))
                    {
                        if (playButton.sourceRect.X == 0)
                        {
                            Game1.playSound("Cowboy_Footstep", null);
                        }
                        playButton.sourceRect.X = 27;
                        hovering = true;
                        return;
                    }
                    playButton.sourceRect.X = 0;
                }
            }
        }
        [HarmonyPatch(typeof(Game1), "_draw")]
        public class Game1__draw_Patch
        {
            public static bool Prefix(GameTime gameTime, RenderTarget2D target_screen, Game1 __instance, ModHooks ___hooks)
            {
                if (!Config.ModEnabled || currentMiniGame == CurrentMiniGame.None || Game1.currentMinigame == null)
                {
                    return true;
                }
                if (target_screen != null)
                {
                    Game1.SetRenderTarget(target_screen);
                }
                __instance.GraphicsDevice.Clear(Game1.bgColor);
                if (___hooks.OnRendering(RenderSteps.FullScene, Game1.spriteBatch, gameTime, target_screen))
                {
                    if (___hooks.OnRendering(RenderSteps.Minigame, Game1.spriteBatch, gameTime, target_screen))
                    {
                        Game1.currentMinigame.draw(Game1.spriteBatch);
                    }
                    ___hooks.OnRendered(RenderSteps.Minigame, Game1.spriteBatch, gameTime, target_screen);
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(Game1), "_update")]
        public class Game1__update_Patch
        {
            public static bool Prefix(Game1 __instance, GameTime gameTime, ScreenFade ___screenFade)
            {
                if (!Config.ModEnabled || currentMiniGame == CurrentMiniGame.None || Game1.currentMinigame == null)
                {
                    currentMiniGame = CurrentMiniGame.None;
                    return true;
                }

                if (Game1.graphics.GraphicsDevice == null)
                {
                    return false;
                }
                bool zoom_dirty = false;
                var pi = AccessTools.Property(typeof(Game1), "gameModeTicks");
                pi.SetValue(null, (int)pi.GetValue(null) + 1);
                if (Game1.options != null)
                {
                    if (Game1.options.baseUIScale != Game1.options.desiredUIScale)
                    {
                        if (Game1.options.desiredUIScale < 0f)
                        {
                            Game1.options.desiredUIScale = Game1.options.desiredBaseZoomLevel;
                        }
                        Game1.options.baseUIScale = Game1.options.desiredUIScale;
                        zoom_dirty = true;
                    }
                    if (Game1.options.desiredBaseZoomLevel != Game1.options.baseZoomLevel)
                    {
                        Game1.options.baseZoomLevel = Game1.options.desiredBaseZoomLevel;
                        Game1.forceSnapOnNextViewportUpdate = true;
                        zoom_dirty = true;
                    }
                }
                if (zoom_dirty)
                {
                    __instance.refreshWindowSettings();
                }
                __instance.CheckGamepadMode();

                Game1.options.reApplySetOptions();
                if (Game1.toggleFullScreen)
                {
                    Game1.toggleFullscreen();
                    Game1.toggleFullScreen = false;
                }
                Game1.input.Update();

                if (Game1.exitToTitle)
                {
                    Game1.exitToTitle = false;
                    __instance.CleanupReturningToTitle();
                }
                AccessTools.Method(typeof(Game1), "SetFreeCursorElapsed").Invoke(null, new object[] { (float)gameTime.ElapsedGameTime.TotalSeconds });
                ((SDKHelper)AccessTools.Property(typeof(Program), "sdk").GetValue(null)).Update();

                if (Game1.game1.IsMainInstance)
                {
                    Game1.keyboardFocusInstance = Game1.game1;
                    foreach (Game1 instance in GameRunner.instance.gameInstances)
                    {
                        if (instance.instanceKeyboardDispatcher.Subscriber != null && instance.instanceTextEntry != null)
                        {
                            Game1.keyboardFocusInstance = instance;
                            break;
                        }
                    }
                }
                if (__instance.IsMainInstance)
                {
                    int current_display_index = __instance.Window.GetDisplayIndex();
                    var lud = AccessTools.FieldRefAccess<Game1, int>(__instance, "_lastUsedDisplay");
                    if (lud  != -1 && lud != current_display_index)
                    {
                        StartupPreferences startupPreferences = new StartupPreferences();
                        startupPreferences.loadPreferences(false, false);
                        startupPreferences.displayIndex = current_display_index;
                        startupPreferences.savePreferences(false, false);
                    }
                    AccessTools.Field(typeof(Game1), "_lastUsedDisplay").SetValue(__instance, current_display_index);
                }
                if (__instance.HasKeyboardFocus())
                {
                    Game1.keyboardDispatcher.Poll();
                }
                else
                {
                    Game1.keyboardDispatcher.Discard();
                }
                if ((Game1.paused || (!__instance.IsActiveNoOverlay && Program.releaseBuild)) && (Game1.options == null || Game1.options.pauseWhenOutOfFocus || Game1.paused) && Game1.multiplayerMode == 0)
                {
                    AccessTools.Method(typeof(Game1), "UpdateChatBox").Invoke(null, null);
                    return false;
                }
                if (Game1.quit)
                {
                    __instance.Exit();
                }







                Game1.ticks++;
                if (__instance.IsActiveNoOverlay)
                {
                    AccessTools.Method(typeof(Game1), "checkForEscapeKeys").Invoke(__instance, null);
                }
                Game1.updateMusic();
                Game1.updateRaindropPosition();
                if (Game1.globalFade)
                {
                    ___screenFade.UpdateGlobalFade();
                }
                else if (Game1.pauseThenDoFunctionTimer > 0)
                {
                    Game1.freezeControls = true;
                    Game1.pauseThenDoFunctionTimer -= gameTime.ElapsedGameTime.Milliseconds;
                    if (Game1.pauseThenDoFunctionTimer <= 0)
                    {
                        Game1.freezeControls = false;
                        Game1.afterFadeFunction afterFadeFunction = Game1.afterPause;
                        if (afterFadeFunction != null)
                        {
                            afterFadeFunction();
                        }
                    }
                }
                bool flag;
                if (Game1.options.gamepadControls)
                {
                    IClickableMenu activeClickableMenu2 = Game1.activeClickableMenu;
                    flag = ((activeClickableMenu2 != null) ? new bool?(activeClickableMenu2.shouldClampGamePadCursor()) : null) ?? false;
                }
                else
                {
                    flag = false;
                }
                if (flag)
                {
                    Point pos = Game1.getMousePositionRaw();
                    Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, __instance.localMultiplayerWindow.Width, __instance.localMultiplayerWindow.Height);
                    if (pos.X < rect.X)
                    {
                        pos.X = rect.X;
                    }
                    else if (pos.X > rect.Right)
                    {
                        pos.X = rect.Right;
                    }
                    if (pos.Y < rect.Y)
                    {
                        pos.Y = rect.Y;
                    }
                    else if (pos.Y > rect.Bottom)
                    {
                        pos.Y = rect.Bottom;
                    }
                    Game1.setMousePositionRaw(pos.X, pos.Y);
                }




                if (Game1.pauseTime > 0f)
                {
                    Game1.updatePause(gameTime);
                }
                if (Game1.fadeToBlack)
                {
                    ___screenFade.UpdateFadeAlpha(gameTime);
                    if (Game1.fadeToBlackAlpha >= 1f)
                    {
                        Game1.fadeToBlack = false;
                    }
                }
                else
                {
                    if (Game1.thumbstickMotionMargin > 0)
                    {
                        Game1.thumbstickMotionMargin -= gameTime.ElapsedGameTime.Milliseconds;
                    }
                    KeyboardState currentKBState = default(KeyboardState);
                    MouseState currentMouseState = default(MouseState);
                    GamePadState currentPadState = default(GamePadState);

                    currentKBState = Game1.GetKeyboardState();
                    currentMouseState = Game1.input.GetMouseState();
                    currentPadState = Game1.input.GetGamePadState();
                    ChatBox chatBox = Game1.chatBox;

                    if ((((chatBox != null) ? new bool?(chatBox.isActive()) : null) ?? false) || Game1.textEntry != null)
                    {
                        currentKBState = default(KeyboardState);
                        currentPadState = default(GamePadState);
                    }
                    else
                    {
                        foreach (Keys i in currentKBState.GetPressedKeys())
                        {
                            if (!Game1.oldKBState.IsKeyDown(i) && Game1.currentMinigame != null)
                            {
                                Game1.currentMinigame.receiveKeyPress(i);
                            }
                        }
                        if (Game1.options.gamepadControls)
                        {
                            if (Game1.currentMinigame == null)
                            {
                                Game1.oldMouseState = currentMouseState;
                                Game1.oldKBState = currentKBState;
                                Game1.oldPadState = currentPadState;
                                AccessTools.Method(typeof(Game1), "UpdateChatBox").Invoke(null, null);
                                return false;
                            }
                            foreach (Buttons b in Utility.getPressedButtons(currentPadState, Game1.oldPadState))
                            {
                                IMinigame currentMinigame = Game1.currentMinigame;
                                if (currentMinigame != null)
                                {
                                    currentMinigame.receiveKeyPress(Utility.mapGamePadButtonToKey(b));
                                }
                            }
                            if (Game1.currentMinigame == null)
                            {
                                Game1.oldMouseState = currentMouseState;
                                Game1.oldKBState = currentKBState;
                                Game1.oldPadState = currentPadState;
                                AccessTools.Method(typeof(Game1), "UpdateChatBox").Invoke(null, null);
                                return false;
                            }
                            if (currentPadState.ThumbSticks.Right.Y < -0.2f && Game1.oldPadState.ThumbSticks.Right.Y >= -0.2f)
                            {
                                Game1.currentMinigame.receiveKeyPress(Keys.Down);
                            }
                            if (currentPadState.ThumbSticks.Right.Y > 0.2f && Game1.oldPadState.ThumbSticks.Right.Y <= 0.2f)
                            {
                                Game1.currentMinigame.receiveKeyPress(Keys.Up);
                            }
                            if (currentPadState.ThumbSticks.Right.X < -0.2f && Game1.oldPadState.ThumbSticks.Right.X >= -0.2f)
                            {
                                Game1.currentMinigame.receiveKeyPress(Keys.Left);
                            }
                            if (currentPadState.ThumbSticks.Right.X > 0.2f && Game1.oldPadState.ThumbSticks.Right.X <= 0.2f)
                            {
                                Game1.currentMinigame.receiveKeyPress(Keys.Right);
                            }
                            if (Game1.oldPadState.ThumbSticks.Right.Y < -0.2f && currentPadState.ThumbSticks.Right.Y >= -0.2f)
                            {
                                Game1.currentMinigame.receiveKeyRelease(Keys.Down);
                            }
                            if (Game1.oldPadState.ThumbSticks.Right.Y > 0.2f && currentPadState.ThumbSticks.Right.Y <= 0.2f)
                            {
                                Game1.currentMinigame.receiveKeyRelease(Keys.Up);
                            }
                            if (Game1.oldPadState.ThumbSticks.Right.X < -0.2f && currentPadState.ThumbSticks.Right.X >= -0.2f)
                            {
                                Game1.currentMinigame.receiveKeyRelease(Keys.Left);
                            }
                            if (Game1.oldPadState.ThumbSticks.Right.X > 0.2f && currentPadState.ThumbSticks.Right.X <= 0.2f)
                            {
                                Game1.currentMinigame.receiveKeyRelease(Keys.Right);
                            }
                            if (Game1.isGamePadThumbstickInMotion(0.2) && Game1.currentMinigame != null && !Game1.currentMinigame.overrideFreeMouseMovement())
                            {
                                float thumbstickToMouseModifier = (float)AccessTools.Property(typeof(Game1), "thumbstickToMouseModifier").GetValue(null);
                                Game1.setMousePosition(Game1.getMouseX() + (int)(currentPadState.ThumbSticks.Left.X * thumbstickToMouseModifier), Game1.getMouseY() - (int)(currentPadState.ThumbSticks.Left.Y * thumbstickToMouseModifier));
                            }
                            else if (Game1.getMouseX() != Game1.getOldMouseX() || Game1.getMouseY() != Game1.getOldMouseY())
                            {
                                Game1.lastCursorMotionWasMouse = true;
                            }
                        }
                        foreach (Keys j in Game1.oldKBState.GetPressedKeys())
                        {
                            if (!currentKBState.IsKeyDown(j) && Game1.currentMinigame != null)
                            {
                                Game1.currentMinigame.receiveKeyRelease(j);
                            }
                        }
                        if (Game1.options.gamepadControls)
                        {
                            if (Game1.currentMinigame == null)
                            {
                                Game1.oldMouseState = currentMouseState;
                                Game1.oldKBState = currentKBState;
                                Game1.oldPadState = currentPadState;
                                AccessTools.Method(typeof(Game1), "UpdateChatBox").Invoke(null, null);
                                return false;
                            }
                            if (currentPadState.IsConnected)
                            {
                                if (currentPadState.IsButtonDown(Buttons.X) && !Game1.oldPadState.IsButtonDown(Buttons.X))
                                {
                                    Game1.currentMinigame.receiveRightClick(Game1.getMouseX(), Game1.getMouseY(), true);
                                }
                                else if (currentPadState.IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))
                                {
                                    Game1.currentMinigame.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
                                }
                                else if (!currentPadState.IsButtonDown(Buttons.X) && Game1.oldPadState.IsButtonDown(Buttons.X))
                                {
                                    Game1.currentMinigame.releaseRightClick(Game1.getMouseX(), Game1.getMouseY());
                                }
                                else if (!currentPadState.IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonDown(Buttons.A))
                                {
                                    Game1.currentMinigame.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
                                }
                            }
                            foreach (Buttons b2 in Utility.getPressedButtons(Game1.oldPadState, currentPadState))
                            {
                                IMinigame currentMinigame2 = Game1.currentMinigame;
                                if (currentMinigame2 != null)
                                {
                                    currentMinigame2.receiveKeyRelease(Utility.mapGamePadButtonToKey(b2));
                                }
                            }
                            if (currentPadState.IsConnected && currentPadState.IsButtonDown(Buttons.A) && Game1.currentMinigame != null)
                            {
                                Game1.currentMinigame.leftClickHeld(0, 0);
                            }
                        }
                        if (Game1.currentMinigame == null)
                        {
                            Game1.oldMouseState = currentMouseState;
                            Game1.oldKBState = currentKBState;
                            Game1.oldPadState = currentPadState;
                            AccessTools.Method(typeof(Game1), "UpdateChatBox").Invoke(null, null);
                            return false;
                        }
                        if (Game1.currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton != ButtonState.Pressed)
                        {
                            Game1.currentMinigame.receiveLeftClick(Game1.getMouseX(), Game1.getMouseY(), true);
                        }
                        if (Game1.currentMinigame != null && currentMouseState.RightButton == ButtonState.Pressed && Game1.oldMouseState.RightButton != ButtonState.Pressed)
                        {
                            Game1.currentMinigame.receiveRightClick(Game1.getMouseX(), Game1.getMouseY(), true);
                        }
                        if (Game1.currentMinigame != null && currentMouseState.LeftButton == ButtonState.Released && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
                        {
                            Game1.currentMinigame.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
                        }
                        if (Game1.currentMinigame != null && currentMouseState.RightButton == ButtonState.Released && Game1.oldMouseState.RightButton == ButtonState.Pressed)
                        {
                            Game1.currentMinigame.releaseLeftClick(Game1.getMouseX(), Game1.getMouseY());
                        }
                        if (Game1.currentMinigame != null && currentMouseState.LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Pressed)
                        {
                            Game1.currentMinigame.leftClickHeld(Game1.getMouseX(), Game1.getMouseY());
                        }
                    }
                    if (Game1.currentMinigame != null && Game1.currentMinigame.tick(gameTime))
                    {
                        Game1.oldMouseState = currentMouseState;
                        Game1.oldKBState = currentKBState;
                        Game1.oldPadState = currentPadState;
                        IMinigame currentMinigame3 = Game1.currentMinigame;
                        if (currentMinigame3 != null)
                        {
                            currentMinigame3.unload();
                        }
                        Game1.currentMinigame = null;
                        Game1.fadeIn = true;
                        Game1.fadeToBlackAlpha = 1f;
                        AccessTools.Method(typeof(Game1), "UpdateChatBox").Invoke(null, null);
                        return false;
                    }
                    if (Game1.currentMinigame == null && Game1.IsMusicContextActive(MusicContext.MiniGame))
                    {
                        Game1.stopMusicTrack(MusicContext.MiniGame);
                    }
                    Game1.oldMouseState = currentMouseState;
                    Game1.oldKBState = currentKBState;
                    var xx = AbigailGame.waveTimer;
                }

                IAudioEngine audioEngine = Game1.audioEngine;
                if (audioEngine != null)
                {
                    audioEngine.Update();
                }

                return false;
            }
        }
    }
}

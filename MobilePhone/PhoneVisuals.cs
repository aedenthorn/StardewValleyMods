using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;

namespace MobilePhone
{
    public class PhoneVisuals
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;
        private static int ringingTicks;

        // call this method from your Entry class
        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }

        public static void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            Point mousePos = Game1.getMousePosition();
            Point lastMousePos = ModEntry.lastMousePosition;
            ModEntry.lastMousePosition = mousePos;

            if (!ModEntry.phoneOpen)
            {
                ModEntry.appRunning = false;
                ModEntry.runningApp = null;

                if (Config.ShowPhoneIcon && Game1.displayHUD && !Game1.eventUp &&  Game1.gameMode == 3 && !Game1.freezeControls && !Game1.panMode && !Game1.HostPaused && !Game1.game1.takingMapScreenshot)
                {
                    PhoneUtils.CheckIconOffScreen();
                    if (ModEntry.clickingPhoneIcon)
                    {
                        if(Helper.Input.IsSuppressed(SButton.MouseLeft) && lastMousePos != mousePos)
                        {
                            ModEntry.draggingPhoneIcon = true;
                            Config.PhoneIconOffsetX += mousePos.X - lastMousePos.X;
                            Config.PhoneIconOffsetY += mousePos.Y - lastMousePos.Y;
                            ModEntry.phoneIconPosition = PhoneUtils.GetPhoneIconPosition();
                        }
                        else if (!Helper.Input.IsSuppressed(SButton.MouseLeft))
                        {
                            if (!ModEntry.draggingPhoneIcon)
                            {
                                PhoneUtils.TogglePhone(true);
                            }
                            else
                            {
                                Helper.WriteConfig(Config);
                            }
                            ModEntry.clickingPhoneIcon = false;
                            ModEntry.draggingPhoneIcon = false;
                        }
                    }
                    float rot = 0;
                    int speed = 3;
                    if (ModEntry.callingNPC != null && Config.VibratePhoneIcon)
                    {
                        ringingTicks++;

                        rot = ringingTicks % (speed * 4);
                        if (rot < speed)
                            rot *= -1;
                        else if (rot < speed * 3)
                            rot -= speed * 2;
                        else
                            rot = speed * 4 - rot;
                        rot /= 20f;
                    }
                    else
                    {
                        ringingTicks = 0;
                    }
                    e.SpriteBatch.Draw(ModEntry.backgroundTexture, new Vector2((int)ModEntry.phoneIconPosition.X + ModEntry.phoneTexture.Width / 20, (int)ModEntry.phoneIconPosition.Y + ModEntry.phoneTexture.Height / 20), null, Color.White, rot, new Vector2(ModEntry.phoneTexture.Width / 2, ModEntry.phoneTexture.Height / 2), 0.1f, SpriteEffects.None, 0.86f);
                    e.SpriteBatch.Draw(ModEntry.phoneTexture, new Vector2((int)ModEntry.phoneIconPosition.X + ModEntry.phoneTexture.Width / 20, (int)ModEntry.phoneIconPosition.Y + ModEntry.phoneTexture.Height / 20), null, Color.White, rot, new Vector2(ModEntry.phoneTexture.Width / 2, ModEntry.phoneTexture.Height / 2), 0.1f, SpriteEffects.None, 0.86f);
                }

                return;
            }
            else
            {
                ModEntry.clickingPhoneIcon = false;
                ModEntry.draggingPhoneIcon = false;
            }

            if (Game1.game1.takingMapScreenshot)
            {
               
                return;
            }

            if (ModEntry.draggingPhone)
            {
                if (Helper.Input.IsSuppressed(SButton.MouseLeft))
                {
                    if (mousePos != lastMousePos)
                    {
                        int x = mousePos.X - lastMousePos.X;
                        int y = mousePos.Y - lastMousePos.Y;
                        if (ModEntry.phoneRotated)
                        {
                            Config.PhoneRotatedOffsetX += x;
                            Config.PhoneRotatedOffsetY += y;
                        }
                        else
                        {
                            Config.PhoneOffsetX += x;
                            Config.PhoneOffsetY += y;
                        }
                        PhoneUtils.RefreshPhoneLayout();
                    }
                }
                else
                {
                    ModEntry.context.Helper.WriteConfig(Config);
                    ModEntry.draggingPhone = false;
                    Monitor.Log($"released dragging phone");
                }
            }
            else if (Helper.Input.IsSuppressed(SButton.MouseLeft) && !ModEntry.movingAppIcon)
            {
                int dy = mousePos.Y - lastMousePos.Y;
                if (Math.Abs(dy) > 0 && ModEntry.screenRect.Contains(mousePos))
                {
                    ModEntry.clickingApp = -1;
                    ModEntry.draggingIcons = true;
                }
                if (ModEntry.draggingIcons)
                {
                    ModEntry.yOffset = (int)Math.Max(Math.Min(0, ModEntry.yOffset + dy), -1 * Math.Max(0, ModEntry.listHeight - PhoneUtils.GetScreenSize().Y));
                }
            }

            e.SpriteBatch.Draw(ModEntry.phoneRotated ? ModEntry.backgroundRotatedTexture : ModEntry.backgroundTexture, ModEntry.phoneRect, Color.White);
            e.SpriteBatch.Draw(ModEntry.phoneRotated ? ModEntry.phoneRotatedTexture : ModEntry.phoneTexture, ModEntry.phoneRect, Color.White);

            Vector2 screenPos = PhoneUtils.GetScreenPosition();
            Vector2 screenSize = PhoneUtils.GetScreenSize();
            if (ModEntry.callingNPC != null)
            {
                Rectangle destRect;
                float scale;
                if (screenSize.X > screenSize.Y - Config.AppHeaderHeight)
                {
                    scale = screenSize.Y - Config.AppHeaderHeight;
                    destRect = new Rectangle((int)(screenPos.X + screenSize.X / 2f - scale / 2f), (int)screenPos.Y, (int)scale, (int)screenSize.Y - Config.AppHeaderHeight);
                }
                else
                {
                    scale = screenSize.X;
                    destRect = new Rectangle((int)(screenPos.X), (int)(screenPos.Y + (screenSize.Y - Config.AppHeaderHeight) / 2f - scale / 2f), (int)screenSize.X, (int)scale);
                }
                if (ModEntry.iHDPortraitsAPI == null)
                {
                    Rectangle portraitSource = new Rectangle(0, 0, 64, 64);
                    e.SpriteBatch.Draw(ModEntry.callingNPC.Portrait, destRect, new Rectangle?(portraitSource), Color.White);
                }
                else
                {
                    ModEntry.iHDPortraitsAPI.DrawPortrait(e.SpriteBatch, ModEntry.callingNPC, 0, destRect);
                }

                SpriteText.drawStringHorizontallyCenteredAt(e.SpriteBatch, ModEntry.callingNPC.getName(), destRect.X + destRect.Width / 2, destRect.Bottom + 16, 999999, -1, 999999, 1f, 0.88f, false, null, 99999);

                if (!ModEntry.inCall)
                {
                    Rectangle answerRect = new Rectangle((int)(screenPos.X), ModEntry.screenRect.Bottom - Config.AppHeaderHeight, (int)(screenSize.X / 2f), Config.AppHeaderHeight);
                    Rectangle declineRect = new Rectangle((int)(screenPos.X + screenSize.X / 2f), ModEntry.screenRect.Bottom - Config.AppHeaderHeight, (int)(screenSize.X / 2f), Config.AppHeaderHeight);
                    e.SpriteBatch.Draw(ModEntry.answerTexture, answerRect, Color.White);
                    e.SpriteBatch.Draw(ModEntry.declineTexture, declineRect, Color.White);
                    float textScale = Config.CallTextScale;
                    string ans = Helper.Translation.Get("answer");
                    Vector2 ansSize = Game1.dialogueFont.MeasureString(ans) * textScale;
                    string dec = Helper.Translation.Get("decline");
                    Vector2 decSize = Game1.dialogueFont.MeasureString(dec) * textScale;
                    e.SpriteBatch.DrawString(Game1.dialogueFont, ans, new Vector2(answerRect.X + answerRect.Width / 2f - ansSize.X / 2f, answerRect.Top + answerRect.Height / 2f - ansSize.Y / 2f), Config.CallTextColor, 0f, Vector2.Zero, textScale, SpriteEffects.None,1f);
                    e.SpriteBatch.DrawString(Game1.dialogueFont, dec, new Vector2(declineRect.X + declineRect.Width / 2f - decSize.X / 2f, declineRect.Top + declineRect.Height / 2f - decSize.Y / 2f), Config.CallTextColor, 0f, Vector2.Zero, textScale, SpriteEffects.None, 1f);
                    if (ModEntry.clicking && !Helper.Input.IsSuppressed(SButton.MouseLeft))
                    {
                        if (answerRect.Contains(mousePos))
                        {
                            PhoneUtils.StopRingTone();

                            MobilePhoneApp.CallNPC(ModEntry.callingNPC);
                            ModEntry.currentCallRings = 0;
                        }
                        else if (declineRect.Contains(mousePos))
                        {
                            PhoneUtils.StopRingTone();

                            ModEntry.currentCallRings = 0;
                            ModEntry.callingNPC = null;

                        }
                    }
                }
                else if(!ModEntry.buildingInCall)
                {
                    Rectangle endRect = new Rectangle((int)(screenPos.X + screenSize.X / 4), ModEntry.screenRect.Bottom - Config.AppHeaderHeight, (int)(screenSize.X / 2), Config.AppHeaderHeight);
                    e.SpriteBatch.Draw(ModEntry.declineTexture, endRect, Color.White);
                    float textScale = Config.CallTextScale;
                    string ends = Helper.Translation.Get("end-call");
                    Vector2 endsSize = Game1.dialogueFont.MeasureString(ends) * textScale;
                    e.SpriteBatch.DrawString(Game1.dialogueFont, ends, new Vector2(endRect.X + endRect.Width / 2f - endsSize.X / 2f, endRect.Top + endRect.Height / 2f - endsSize.Y / 2f), Config.CallTextColor, 0f, Vector2.Zero, textScale, SpriteEffects.None,1f);
                    if (ModEntry.clicking && !Helper.Input.IsSuppressed(SButton.MouseLeft))
                    {
                        if (endRect.Contains(mousePos) && !ModEntry.buildingInCall)
                        {
                            if (ModEntry.isReminiscing)
                            {
                                Game1.CurrentEvent?.skipEvent();
                                //ModEntry.reminisceEvent = null;
                                ModEntry.isReminiscing = false;
                            }
                            MobilePhoneCall.EndCall();
                            if (Game1.activeClickableMenu is DialogueBox)
                                Game1.activeClickableMenu = null;
                        }
                    }
                }
                if ((ModEntry.clicking || ModEntry.clickingPhoneIcon) && !Helper.Input.IsSuppressed(SButton.MouseLeft))
                {
                    ModEntry.clickingApp = -1;
                    ModEntry.switchingApp = -1;
                    ModEntry.movingAppIconOffset = new Point(0, 0);
                    ModEntry.clickingTicks = 0;
                    ModEntry.clicking = false;
                    ModEntry.movingAppIcon = false;
                    ModEntry.clickingPhoneIcon = false;
                    ModEntry.draggingPhone = false;
                    ModEntry.draggingIcons = false;
                }

                return;
            }

            if (ModEntry.appRunning)
            {
                return;
            }
            if (ModEntry.runningApp == Helper.ModRegistry.ModID && Game1.activeClickableMenu == null)
            {
                MobilePhoneApp.OpenPhoneBook();
                return;
            }

            List<string> keys = new List<string>(ModEntry.appOrder);

            if ((ModEntry.clicking || ModEntry.clickingPhoneIcon) && !Helper.Input.IsSuppressed(SButton.MouseLeft))
            {
                Monitor.Log($"released click");
                if (ModEntry.movingAppIcon && ModEntry.switchingApp != -1)
                {
                    Monitor.Log($"switching app: {ModEntry.switchingApp} clicking app {ModEntry.clickingApp}");
                    Game1.playSound("stoneStep");
                    ModEntry.appOrder[ModEntry.clickingApp] = keys[ModEntry.switchingApp];
                    ModEntry.appOrder[ModEntry.switchingApp] = keys[ModEntry.clickingApp];
                    keys = new List<string>(ModEntry.appOrder);
                    Config.AppList = keys.ToArray();
                    Helper.WriteConfig(Config);
                }
                else if (ModEntry.draggingIcons)
                {
                    ModEntry.draggingIcons = false;
                }
                else if(!ModEntry.movingAppIcon)
                {
                    for (int i = 0; i < keys.Count; i++)
                    {
                        Vector2 pos = PhoneUtils.GetAppPos(i);
                        Rectangle r = new Rectangle((int)pos.X, (int)pos.Y, Config.IconWidth, Config.IconHeight);
                        if (r.Contains(mousePos))
                        {
                            if(keys[i] == Helper.ModRegistry.ModID + "_Rotate")
                            {
                                Monitor.Log($"rotating phone app");
                                PhoneUtils.RotatePhone();
                            }
                            else if (ModEntry.apps[keys[i]].keyPress != null)
                            {
                                Monitor.Log($"pressing key {ModEntry.apps[keys[i]].keyPress}");
                                PhoneInput.PressKey(ModEntry.apps[keys[i]]);
                            }
                            else
                            {
                                Monitor.Log($"starting app {ModEntry.apps[keys[i]].name}");
                                ModEntry.apps[keys[i]].action?.Invoke();
                            }
                        }
                    }
                }
                ModEntry.clickingApp = -1;
                ModEntry.switchingApp = -1;
                ModEntry.movingAppIconOffset = new Point(0, 0);
                ModEntry.clickingTicks = 0;
                ModEntry.clicking = false;
                ModEntry.movingAppIcon = false;
                ModEntry.clickingPhoneIcon = false;
                ModEntry.draggingPhone = false;
                ModEntry.draggingIcons = false;
            }
            else if (ModEntry.clicking && ModEntry.clickingApp != -1 && !ModEntry.movingAppIcon)
            {
                
                if (lastMousePos == mousePos)
                {
                    if (ModEntry.clickingTicks > Config.TicksToMoveAppIcon)
                    {
                        Game1.playSound("pickUpItem");
                        ModEntry.movingAppIconOffset = new Point(5,5);
                        ModEntry.movingAppIcon = true;
                    }
                    else
                    {
                        ModEntry.clickingTicks++;
                    }
                }
                else
                {
                    ModEntry.clickingTicks = 0;
                    ModEntry.clickingApp = -1;
                }
            }
            else if (ModEntry.movingAppIcon)
            {
                ModEntry.movingAppIconOffset = new Point(ModEntry.movingAppIconOffset.X + mousePos.X - lastMousePos.X, ModEntry.movingAppIconOffset.Y + mousePos.Y - lastMousePos.Y);
                Vector2 currentPos = PhoneUtils.GetAppPos(ModEntry.clickingApp);
                if (ModEntry.screenRect.Contains(Utility.Vector2ToPoint(currentPos)))
                {
                    bool alreadySwitched = false;
                    if(ModEntry.switchingApp != -1)
                    {
                        Vector2 pos = PhoneUtils.GetAppPos(ModEntry.switchingApp, true);
                        if (Vector2.Distance(currentPos, pos) < (Config.IconWidth + Config.IconHeight) / 4f)
                            alreadySwitched = true;
                    }
                    if (!alreadySwitched)
                    {
                        for (int i = 0; i < keys.Count; i++)
                        {
                            Vector2 pos = PhoneUtils.GetAppPos(i);
                            if (i != ModEntry.clickingApp && Vector2.Distance(currentPos, pos) < (Config.IconWidth + Config.IconHeight) / 4f)
                            {
                                ModEntry.switchingApp = i;
                                Monitor.Log($"new switching app: {ModEntry.switchingApp} clicking app {ModEntry.clickingApp}");
                                break;
                            }
                            else
                            {
                                ModEntry.switchingApp = -1;
                            }
                        }
                    }
                }
            }


            string appHover = null;
            bool hover = false;
            if (mousePos == lastMousePos && !Helper.Input.IsSuppressed(SButton.MouseLeft))
            {
                hover = true;
            }
            else
            {
                ModEntry.ticksSinceMoved = 0;
            }

            int screenBottom = (int)(screenPos.Y + screenSize.Y);
            for (int i = keys.Count - 1; i >= 0; i--)
            {

                MobileApp app = ModEntry.apps[keys[i]];
                
                Vector2 appPos = PhoneUtils.GetAppPos(i);
                Rectangle r = new Rectangle(0, 0, Config.IconWidth, Config.IconHeight);
                Rectangle sourceRect = r;
                if (!ModEntry.movingAppIcon || i != ModEntry.clickingApp)
                {
                    if (appPos.Y < screenPos.Y - r.Height * 2 || appPos.Y >= screenBottom)
                    {
                        continue;
                    }
                    if (appPos.Y < screenPos.Y)
                    {
                        int cutTop = (int)screenPos.Y - (int)appPos.Y;
                        sourceRect = new Rectangle(r.X, r.Y + cutTop, r.Width, r.Height - cutTop);
                        appPos = new Vector2(appPos.X, screenPos.Y);
                    }
                    else if (appPos.Y > screenBottom - r.Height)
                    {
                        int cutBottom = screenBottom - r.Height - (int)appPos.Y;
                        sourceRect = new Rectangle(r.X, r.Y, r.Width, r.Height + cutBottom);
                    }
                }

                e.SpriteBatch.Draw(app.icon, new Rectangle((int)appPos.X, (int)appPos.Y, sourceRect.Width, sourceRect.Height), sourceRect, Color.White, 0, Vector2.Zero, SpriteEffects.None, i == ModEntry.clickingApp && ModEntry.movingAppIcon ? 1f : 0.5f);

                Rectangle rect = new Rectangle((int)appPos.X, (int)appPos.Y, Config.IconWidth, sourceRect.Height);
                if (hover && !Helper.Input.IsSuppressed(SButton.MouseLeft) && rect.Contains(mousePos))
                {
                    ModEntry.ticksSinceMoved++;
                    if (ModEntry.ticksSinceMoved > Config.ToolTipDelayTicks)
                        appHover = app.name;
                }
            }
            if (appHover != null)
            {
                e.SpriteBatch.DrawString(Game1.dialogueFont, appHover, new Vector2(mousePos.X, mousePos.Y) - Game1.dialogueFont.MeasureString(appHover) + new Vector2(-2, 2), Color.Black);
                e.SpriteBatch.DrawString(Game1.dialogueFont, appHover, new Vector2(mousePos.X, mousePos.Y) - Game1.dialogueFont.MeasureString(appHover), Color.White);
            }

        }


        public static void Display_WindowResized(object sender, StardewModdingAPI.Events.WindowResizedEventArgs e)
        {
            PhoneUtils.RefreshPhoneLayout();
        }

        public static void CreatePhoneTextures()
        {
            if (ThemeApp.skinDict.ContainsKey(Config.PhoneSkinPath))
            {
                ModEntry.phoneTexture = ThemeApp.skinDict[Config.PhoneSkinPath][0];
                ModEntry.phoneRotatedTexture = ThemeApp.skinDict[Config.PhoneSkinPath][1];
            }
            else
            {
                ModEntry.phoneTexture = ThemeApp.skinDict[Path.Combine("assets", "skins", "black.png")][0];
                ModEntry.phoneRotatedTexture = ThemeApp.skinDict[Path.Combine("assets", "skins", "black.png")][1];
            }
            if (ThemeApp.backgroundDict.ContainsKey(Config.BackgroundPath))
            {
                ModEntry.backgroundTexture = ThemeApp.backgroundDict[Config.BackgroundPath][0];
                ModEntry.backgroundRotatedTexture = ThemeApp.backgroundDict[Config.BackgroundPath][1];
            }
            else
            {
                ModEntry.backgroundTexture = ThemeApp.backgroundDict[Path.Combine("assets", "backgrounds", "clouds.png")][0];
                ModEntry.backgroundRotatedTexture = ThemeApp.backgroundDict[Path.Combine("assets", "backgrounds", "clouds.png")][1];
            }
            ModEntry.upArrowTexture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", Config.UpArrowTexturePath));
            ModEntry.downArrowTexture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", Config.DownArrowTexturePath));
            if (Config.ShowPhoneIcon)
            {
                ModEntry.iconTexture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", Config.iconTexturePath));
            }
        }

        public static Texture2D MakeColorTexture(Color color, Vector2 size)
        {
            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)size.X, (int)size.Y);
            Color[] data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = color;
            }
            texture.SetData(data);
            return texture;
        }

    }
}

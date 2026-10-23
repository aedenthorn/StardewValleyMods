using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;

namespace ImmersiveToolbar
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressSwitchToolButton))]
        public static class Game1_pressSwitchToolButton_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;
                int whichWay = ((Game1.input.GetMouseState().ScrollWheelValue > Game1.oldMouseState.ScrollWheelValue) ? (-1) : ((Game1.input.GetMouseState().ScrollWheelValue < Game1.oldMouseState.ScrollWheelValue) ? 1 : 0));
                if (Game1.options.gamepadControls && whichWay == 0)
                {
                    if (Game1.input.GetGamePadState().IsButtonDown(Buttons.LeftTrigger))
                    {
                        whichWay = -1;
                    }
                    else if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightTrigger))
                    {
                        whichWay = 1;
                    }
                }
                if (Game1.options.invertScrollDirection)
                {
                    whichWay *= -1;
                }
                if (whichWay == 0)
                {
                    return;
                }
                scroll.Value = 96 * whichWay;
            }
        }
        [HarmonyPatch(typeof(Toolbar), nameof(Toolbar.receiveLeftClick))]
        public static class Toolbar_receiveLeftClick_Patch
        {
            public static bool Prefix()
            {
                return !Config.ModEnabled;
            }
        }
        [HarmonyPatch(typeof(Toolbar), nameof(Toolbar.receiveRightClick))]
        public static class Toolbar_receiveRightClick_Patch
        {
            public static bool Prefix()
            {
                return !Config.ModEnabled;
            }
        }
        [HarmonyPatch(typeof(Toolbar), nameof(Toolbar.performHoverAction))]
        public static class Toolbar_performHoverAction_Patch
        {
            public static bool Prefix(Toolbar __instance, ref bool ___hoverDirty, int x, int y)
            {
                if (!Config.ModEnabled)
                    return true;
                if (___hoverDirty)
                {
                    __instance.gameWindowSizeChanged(new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height), new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
                    ___hoverDirty = false;
                }
                __instance.hoverItem = null;

                Rectangle r = new(Game1.uiViewport.Width / 2 - 48 - 16, __instance.yPositionOnScreen - 96 - 8, 96, 96);
                if (r.Contains(x, y))
                {
                    int index = Game1.player.CurrentToolIndex;
                    if (Game1.player.Items.Count > index && Game1.player.Items[index] != null)
                    {
                        __instance.hoverItem = Game1.player.Items[index];
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(Toolbar), nameof(Toolbar.draw), new Type[] { typeof(SpriteBatch) })]
        public static class Toolbar_draw_Patch
        {
            public static bool Prefix(Toolbar __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return true;
                if (Game1.activeClickableMenu != null)
                {
                    return false;
                }
                Point playerGlobalPos = Game1.player.StandingPixel;
                Vector2 playerGlobalVec = new Vector2((float)playerGlobalPos.X, (float)playerGlobalPos.Y);
                Vector2 playerLocalVec = Game1.GlobalToLocal(Game1.viewport, playerGlobalVec);
                bool alignTop;
                if (Game1.options.pinToolbarToggle)
                {
                    alignTop = false;
                    __instance.transparency = Math.Min(1f, __instance.transparency + 0.075f);
                    if (playerLocalVec.Y > (float)(Game1.viewport.Height - 192))
                    {
                        __instance.transparency = Math.Max(0.33f, __instance.transparency - 0.15f);
                    }
                }
                else
                {
                    alignTop = playerLocalVec.Y > (float)(Game1.viewport.Height / 2 + 64);
                    __instance.transparency = 1f;
                }
                int margin = Utility.makeSafeMarginY(8);
                int num = __instance.yPositionOnScreen;
                if (!alignTop)
                {
                    __instance.yPositionOnScreen = Game1.uiViewport.Height;
                    __instance.yPositionOnScreen += 8;
                    __instance.yPositionOnScreen -= margin;
                }
                else
                {
                    __instance.yPositionOnScreen = 112;
                    __instance.yPositionOnScreen -= 8;
                    __instance.yPositionOnScreen += margin;
                }
                if (num != __instance.yPositionOnScreen)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        __instance.buttons[i].bounds.Y = __instance.yPositionOnScreen - 96 + 8;
                    }
                }
                float fadeSpeed = 15;
                float opacity = fadeTimer.Value <= 0 ? (Config.FadeOpacity + (1 - Config.FadeOpacity) * (fade.Value / fadeSpeed)) : 1f;
                Vector2 toDraw = new Vector2((float)(Game1.uiViewport.Width / 2 - 48), (float)(__instance.yPositionOnScreen - 96 + 8));
                var index = Game1.player.CurrentToolIndex;
                if (Config.BackgroundOpacity > 0)
                {
                    IClickableMenu.drawTextureBox(b, Game1.menuTexture, __instance.toolbarTextSource, Game1.uiViewport.Width / 2 - 48 - 16, __instance.yPositionOnScreen - 96 - 8, 96, 96, Color.White * __instance.transparency * opacity * Config.BackgroundOpacity, 1f, false, -1f);
                    b.Draw(Game1.menuTexture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), Color.White * __instance.transparency * opacity);
                }
                //if (Config.DrawBorder)
                //{
                //    b.Draw(Game1.menuTexture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 56, -1, -1)), Color.White * __instance.transparency * opacity);
                //}

                if (scroll.Value != 0)
                {
                    toDraw.X += scroll.Value;
                    float shift = (scroll.Value > 0 ? 1 : -1) * Config.ScrollSpeedPercent / 100f * 2;
                    if (Math.Sign(scroll.Value - shift) != Math.Sign(scroll.Value))
                    {
                        scroll.Value = 0;
                    }
                    else
                    {
                        scroll.Value -= shift * 4;
                    }
                    fadeTimer.Value = Config.FadeDelay;
                    fade.Value = fadeSpeed;
                }
                else
                {
                    if (fadeTimer.Value > 0)
                    {
                        fadeTimer.Value--;
                    }
                }
                if (Game1.player.Items.Count > index && Game1.player.Items[index] != null)
                {
                    Game1.player.Items[index].drawInMenu(b, toDraw, 0.9f, __instance.transparency * opacity, 0.88f);
                    if (!Game1.options.gamepadControls && Config.ShowNumbers)
                    {
                        b.DrawString(Game1.tinyFont, __instance.slotText[index], toDraw + new Vector2(4f, -8f), Color.DimGray * __instance.transparency);
                    }
                }
                if (fadeTimer.Value > 0 || Config.AlwaysShowAdjacent)
                {
                    for (int i = 1; i < 13; i++)
                    {
                        int idx = (index + i) % 12;
                        if (Game1.player.Items.Count > idx && Game1.player.Items[idx] != null)
                        {
                            Game1.player.Items[idx].drawInMenu(b, toDraw + new Vector2(96, 0f), 0.8f, __instance.transparency * opacity * Config.AdjacentOpacity, 0.88f);
                            if (!Game1.options.gamepadControls && Config.ShowNumbers)
                            {
                                b.DrawString(Game1.tinyFont, __instance.slotText[idx], toDraw + new Vector2(96, 0f) + new Vector2(4f, -8f), Color.DimGray * __instance.transparency);
                            }
                            if (scroll.Value < 0)
                            {
                                for (int j = 1; j < 13; j++)
                                {
                                    int idx2 = (idx + j) % 12;
                                    if (Game1.player.Items.Count > idx2 && Game1.player.Items[idx2] != null)
                                    {
                                        Game1.player.Items[idx2].drawInMenu(b, toDraw + new Vector2(192, 0f), 0.8f, __instance.transparency * opacity * Config.AdjacentOpacity * (Math.Abs(scroll.Value) / 96f), 0.88f);
                                        if (!Game1.options.gamepadControls && Config.ShowNumbers)
                                        {
                                            b.DrawString(Game1.tinyFont, __instance.slotText[idx2], toDraw + new Vector2(192, 0f) + new Vector2(4f, -8f), Color.DimGray * __instance.transparency);
                                        }
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                    for (int i = 1; i < 13; i++)
                    {
                        int idx = index - i;
                        if(idx < 0)
                        {
                            idx += 12;
                        }
                        if (Game1.player.Items.Count > idx && Game1.player.Items[idx] != null)
                        {
                            Game1.player.Items[idx].drawInMenu(b, toDraw - new Vector2(96, 0f), 0.8f, __instance.transparency * opacity * Config.AdjacentOpacity, 0.88f);
                            if (!Game1.options.gamepadControls && Config.ShowNumbers)
                            {
                                b.DrawString(Game1.tinyFont, __instance.slotText[idx], toDraw - new Vector2(96, 0f) + new Vector2(4f, -8f), Color.DimGray * __instance.transparency);
                            }
                            if (scroll.Value > 0)
                            {
                                for (int j = 1; j < 13; j++)
                                {
                                    int idx2 = idx - i;
                                    if (idx2 < 0)
                                    {
                                        idx2 += 12;
                                    }
                                    if (Game1.player.Items.Count > idx2 && Game1.player.Items[idx2] != null)
                                    {
                                        Game1.player.Items[idx2].drawInMenu(b, toDraw - new Vector2(192, 0f), 0.8f, __instance.transparency * opacity * Config.AdjacentOpacity * (Math.Abs(scroll.Value) / 96f), 0.88f);
                                        if (!Game1.options.gamepadControls && Config.ShowNumbers)
                                        {
                                            b.DrawString(Game1.tinyFont, __instance.slotText[idx2], toDraw - new Vector2(192, 0f) + new Vector2(4f, -8f), Color.DimGray * __instance.transparency);
                                        }
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                else if (fade.Value > 0)
                {
                    fade.Value--;
                }


                if (__instance.hoverItem != null)
                {
                    IClickableMenu.drawToolTip(b, __instance.hoverItem.getDescription(), __instance.hoverItem.DisplayName, __instance.hoverItem, false, -1, 0, null, -1, null, -1, null);
                    __instance.hoverItem = null;
                }
                return false;
            }
        }
    }
}
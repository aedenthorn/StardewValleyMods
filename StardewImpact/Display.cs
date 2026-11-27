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
    public partial class ModEntry
    {

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || Game1.player.CurrentTool is not MeleeWeapon)
                return;
            var scale = Config.PortraitScale;
            var rects = GetCharacterRectangles();
            int currentSlot = -1;
            if (Game1.player.modData.TryGetValue(currentSlotKey, out string cs))
            {
                int.TryParse(cs, out currentSlot);
            }
            for (int i = 0; i < rects.Count; i++)
            {
                var pos = rects[i].Location.ToVector2();
                e.SpriteBatch.Draw(backTexture, pos, new Rectangle(0,0,frameTexture.Width, frameTexture.Height), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1f);
                if (Game1.player.modData.TryGetValue(slotPrefix + (i + 1), out string name) && characterDict.TryGetValue(name, out CharacterData data) && data.Portrait is not null)
                {
                    e.SpriteBatch.Draw(data.Portrait, pos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1.02f);
                    e.SpriteBatch.Draw(frameTexture, pos, null, data.CharacterColor, 0, Vector2.Zero, scale, SpriteEffects.None, 1.03f);

                    var burstAlpha = data.BurstCooldownValue > 0 ? 0.5f : 1f;

                    if (i + 1 == currentSlot)
                    {
                        var skillAlpha = data.SkillCooldownValue > 0 ? 0.5f : 1f;
                        var burstPos = new Vector2(Game1.viewport.Width - Config.CurrentSkillOffsetX, Game1.viewport.Height - Config.CurrentSkillOffsetY) - new Vector2(data.BurstIcon.Width * scale, data.BurstIcon.Height * scale);
                        var skillPos = burstPos - new Vector2(data.SkillIcon.Width * scale + Config.PortraitSpacing, 0);

                        if(data.SkillEvent.Count > 0)
                        {
                            // draw skill

                            if (data.SkillCooldownValue > 0)
                            {
                                e.SpriteBatch.Draw(whiteTexture, skillPos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1f);
                                string scd = string.Format("{0:0.0}", data.SkillCooldownValue);
                                e.SpriteBatch.Draw(data.SkillIcon, skillPos, null, data.CharacterColor * skillAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, 1.03f);
                                SpriteText.drawStringHorizontallyCenteredAt(e.SpriteBatch, scd, (int)(skillPos.X + data.SkillIcon.Width * scale / 2), (int)(skillPos.Y + (data.SkillIcon.Height * scale - SpriteText.getHeightOfString(scd)) / 2), 999999, -1, 999999, 1, 1.04f, false, 1, 99999);
                            }
                            else
                            {
                                e.SpriteBatch.Draw(data.SkillIcon, skillPos, null, Utility.Get2PhaseColor(Color.White, data.CharacterColor) * skillAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, 1.03f);
                            }
                        }

                        if (data.BurstEvent.Count > 0)
                        {


                            // draw burst

                            if (data.BurstCooldownValue > 0)
                            {
                                e.SpriteBatch.Draw(whiteTexture, burstPos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1f);
                            }

                            if (data.CurrentEnergy <= 0)
                            {
                                e.SpriteBatch.Draw(data.BurstIcon, burstPos, null, Color.White * burstAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, 1.03f);
                            }
                            else if (data.CurrentEnergy >= data.BurstEnergyCost)
                            {
                                e.SpriteBatch.Draw(data.BurstIcon, burstPos, null, Utility.Get2PhaseColor(Color.White, data.CharacterColor, 0, 3, 0) * burstAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, 1.03f);
                            }
                            else
                            {
                                int offset = (int)Math.Round(data.BurstIcon.Height * (1 - (data.CurrentEnergy / data.BurstEnergyCost)));
                                e.SpriteBatch.Draw(data.BurstIcon, burstPos, new Rectangle(0, 0, data.BurstIcon.Width, offset), Color.White * burstAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, 1.02f);
                                e.SpriteBatch.Draw(data.BurstIcon, burstPos + new Vector2(0, offset * scale), new Rectangle(0, offset, data.BurstIcon.Width, data.BurstIcon.Height - offset), data.CharacterColor * burstAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, 1.02f);
                            }
                            if (data.BurstCooldownValue > 0)
                            {
                                string bcd = string.Format("{0:0.0}", data.BurstCooldownValue);
                                SpriteText.drawStringHorizontallyCenteredAt(e.SpriteBatch, bcd, (int)(burstPos.X + data.BurstIcon.Width * scale / 2), (int)(burstPos.Y + (data.BurstIcon.Height * scale - SpriteText.getHeightOfString(bcd)) / 2), 999999, -1, 999999, 1, 1.04f, false, 1, 99999);
                            }
                        }
                    }
                    else
                    {
                        if (data.BurstEvent.Count > 0 && data.CurrentEnergy >= data.BurstEnergyCost)
                        {
                            e.SpriteBatch.Draw(data.BurstIcon, pos - new Vector2(frameTexture.Width * scale + Config.PortraitSpacing, 0), null, Utility.Get2PhaseColor(Color.White, data.CharacterColor) * burstAlpha, 0, Vector2.Zero, scale, SpriteEffects.None, 1.03f);
                        }
                    }
                    
                }
                else
                {
                    e.SpriteBatch.Draw(frameTexture, pos, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1.03f);
                }
            }
        }

    }
}
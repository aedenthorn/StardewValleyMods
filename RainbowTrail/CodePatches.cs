using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RainbowTrail
{
    public partial class ModEntry
    {
        public static int buffId = 4277377;

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static void Prefix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;

                bool active = RainbowTrailStatus(__instance);
                bool exists = trailDict.TryGetValue(__instance.UniqueMultiplayerID, out var set);
                float max = Config.MaxLength;
                int d = __instance.FacingDirection;
                var pos = __instance.Position;

                if (active)
                {
                    if (Game1.player == __instance && Config.MoveSpeed != 0)
                    {
                        Buff? buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == buffId);
                        if (buff == null)
                        {
                            Game1.buffsDisplay.addOtherBuff(
                                buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, speed: Config.MoveSpeed, 0, 0, minutesDuration: 1, source: "Rainbow Trail Mod", displaySource: "Rainbow Trail") { which = buffId }
                            );
                        }
                        buff.millisecondsDuration = 50;
                    }
                }
                if (!exists)
                {
                    if (!active)
                        return;
                    set = new Queue<PositionInfo>();
                    trailDict.Add(__instance.UniqueMultiplayerID, set);
                }
                if (set.Count < max + 1)
                    set.Enqueue(new PositionInfo(pos, d));
                if(rainbowTexture is null)
                {
                    rainbowTexture = SHelper.GameContent.Load<Texture2D>(rainbowTrailKey);
                }
                for (int i = 0; i < set.Count; i++)
                {
                    Vector2 p = set.ElementAt(i).position;
                    int ed = set.ElementAt(i).direction;
                    if (i == 0 || set.ElementAt(i - 1).position != p)
                    {
                        if (d == 0 && i > max - 1)
                            continue;
                        Point destSize = new Point(128, 128);
                        Rectangle? sourceRect = null;
                        float offsetX = 0;
                        float offsetY = 0;
                        float rot = 0;
                        int maxRange = 48;
                        int rangeX = (int)Math.Abs(p.X - pos.X);
                        int rangeY = (int)Math.Abs(p.Y - pos.Y);
                        if(ed % 2 == 1)
                        {
                            if (d == 1 && rangeY < 32 && rangeX < maxRange)
                            {
                                sourceRect = new Rectangle(0, 0, 64 - maxRange + rangeX, 64);
                                destSize = new Point(sourceRect.Value.Size.X * 2, sourceRect.Value.Size.Y * 2);
                            }
                            else if (d == 3 && rangeY < 32 && rangeX < maxRange)
                            {
                                sourceRect = new Rectangle(maxRange - rangeX, 0, maxRange + rangeX, 64);
                                offsetX = 2 * (rangeX - maxRange);
                                destSize = new Point(sourceRect.Value.Size.X * 2, sourceRect.Value.Size.Y * 2);
                            }
                        }
                        else
                        {
                            destSize = new Point(128, 64);
                        }
                        if (ed == 0)
                        {
                            rot = (float)Math.PI / 2f;
                            offsetX -= 96;
                            offsetY += 8;
                        }
                        else if (ed == 2)
                        {
                            rot = (float)Math.PI * 3 / 2f;
                            offsetX += 96;
                            offsetY -= 118;
                        }
                        b.Draw(rainbowTexture, new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(p) - new Vector2(32f + offsetX - (ed == 2 ? 128 : 0), 88 + offsetY)), destSize), sourceRect, Color.White * (0.5f - (max - i) / max / 2),  rot, Vector2.Zero, SpriteEffects.None, p.Y / 10000f - 0.005f + (d == 0 ? 0.0067f : 0) - (max + 1 - i) / 10000f);
                    }
                }
                if((!active && set.Count > 0) || set.Count > max)
                    set.Dequeue();
                if (!active)
                {
                    if(set.Count > 0)
                        set.Dequeue();
                    else
                        trailDict.Remove(__instance.UniqueMultiplayerID);
                }
            }
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AreaOfEffect
{
    public class CastSpellMenu : IClickableMenu
    {

        private Vector2? tile;
        private Tool tool;
        private List<string> spellTypes;
        private Vector2 lastPos;
        private List<Vector2> dots = new();
        private List<Vector2> predots = new();
        private List<int> ends = new();
        private List<SpellDirection> directions = new();
        private SpellDirection currentDirection = SpellDirection.None;
        private int lastOnTrackDot;
        private bool spelling;
        private bool opened;

        public CastSpellMenu(Tool t, List<string> types, Vector2? v = null)
        {
            tile = v;
            tool = t;
            spellTypes = types;
            dots = new();
            ends = new();
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            if (directions.Any())
            {
                //for (int i = 0; i < directions.Count; i++)
                //{
                //    b.DrawString(Game1.dialogueFont, directions[i].ToString(), new Vector2(0, i * 64), Color.White);
                //}
            }
            if (spelling)
            {
                //predots.Clear();
                var spell = GetSpell(true);
                if (spell.Value != null)
                {
                    SpriteText.drawStringWithScrollCenteredAt(b, spell.Value.DisplayName, Game1.viewport.Width / 2, Game1.viewport.Height - 128);
                }
            }
            else
            {
                
            }
            if (ModEntry.Config.ExtraDrawEffects)
            {
                var pos = Game1.getMousePosition().ToVector2();
                int c = 20;
                predots.Add(pos);
                if (predots.Count > c)
                {
                    predots.RemoveAt(0);
                }
                for (int i = 0; i < predots.Count; i++)
                {
                    float alpha = (i + 1) / (c * 2f);
                    float scale = 1 / alpha / 2;
                    if (i > 0)
                    {
                        var distance = Vector2.Distance(predots[i], predots[i - 1]);
                        if (distance > 20)
                        {
                            float f = 10f;
                            while (f < distance)
                            {
                                b.Draw(Game1.mouseCursors, Vector2.Lerp(predots[i - 1], predots[i], f / distance) - new Vector2(60, 60) * scale, new Rectangle(88, 1779, 30, 30), Color.White * alpha, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, 1f);
                                f += 20;
                            }
                        }
                    }
                    b.Draw(Game1.mouseCursors, predots[i] - new Vector2(60, 60) * scale, new Rectangle(88, 1779, 30, 30), Color.White * alpha, 0, Vector2.Zero, 4f * scale, SpriteEffects.None, 1f);
                }
            }

            //b.DrawString(Game1.dialogueFont, currentDirection.ToString(), new Vector2(0, directions.Count * 64), Color.White);
            if (dots.Any())
            {
                int count = 0;
                for (int i = 0; i < dots.Count; i++)
                {
                    if(i > 0)
                    {
                        var distance = Vector2.Distance(dots[i], dots[i - 1]);
                        if(distance > 30)
                        {
                            float f = 15f;
                            while(f < distance)
                            {
                                b.Draw(Game1.mouseCursors, Vector2.Lerp(dots[i - 1], dots[i], f / distance) - new Vector2(60, 60), new Rectangle(88, 1779, 30, 30), GetColor(count), 0, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                                f += 30;
                            }
                        }
                    }
                    b.Draw(Game1.mouseCursors, dots[i] - new Vector2(60, 60), new Rectangle(88, 1779, 30, 30), GetColor(count), 0, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    if (ends.Count > count && ends[count] <= i)
                    {
                        count++;
                    }
                }
            }
            drawMouse(b);
        }

        private Color GetColor(int i)
        {
            var idx = directions.Count <= i ? (int)currentDirection :(int)directions[i];
            return ModEntry.GetDirectionColor(idx);
        }

        public override void update(GameTime time)
        {
            int minDot = 16;
            int minSegment = 256;
            if (ModEntry.SHelper.Input.IsDown(SButton.MouseLeft))
            {
                if (!opened)
                {
                    return;
                }
                if (!spelling)
                {
                    spelling = true;
                    lastPos = Game1.getMousePosition().ToVector2();
                    dots.Clear();
                    ends.Clear();
                    directions.Clear();
                    dots.Add(lastPos);
                    lastOnTrackDot = 0;
                    currentDirection = SpellDirection.None;
                    return;
                }
            }
            else
            {
                if(!opened)
                {
                    opened = true;
                    return;
                }
                if (spelling && directions.Any())
                {
                    if(currentDirection != directions.Last())
                    {
                        directions.Add(currentDirection);
                    }
                    spelling = false;
                    CheckSpell();
                }
                spelling = false;
                dots.Clear();
                ends.Clear();
                directions.Clear();
                lastOnTrackDot = 0;
                currentDirection = SpellDirection.None;
                return;
            }
            var newPos = Game1.getMousePosition().ToVector2();
            var distance = Vector2.Distance(lastPos, newPos);
            if (distance < minDot)
                return;
            lastPos = newPos;
            dots.Add(newPos);

            SpellDirection newDirection = GetDirection(newPos - dots[lastOnTrackDot]);
            if (newDirection == currentDirection)
            {
                lastOnTrackDot = dots.Count - 1;
                return;
            }
            distance = Vector2.Distance(dots[lastOnTrackDot], newPos);
            if (distance >= minSegment)
            {
                if(currentDirection != SpellDirection.None)
                {
                    Game1.playSound("cowboy_explosion");
                    directions.Add(currentDirection);
                    ends.Add(lastOnTrackDot);
                }
                lastOnTrackDot = dots.Count - 1;
                currentDirection = newDirection;
            }
        }

        private SpellDirection GetDirection(Vector2 v)
        {
            var angle = Math.Atan2(v.X, -v.Y);
            if (angle < 0)
                angle += Math.PI * 2;
            if(angle < Math.PI / 8f)
            {
                return SpellDirection.Up;
            }
            if(angle < Math.PI * 3f / 8f)
            {
                return SpellDirection.UpRight;
            }
            if(angle < Math.PI * 5f / 8f)
            {
                return SpellDirection.Right;
            }
            if(angle < Math.PI * 7f / 8f)
            {
                return SpellDirection.DownRight;
            }
            if(angle < Math.PI * 9f / 8f)
            {
                return SpellDirection.Down;
            }
            if(angle < Math.PI * 11f / 8f)
            {
                return SpellDirection.DownLeft;
            }
            if(angle < Math.PI * 13f / 8f)
            {
                return SpellDirection.Left;
            }
            if(angle < Math.PI * 15f / 8f)
            {
                return SpellDirection.UpLeft;
            }
            return SpellDirection.Up;
        }

        private void CheckSpell()
        {
            if (!directions.Any())
                return;

            var spell = GetSpell(false);
            if(spell.Value != null)
            {
                var sound = spell.Value.SetSound;
                if (!string.IsNullOrEmpty(sound))
                    Game1.playSound(sound);

                ModEntry.SetEffect(tool, spell.Key);
                Game1.activeClickableMenu = null;
            }
        }

        private KeyValuePair<string, SpellData> GetSpell(bool spelling)
        {
            var dir = directions.ToList();
            if (spelling)
            {
                dir.Add(currentDirection);
            }
            foreach (var kvp in ModEntry.SpellDict.Where(p => spellTypes == null || spellTypes.Contains(p.Key)))
            {
                if (kvp.Value.Sequence == null)
                    continue;
                if (kvp.Value.Sequence.Count == dir.Count)
                {
                    for (int i = 0; i < dir.Count; i++)
                    {
                        if (kvp.Value.Sequence[i] != dir[i])
                        {
                            goto next;
                        }
                    }
                    return kvp;
                next:
                    continue;
                }
            }
            return default;
        }
    }
}
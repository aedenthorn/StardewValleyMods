using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Clocks
{
    public partial class ModEntry
    {
        public static float lastHours;

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Furniture_draw_Patch
        {
            public static void Postfix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !ClockDict.TryGetValue(__instance.ItemId, out var data))
                    return;
                var minuteHand = data.MinuteHand is null ? Game1.staminaRect : SHelper.GameContent.Load <Texture2D>(data.MinuteHand);
                var hourHand = data.HourHand is null ? Game1.staminaRect : SHelper.GameContent.Load <Texture2D>(data.HourHand);
                var nub = data.Nub is null ? Game1.staminaRect : SHelper.GameContent.Load <Texture2D>(data.Nub);

                var hourHandSourceRect = data.HourHandSourceRect ?? new Rectangle(0, 0, 2, 5);
                var minuteHandSourceRect = data.MinuteHandSourceRect ?? new Rectangle(0, 0, 2, 8);
                var nubSourceRect = data.NubSourceRect ?? new Rectangle(0, 0, 2, 2);
                
                var hourHandSize = new Point((int)Math.Round(hourHandSourceRect.Size.X * data.Scale), (int)Math.Round(hourHandSourceRect.Size.Y * data.Scale));
                var minuteHandSize = new Point((int)Math.Round(minuteHandSourceRect.Size.X * data.Scale), (int)Math.Round(minuteHandSourceRect.Size.Y * data.Scale));
                var nubSize = new Point((int)Math.Round(nubSourceRect.Size.X * data.Scale), (int)Math.Round(nubSourceRect.Size.Y * data.Scale));

                var hourOrigin = new Vector2(hourHandSourceRect.Size.X / 2f, hourHandSourceRect.Size.Y);
                var minuteOrigin = new Vector2(minuteHandSourceRect.Size.X / 2f, minuteHandSourceRect.Size.Y);

                alpha *= (data.Alpha ?? Config.Alpha);

                var pos = (Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(__instance.TileLocation.X * 64), (float)(__instance.TileLocation.Y * 64))) + data.DrawOffset).ToPoint();
                var layer = (__instance.furniture_type.Value == 12) ? (2E-09f + __instance.TileLocation.Y / 100000f) : ((float)(__instance.boundingBox.Value.Bottom - ((__instance.furniture_type.Value == 6 || __instance.furniture_type.Value == 17 || __instance.furniture_type.Value == 13) ? 48 : 8)) / 10000f);

                float mins = Game1.timeOfDay % 100 + ((float)Game1.gameTimeInterval / Game1.realMilliSecondsPerGameTenMinutes) * 10;
                spriteBatch.Draw(hourHand, new Rectangle(pos, hourHandSize), hourHandSourceRect, (data.HourHandColor ?? Config.HourHandColor) * alpha, GetHourRotation(), hourOrigin, SpriteEffects.None, layer + 0.0001f);
                spriteBatch.Draw(minuteHand, new Rectangle(pos, minuteHandSize), minuteHandSourceRect, (data.MinuteHandColor ?? Config.MinuteHandColor) * alpha, GetMinRotation(), minuteOrigin, SpriteEffects.None, layer + 0.00011f);
                spriteBatch.Draw(nub, new Rectangle(pos - new Point(nubSize.X / 2, nubSize.Y / 2), nubSize), nubSourceRect, (data.NubColor ?? Config.NubColor) * alpha, 0f, Vector2.Zero, SpriteEffects.None, layer + 0.00012f);
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.draw))]
        public class Building_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Building.draw");

                var codes = new List<CodeInstruction>(instructions);
                bool foundHours = false;
                bool fixedHours = false;
                bool foundMins = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(Game1), nameof(Game1.realMilliSecondsPerGameTenMinutes)))
                    {
                        if (foundHours)
                        {
                            foundMins = true;
                        }
                        else
                        {
                            foundHours = true;
                        }
                        continue;
                    }
                    if (codes[i].opcode == OpCodes.Newobj && codes[i].operand is ConstructorInfo mi && mi == AccessTools.Constructor(typeof(Vector2), new Type[] { typeof(float), typeof(float) }))
                    {
                        if (foundMins)
                        {
                            SMonitor.Log($"Adding min switch");
                            codes.Insert(i - 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(GetMinRotationForTranspiler))));
                            break;
                        }
                        else if (foundHours && !fixedHours)
                        {
                            SMonitor.Log($"Adding hour switch");
                            codes.Insert(i - 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(GetHourRotationForTranspiler))));
                            fixedHours = true;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawAboveAlwaysFrontLayer))]
        public class GameLocation_drawAboveAlwaysFrontLayer_Patch
        {

            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;
                if (!clockLocationCache.TryGetValue(__instance.NameOrUniqueName, out var list))
                {
                    list = MapClockDict.Values.Where(d => d.GameLocation == __instance.NameOrUniqueName).ToList();
                    clockLocationCache[__instance.NameOrUniqueName] = list;
                }
                foreach (var data in list)
                {
                    var background = data.Background is null ? null : SHelper.GameContent.Load<Texture2D>(data.Background);
                    var minuteHand = data.MinuteHand is null ? Game1.staminaRect : SHelper.GameContent.Load<Texture2D>(data.MinuteHand);
                    var hourHand = data.HourHand is null ? Game1.staminaRect : SHelper.GameContent.Load<Texture2D>(data.HourHand);
                    var nub = data.Nub is null ? Game1.staminaRect : SHelper.GameContent.Load<Texture2D>(data.Nub);

                    var backgroundSourceRect = data.BackgroundSourceRect ?? (background is null ? new() : new Rectangle(0, 0, background.Width, background.Height));
                    var hourHandSourceRect = data.HourHandSourceRect ?? new Rectangle(0, 0, 2, 5);
                    var minuteHandSourceRect = data.MinuteHandSourceRect ?? new Rectangle(0, 0, 2, 8);
                    var nubSourceRect = data.NubSourceRect ?? new Rectangle(0, 0, 2, 2);

                    var backgroundSize = background is null ? Point.Zero : new Point((int)Math.Round(backgroundSourceRect.Size.X * data.Scale), (int)Math.Round(backgroundSourceRect.Size.Y * data.Scale));
                    var hourHandSize = new Point((int)Math.Round(hourHandSourceRect.Size.X * data.Scale), (int)Math.Round(hourHandSourceRect.Size.Y * data.Scale));
                    var minuteHandSize = new Point((int)Math.Round(minuteHandSourceRect.Size.X * data.Scale), (int)Math.Round(minuteHandSourceRect.Size.Y * data.Scale));
                    var nubSize = new Point((int)Math.Round(nubSourceRect.Size.X * data.Scale), (int)Math.Round(nubSourceRect.Size.Y * data.Scale));

                    var hourOrigin = new Vector2(hourHandSourceRect.Size.X / 2f, hourHandSourceRect.Size.Y);
                    var minuteOrigin = new Vector2(minuteHandSourceRect.Size.X / 2f, minuteHandSourceRect.Size.Y);

                    var alpha = (data.Alpha ?? Config.Alpha);
                    var tile = data.Tile;
                    var pos = (Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(tile.X * 64), (float)(tile.Y * 64))) + data.DrawOffset).ToPoint();
                    var layer = 0.98f;

                    float mins = Game1.timeOfDay % 100 + ((float)Game1.gameTimeInterval / Game1.realMilliSecondsPerGameTenMinutes) * 10;
                    if (background != null)
                    {
                        b.Draw(background, new Rectangle(pos.X - backgroundSize.X / 2 + data.BackgroundOffset.X, pos.Y - backgroundSize.Y / 2 + data.BackgroundOffset.Y, backgroundSize.X, backgroundSize.Y), backgroundSourceRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layer + 0.0001f);
                    }
                    b.Draw(hourHand, new Rectangle(pos, hourHandSize), hourHandSourceRect, (data.HourHandColor ?? Config.HourHandColor) * alpha, GetHourRotation(), hourOrigin, SpriteEffects.None, layer + 0.00011f);
                    b.Draw(minuteHand, new Rectangle(pos, minuteHandSize), minuteHandSourceRect, (data.MinuteHandColor ?? Config.MinuteHandColor) * alpha, GetMinRotation(), minuteOrigin, SpriteEffects.None, layer + 0.00012f);
                    b.Draw(nub, new Rectangle(pos - new Point(nubSize.X / 2, nubSize.Y / 2), nubSize), nubSourceRect, (data.NubColor ?? Config.NubColor) * alpha, 0f, Vector2.Zero, SpriteEffects.None, layer + 0.00013f);

                }
            }
        }
        [HarmonyPatch(typeof(Town), nameof(Town.drawAboveAlwaysFrontLayer))]
        public class Town_drawAboveAlwaysFrontLayer_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Town.drawAboveAlwaysFrontLayer");

                var codes = new List<CodeInstruction>(instructions);
                bool foundHours = false;
                bool fixedHours = false;
                bool foundMins = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo fi && fi == AccessTools.Field(typeof(Game1), nameof(Game1.realMilliSecondsPerGameTenMinutes)))
                    {
                        if (foundHours)
                        {
                            foundMins = true;
                        }
                        else
                        {
                            foundHours = true;
                        }
                        continue;
                    }
                    if (codes[i].opcode == OpCodes.Newobj && codes[i].operand is ConstructorInfo mi && mi == AccessTools.Constructor(typeof(Vector2), new Type[] { typeof(float), typeof(float) }))
                    {
                        if (foundMins)
                        {
                            SMonitor.Log($"Adding min switch");
                            codes.Insert(i - 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(GetMinRotationForTranspiler))));
                            break;
                        }
                        else if (foundHours && !fixedHours)
                        {
                            SMonitor.Log($"Adding hour switch");
                            codes.Insert(i - 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(GetHourRotationForTranspiler))));
                            fixedHours = true;
                        }
                    }
                }
                return codes.AsEnumerable();
            }
        }
    }
}
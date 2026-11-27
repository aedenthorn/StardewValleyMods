using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StardewValley.Locations;

namespace DeathTweaks
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Event), nameof(Event.command_hospitaldeath))]
        public class Event_command_hospitaldeath_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Event.command_hospitaldeath");
                var codes = new List<CodeInstruction>(instructions);
                bool found1 = false;
                bool found2 = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand is MethodInfo && (MethodInfo)codes[i + 1].operand == AccessTools.Method(typeof(Random), nameof(Random.NextDouble)) && codes[i+3].opcode == OpCodes.Bge_Un_S)
                    {
                        SMonitor.Log($"adding modifier to drop everything on death");
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckDropEverything))));
                        found1 = true;
                    }
                    else if (!found2 && i < codes.Count - 3 && codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Money)) && codes[i+2].opcode == OpCodes.Sub)
                    {
                        SMonitor.Log($"adding modifier to not lose money on death");
                        var getLoc = new CodeInstruction(codes[i + 1].opcode, codes[i + 1].operand);
                        var setLoc = new CodeInstruction(codes[i - 3].opcode, codes[i - 3].operand);
                        codes.Insert(i - 2, setLoc);
                        codes.Insert(i - 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(SetMoneyLost))));
                        codes.Insert(i - 2, getLoc);
                        found2 = true;
                    }
                    if (found1 && found2)
                        break;
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Event), nameof(Event.command_minedeath))]
        public class Event_command_minedeath_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Event.command_hospitaldeath");
                var codes = new List<CodeInstruction>(instructions);
                bool found1 = false;
                bool found2 = false;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Callvirt && codes[i + 1].operand is MethodInfo && (MethodInfo)codes[i + 1].operand == AccessTools.Method(typeof(Random), nameof(Random.NextDouble)) && codes[i + 3].opcode == OpCodes.Bge_Un_S)
                    {
                        SMonitor.Log($"adding modifier to drop everything on death");
                        codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckDropEverything))));
                        found1 = true;
                    }
                    else if (!found2 && i < codes.Count - 3 && codes[i].opcode == OpCodes.Stloc_1 && codes[i + 2].opcode == OpCodes.Stloc_2)
                    {
                        SMonitor.Log($"adding modifier to not lose money on death");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Stloc_1));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(SetMoneyLost))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldloc_1));
                        found2 = true;
                    }
                    if (found1 && found2)
                        break;
                }
                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Event), nameof(Event.command_showItemsLost))]
        public class Event_command_showItemsLost_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;
                CheckCreateChest();
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.warpFarmer), new Type[] { typeof(string), typeof(int), typeof(int), typeof(bool) })]
        public class Game1_warpFarmer_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled || !Game1.killScreen)
                    return;
                deathData.Value = new DeathData()
                {
                    location = Game1.player.currentLocation,
                    position = Game1.player.getTileLocation()
                };
            }
        }
        [HarmonyPatch(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Crop_draw_Patch
        {
            public static bool Prefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha, int ___currentLidFrame)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(modKey))
                    return true;
                Texture2D texture = SHelper.GameContent.Load<Texture2D>(Config.TombStonePath);
                float draw_x = (float)x;
                float draw_y = (float)y;
                float base_sort_order = Math.Max(0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;

                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y) * 64f)) + Config.TombStoneOffset, Config.TombStoneRect, Color.White, 0f, Vector2.Zero, Config.TombStoneScale, SpriteEffects.None, base_sort_order);
                return false;
            }
        }
        [HarmonyPatch(typeof(Chest), nameof(Chest.dumpContents))]
        public class Chest_dumpContents_Patch
        {
            public static void Postfix(Chest __instance, GameLocation location)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(modKey))
                    return;
                deathData.Value = null;
            }
        }
    }
}
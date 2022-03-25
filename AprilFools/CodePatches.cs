
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AprilFools
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(InventoryMenu), nameof(InventoryMenu.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) })]
        public class InventoryMenu_draw
        {
            private static int lastSlotNumber = -1;
            private static int avoidTicks = 0;
            public static void Prefix(InventoryMenu __instance)
            {
                if (!Config.EnableMod)
                    return;
                avoidTicks++;
                foreach (ClickableComponent clickableComponent in __instance.inventory)
                {
                    int slotNumber = Convert.ToInt32(clickableComponent.name);
                    if (clickableComponent.containsPoint(Game1.getMouseX(), Game1.getMouseY()) && slotNumber < __instance.actualInventory.Count && (__instance.actualInventory[slotNumber] == null || __instance.highlightMethod(__instance.actualInventory[slotNumber])) && slotNumber < __instance.actualInventory.Count && __instance.actualInventory[slotNumber] != null)
                    {
                        if (slotNumber == lastSlotNumber || avoidTicks < 30)
                            return;
                        avoidTicks = 0;
                        lastSlotNumber = slotNumber;
                        if (Game1.random.NextDouble() < 0.5)
                            return;

                        MoveToRandomSlot(ref __instance.actualInventory, slotNumber);

                    }
                }
            }

            private static void MoveToRandomSlot(ref IList<Item> inv, int slot)
            {
                List<int> slots = new List<int>() { slot - 1, slot + 1, slot - 12, slot + 12 };
                foreach(var i in slots){
                    if(i > 0 && i < inv.Count && inv[i] == null)
                    {
                        inv[i] = inv[slot];
                        inv[slot] = null;
                        Game1.playSound("dwop");
                        return;
                    }
                }
            }
        }
        private static int screamTicks = 0;
        [HarmonyPatch(typeof(Tree), nameof(Tree.performToolAction))]
        public class Tree_performToolAction
        {
            public static bool Prefix(Tree __instance, Tool t, Vector2 tileLocation, GameLocation location, ref float __state)
            {
                if (!Config.EnableMod)
                    return true;
                __state = __instance.health.Value;
                if (Game1.random.NextDouble() < 0.25 && t is Axe)
                {
                    var list = Utility.getSurroundingTileLocationsArray(tileLocation).ToList();
                    ShuffleList(list);
                    foreach (var tile in list)
                    {
                        var x = t.getLastFarmerToUse().GetBoundingBox();
                        var y = tile * 64;
                        if (t.getLastFarmerToUse().GetBoundingBox().Intersects(new Rectangle((int)tile.X * 64,(int)tile.Y * 64, 64, 64)))
                            continue;
                        if (!location.terrainFeatures.ContainsKey(tile) && !location.objects.ContainsKey(tile) && location.map.GetLayer("Buildings").PickTile(new Location((int)tile.X * 64, (int)tile.Y * 64), Game1.viewport.Size) == null)
                        {
                            location.terrainFeatures[tile] = __instance;
                            location.terrainFeatures.Remove(tileLocation);
                            Game1.playSound("leafrustle");
                            return false;
                        }

                    }
                }
                return true;
                    
            }
            public static void Postfix(Tree __instance, ref float __state)
            {
                if (!Config.EnableMod || __state <= __instance.health.Value)
                    return;
                if(__instance.health.Value > 0 && screamTicks > 120)
                {
                    screamTicks = 0;
                    __instance.modData["aedenthorn.AprilFools/speech"] = SHelper.Translation.Get("tree-chop-" + Game1.random.Next(1, 8));
                    __instance.modData["aedenthorn.AprilFools/timer"] = "60/60";
                }
                else if (__instance.health.Value <= 0)
                {
                    __instance.modData.Remove("aedenthorn.AprilFools/speech");
                }
            }
        }
        [HarmonyPatch(typeof(Tree), nameof(Tree.draw), new Type[] { typeof(SpriteBatch), typeof(Vector2) })]
        public class Tree_draw
        {
            public static void Postfix(Tree __instance, SpriteBatch spriteBatch, Vector2 tileLocation, NetBool ___falling)
            {
                if (!Config.EnableMod)
                    return;
                if(screamTicks <= 120)
                    screamTicks++;
                __instance.modData.TryGetValue("aedenthorn.AprilFools/speech", out string speech);
                if (!___falling.Value && speech == null)
                    return;

                if (___falling.Value && speech == null)
                {
                    speech = SHelper.Translation.Get("tree-fall-" + Game1.random.Next(1, 5));
                    __instance.modData["aedenthorn.AprilFools/speech"] = speech;
                    __instance.modData["aedenthorn.AprilFools/timer"] = "120/120";
                }

                Vector2 local = Game1.GlobalToLocal(tileLocation * 64 + new Vector2(32, ___falling.Value || __instance.stump.Value ? -128 : -192));
                string[] timerString = __instance.modData["aedenthorn.AprilFools/timer"].Split('/');
                int.TryParse(timerString[0], out int timer);
                int.TryParse(timerString[1], out int timerMax);
                timer--;
                if (timer <= 0)
                {
                    __instance.modData.Remove("aedenthorn.AprilFools/speech");
                    return;
                }
                __instance.modData["aedenthorn.AprilFools/timer"] = $"{timer}/{timerMax}";
                float alpha = 1;
                if (timer < 5)
                    alpha = timer / 5f;
                else if (timer > timerMax - 5)
                    alpha = timerMax - timer / 5f;
                SpriteText.drawStringWithScrollCenteredAt(spriteBatch, speech, (int)local.X, (int)local.Y, "", alpha, -1, 1, 1, false);
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.MovePosition))]
        public static class Farmer_MovePosition_Patch
        {
            public static void Prefix(Farmer __instance)
            {
                if (!Config.EnableMod)
                    return;

                if (backwardsFarmer)
                {
                    if(__instance.movementDirections.Contains(0) && !__instance.movementDirections.Contains(2))
                    {
                        __instance.movementDirections.Remove(0);
                        __instance.movementDirections.Add(2);
                    }
                    else if(__instance.movementDirections.Contains(2) && !__instance.movementDirections.Contains(0))
                    {
                        __instance.movementDirections.Remove(2);
                        __instance.movementDirections.Add(0);
                    }
                    if(__instance.movementDirections.Contains(1) && !__instance.movementDirections.Contains(3))
                    {
                        __instance.movementDirections.Remove(1);
                        __instance.movementDirections.Add(3);
                    }
                    else if(__instance.movementDirections.Contains(3) && !__instance.movementDirections.Contains(1))
                    {
                        __instance.movementDirections.Remove(3);
                        __instance.movementDirections.Add(1);
                    }
                }
            }
        }
        //[HarmonyPatch(typeof(InputState), nameof(InputState.SetMousePosition))]
        public class SetMousePosition_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FruitTree.draw");
                var codes = new List<CodeInstruction>(instructions);
                bool found1 = false;
                int which = 0;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (!found1 && i < codes.Count - 2 && codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i + 1].opcode == OpCodes.Ldc_R4 && (float)codes[i + 1].operand == 1E-07f)
                    {
                        SMonitor.Log("shifting bottom of tree draw layer offset");
                        codes[i + 1].opcode = OpCodes.Ldarg_0;
                        found1 = true;
                    }
                    if (i > 0 && i < codes.Count - 15 && codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(Game1), nameof(Game1.objectSpriteSheet)) && codes[i + 15].opcode == OpCodes.Call && (MethodInfo)codes[i + 15].operand == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)))
                    {
                        SMonitor.Log("modifying fruit color");
                        codes[i + 15].opcode = OpCodes.Call;
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldc_I4, which++));
                        codes.Insert(i + 15, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                    if (found1 && which >= 2)
                        break;
                }

                return codes.AsEnumerable();
            }
        }

    }
}